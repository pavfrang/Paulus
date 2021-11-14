using Paulus.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Paulus.Serial.InfusionPump;

namespace Paulus.Serial.Dropletino
{
    //objects used in Acidino project

    public class AutomationSettings
    {
        public int SensorsOnDurationInSeconds { get; set; } //3*60 sec
        public int SensorsOffDurationInSeconds { get; set; } //3*60 sec
        public int MaximumLoops { get; set; }
    }

    public enum AutomationStep
    {
        [Description("Sensors ON")]
        SensorsOn,
        [Description("Sensors OFF")]
        SensorsOff,
    }

    public class SensorRelayCommander : SerialDevice
    {
        public SensorRelayCommander(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
            : this(new SerialPort(portName, baudRate, parity, dataBits, stopBits)) { }

        public SensorRelayCommander(SerialPort port)
            : base(port, ResponseMode.Immediate)
        {
            SendMessageSuffix = "\n";
            ReceiveMessageSuffix = "\r\n";
            SensorRelays = new SensorRelays(false);

            serialPort.WriteTimeout = 400;
            serialPort.ReadTimeout = 400;

        }

        public SensorRelays SensorRelays;

        //infusion pump commander must be externally set  in order for the automation to work!
        public InfusionPumpCommander InfusionPumpCommander;

        #region Send commands

        public override bool Reset()
        {
            SimpleSerialCommandWithResponse<string> response = Reset2().Result;
            return response.Success;
        }

        public async Task<SimpleSerialCommandWithResponse<string>> Reset2()
        {
            SimpleSerialCommandWithResponse<string> response = await SerialCommands.CreateWithStringResponse("RESET").SendAndGetCommand(this);

            if (response.Success)
            {
                SensorRelays.Sensor1Relay.ActualValue = 0.0f;
                SensorRelays.Sensor2Relay.ActualValue = 0.0f;
            }
            return response;
        }

        #region ON/OFF commands

        public async Task<SimpleSerialCommandWithResponse<string>> Sensor1On()
        {
            SimpleSerialCommandWithResponse<string> response =
                await SerialCommands.CreateWithStringResponse("S1 ON").SendAndGetCommand(this);
            if (response.Success)
                SensorRelays.Sensor1Relay.ActualValue = 1.0f;
            return response;
        }


        public async Task<SimpleSerialCommandWithResponse<string>> Sensor1Off()
        {
            SimpleSerialCommandWithResponse<string> response =
                await SerialCommands.CreateWithStringResponse("S1 OFF").SendAndGetCommand(this);
            if (response.Success)
                SensorRelays.Sensor1Relay.ActualValue = 0.0f;
            return response;
        }

        public async Task<SimpleSerialCommandWithResponse<string>> Sensor2On()
        {
            SimpleSerialCommandWithResponse<string> response =
                await SerialCommands.CreateWithStringResponse("S2 ON").SendAndGetCommand(this);
            if (response.Success)
                SensorRelays.Sensor2Relay.ActualValue = 1.0f;
            return response;
        }


        public async Task<SimpleSerialCommandWithResponse<string>> Sensor2Off()
        {
            SimpleSerialCommandWithResponse<string> response =
                await SerialCommands.CreateWithStringResponse("S2 OFF").SendAndGetCommand(this);
            if (response.Success)
                SensorRelays.Sensor2Relay.ActualValue = 0.0f;
            return response;
        }

        #endregion

        #endregion

        public AutomationSettings AutomationSettings { get; set; } = new AutomationSettings();

        public Automation<AutomationStep> Automation { get; private set; }

        public void StartAutomation()
        {
            Automation = new Automation<AutomationStep>();

            //set automation settings
            Automation.MaximumLoops = AutomationSettings.MaximumLoops;
            Automation.AutomationStepDurationsInSeconds = new Dictionary<AutomationStep, float> {
                { AutomationStep.SensorsOff, AutomationSettings.SensorsOffDurationInSeconds},
                { AutomationStep.SensorsOn, AutomationSettings.SensorsOnDurationInSeconds}
            };

            //add the main automation handler
            Automation.AutomationStepChanged += Automation_AutomationStepChanged;
            Automation.Started += Automation_Started;
            Automation.Finished += Automation_Finished;
            Automation.Paused += Automation_Paused;
            Automation.Resumed += Automation_Resumed;

            Automation.Start();
        }

        private async void Automation_Paused(object sender, EventArgs e)
        {
            await InfusionPumpCommander.Stop();
        }

        private async void Automation_Resumed(object sender, EventArgs e)
        {
            await InfusionPumpCommander.Start();
        }

        private async void Automation_Finished(object sender, EventArgs e)
        {
            await InfusionPumpCommander.Stop();
        }

        private async void Automation_Started(object sender, EventArgs e)
        {
            //InfusionPumpCommander.SetInfusionRate();
            //InfusionPumpCommander.SetSyringe();
            //InfusionPumpCommander.SetTargetVolume();
            await InfusionPumpCommander.Start();
        }

        private async void Automation_AutomationStepChanged(object sender, EventArgs e)
        {
            switch (Automation.CurrentAutomationStep)
            {
                case AutomationStep.SensorsOff:
                    await Sensor1Off();
                    await Sensor2Off();
                    break;
                case AutomationStep.SensorsOn:
                    await Sensor1On();
                    await Sensor2On();
                    break;
            }
        }
    }

}
