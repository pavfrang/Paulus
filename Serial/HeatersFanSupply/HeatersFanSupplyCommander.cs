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

namespace Paulus.Serial.HeatersFanSupply
{
    //objects used in Acidino project

    public class AutomationSettings
    {
        public int HeaterSupplySteps { get; set; } //21
        public int HeatersOnSupplyOffTimeInSeconds { get; set; } //3600-10 sec
        public int HeatersOnSupplyOnTimeInSeconds { get; set; } //10 sec
        public int FanTimeInSeconds { get; set; } //1*3600 sec
        public int EverythingOffTimeInSeconds { get; set; } //10 sec
    }

    public enum AutomationStep
    {
        Paused,
        Started,
        [Description("Heaters ON, Supply OFF")]
        HeatersOnSupplyOff,
        [Description("Heaters ON, Supply ON")]
        HeatersOnSupplyOn,
        [Description("Heaters OFF, Fan ON")]
        HeatersOffFanOn,
        [Description("Everything OFF")]
        EverythingOff
    }

    public class HeatersFanSupplyCommander : SerialDevice
    {
        public HeatersFanSupplyCommander(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
            : this(new SerialPort(portName, baudRate, parity, dataBits, stopBits)) { }

        public HeatersFanSupplyCommander(SerialPort port)
            : base(port, ResponseMode.Immediate)
        {
            SendMessageSuffix = "\n";
            ReceiveMessageSuffix = "\r\n";
            HeatersFanSupply = new HeatersFanSupply(false);

            serialPort.WriteTimeout = 400;
            serialPort.ReadTimeout = 400;
        }

        public HeatersFanSupply HeatersFanSupply;

        public string LastCommandSent;
        public Queue<string> Responses = new Queue<string>();

        #region Send commands


        //SemaphoreSlim syncSemaphore = new SemaphoreSlim(1);
        public async Task<bool> Reset2()
        {
            //ReceiveMessage2Suffix = "\r\n";
            //bool sent = SendMessage("RESET");
            //LastRelayCommand = "RESET";

            //var response = await SerialCommands.CreateWithStringResponse("RESET").SendAndGetCommand(this);

            //if (response.Success)
            //{
            //    HeatersFanSupply.Heaters.ActualValue = 0.0f;
            //    HeatersFanSupply.Fan.ActualValue = 0.0f;
            //    HeatersFanSupply.Supply.ActualValue = 0.0f;
            //}
            //return response;
            await syncSemaphore.WaitAsync();
            try
            {
                LastCommandSent = "RESET";
                serialPort.WriteLine("RESET");
                string reply = await Task.Run(() => serialPort.ReadTo("\r\n"));
                Responses.Enqueue(reply);
                serialPort.ReadExisting(); //clear any buffers
                if (reply.Length > 0)
                {
                    HeatersFanSupply.Heaters.ActualValue = 0.0f;
                    HeatersFanSupply.Fan.ActualValue = 0.0f;
                    HeatersFanSupply.Supply.ActualValue = 0.0f;
                }
                return true;
            }
            catch { return false; }
            finally { syncSemaphore.Release(); }

        }


        #region ON/OFF commands

        private async Task<bool> sendCommand(string command, string expectedResponse, Action fSuccess)
        {

            await syncSemaphore.WaitAsync();
            try
            {
                LastCommandSent = command;
                serialPort.WriteLine(command);
                string reply = await Task.Run(() => serialPort.ReadTo("\r\n"));
                Responses.Enqueue(reply);

                bool success = reply == expectedResponse;
                if (success)
                    fSuccess();

                return success;
            }
            catch { return false; }
            finally { syncSemaphore.Release(); }
        }
        //public async Task<SimpleSerialCommandWithResponse<string>> SendHeatersOn()
        public async Task<bool> SendHeatersOn()
        {
            //ReceiveMessage2Suffix = "\r\n";
            //bool sent = SendMessage("HEATERS ON");
            //LastRelayCommand = "HEATERS ON";
            //if (sent) HeatersFanSupply.Heaters.ActualValue = 1.0f;
            //return sent;

            //var response = await SerialCommands.CreateWithStringResponse("HEATERS ON").SendAndGetCommand(this);
            //if (response.Success) 
            //    HeatersFanSupply.Heaters.ActualValue = 1.0f;
            //return response;

            return await sendCommand("HEATERS ON", "[HEATERS ON OK]",
                () => HeatersFanSupply.Heaters.ActualValue = 1.0f);
        }


        //public async Task<SimpleSerialCommandWithResponse<string>> SendHeatersOff()
        public async Task<bool> SendHeatersOff()
        {
            //ReceiveMessage2Suffix = "\r\n";
            //bool sent = SendMessage("HEATERS OFF");
            //LastRelayCommand = "HEATERS OFF";
            //if (sent) HeatersFanSupply.Heaters.ActualValue = 0.0f;
            //return sent;


            //var response = await SerialCommands.CreateWithStringResponse("HEATERS OFF").SendAndGetCommand(this);
            //if (response.Success)
            //    HeatersFanSupply.Heaters.ActualValue = 0.0f;
            //return response;

            return await sendCommand("HEATERS OFF", "[HEATERS OFF OK]",
                () => HeatersFanSupply.Heaters.ActualValue = 0.0f);
        }

        //public async Task<SimpleSerialCommandWithResponse<string>> SendFanOn()
        public async Task<bool> SendFanOn()
        {
            //ReceiveMessage2Suffix = "";
            //bool sent = SendMessage("FAN ON");
            //LastRelayCommand = "FAN ON";
            //if (sent) HeatersFanSupply.Fan.ActualValue = 1.0f;
            //return sent;

            //var response = await SerialCommands.CreateWithStringResponse("FAN ON").SendAndGetCommand(this);
            //if (response.Success)
            //    HeatersFanSupply.Fan.ActualValue = 1.0f;
            //return response;

            return await sendCommand("FAN ON", "[FAN ON OK]",
                () => HeatersFanSupply.Fan.ActualValue = 1.0f);
        }

        //public async Task<SimpleSerialCommandWithResponse<string>> SendFanOff()
        public async Task<bool> SendFanOff()
        {
            //ReceiveMessage2Suffix = "";
            //bool sent = SendMessage("FAN OFF");
            //LastRelayCommand = "FAN OFF";
            //if (sent) HeatersFanSupply.Fan.ActualValue = 0.0f;
            //return sent;


            //var response = await SerialCommands.CreateWithStringResponse("FAN OFF").SendAndGetCommand(this);
            //if (response.Success)
            //    HeatersFanSupply.Fan.ActualValue = 0.0f;
            //return response;

            return await sendCommand("FAN OFF", "[FAN OFF OK]",
                () => HeatersFanSupply.Fan.ActualValue = 0.0f);

        }

        //public async Task<SimpleSerialCommandWithResponse<string>> SendSupplyOn()
        public async Task<bool> SendSupplyOn()
        {
            //ReceiveMessage2Suffix = "";
            //bool sent = SendMessage("SUPPLY ON");
            //LastRelayCommand = "SUPPLY ON";
            //if (sent) HeatersFanSupply.Supply.ActualValue = 1.0f;
            //return sent;

            //var response = await SerialCommands.CreateWithStringResponse("SUPPLY ON").SendAndGetCommand(this);
            ////update the value
            //if (response.Success)
            //    HeatersFanSupply.Supply.ActualValue = 1.0f;
            //return response;

            return await sendCommand("SUPPLY ON", "[SUPPLY ON OK]",
                () => HeatersFanSupply.Supply.ActualValue = 1.0f);
        }
        //public async Task<SimpleSerialCommandWithResponse<string>> SendSupplyOff()
        public async Task<bool> SendSupplyOff()
        {
            //ReceiveMessage2Suffix = "";
            //bool sent = SendMessage("SUPPLY OFF");
            //LastRelayCommand = "SUPPLY OFF";
            //if (sent) HeatersFanSupply.Supply.ActualValue = 0.0f;
            //return sent;

            //var response = await SerialCommands.CreateWithStringResponse("SUPPLY OFF").SendAndGetCommand(this);
            ////update the value
            //if (response.Success)
            //    HeatersFanSupply.Supply.ActualValue = 0.0f;
            //return response;

            return await sendCommand("SUPPLY OFF", "[SUPPLY OFF OK]",
                () => HeatersFanSupply.Supply.ActualValue = 0.0f);
        }

        #endregion

        //public bool SendReadTemperature()
        //{
        //    ReceiveMessage2Suffix = "";
        //    return SendMessage("TEMP");
        //}

        //$"[TEMPERATURE]\t{temperature}"
        public async Task<float?> ReadTemperature()
        {
            await syncSemaphore.WaitAsync();
            try
            {
                LastCommandSent = "TEMP";
                serialPort.WriteLine("TEMP");
                string reply = await Task.Run(() => serialPort.ReadTo("\r\n"));
                Responses.Enqueue(reply);

                float temperature;
                bool parsed = float.TryParse(reply, out temperature);
                if (parsed)
                    HeatersFanSupply.Temperature.ActualValue = temperature;

                return float.Parse(reply);
            }
            catch { return null; }
            finally { syncSemaphore.Release(); }

            //var response = await SerialCommands.CreateWithFloatResponse("TEMP").SendAndGetCommand(this);
            ////update the value
            //if (!response.IsError)
            //    HeatersFanSupply.Temperature.ActualValue = response.Reply.Value;
            ////else MessageBox.Show("STOP!");
            //return response;
        }

        //public string LastRelayCommand { get; private set; }

        #endregion

        public AutomationSettings AutomationSettings { get; set; } = new AutomationSettings();

        private global::System.Windows.Forms.Timer tmrAutomation;

        private AutomationStep _currentAutomationStep;
        public AutomationStep CurrentAutomationStep
        {
            get { return _currentAutomationStep; }
            set
            {
                _currentAutomationStep = value;
                OnAutomationStepChanged();
            }

        }

        public DateTime StartAutomationTime { get; private set; }

        public TimeSpan ElapsedAutomationTime { get { return DateTime.Now - StartAutomationTime; } }

        public int LoopsCompleted { get; private set; }

        public bool StartAutomation()
        {
            StartAutomationTime = DateTime.Now;
            LoopsCompleted = 0;

            tmrAutomation = new global::System.Windows.Forms.Timer();
            tmrAutomation.Interval = 500;

            tmrAutomation.Tick += tmrAutomation_Tick;

            OnAutomationStarted();

            tmrAutomation.Start();
            return true;
        }

        public bool IsAutomationRunning { get { return tmrAutomation?.Enabled ?? false; } }

        public event EventHandler AutomationStarted;
        protected void OnAutomationStarted()
        {
            nextAutomationStepTime = DateTime.Now;
            CurrentAutomationStep = AutomationStep.Started;

            AutomationStarted?.Invoke(this, EventArgs.Empty);
        }

        //public event EventHandler AutomationStepChanging;
        //protected void OnAutomationStepChanging()
        //{
        //    AutomationStepChanging?.Invoke(this, EventArgs.Empty);
        //}

        public event EventHandler AutomationStepChanged;

        protected void OnAutomationStepChanged()
        {
            AutomationStepChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<TimeEventArgs> WaitingForNextStep;
        protected void OnWaitingForNextStep(TimeEventArgs e)
        {
            WaitingForNextStep?.Invoke(this, e);
        }

        public int CurrentHeaterSupplyStep { get; private set; }

        DateTime nextAutomationStepTime = DateTime.Now;
        public TimeSpan GetTimeUntilNextEvent { get { return nextAutomationStepTime - DateTime.Now; } }

        DateTime lastReadTemperatureTime = DateTime.Now;
        private async void tmrAutomation_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            //bool sent = false;
            if ((now - lastReadTemperatureTime).TotalSeconds >= 1.0)
            {
                //sent = SendReadTemperature(); if (!sent) SendReadTemperature();
                await ReadTemperature();
                lastReadTemperatureTime = now;
            }

            bool shouldChangeStep; // = false;
            shouldChangeStep = DateTime.Now >= nextAutomationStepTime;

            if (!shouldChangeStep)
            {
                OnWaitingForNextStep(new TimeEventArgs(nextAutomationStepTime - DateTime.Now));
                return;
            }

            //we come here if we should change the step!

            switch (CurrentAutomationStep)
            {
                case AutomationStep.Started:
                case AutomationStep.EverythingOff:
                    if (CurrentAutomationStep == AutomationStep.EverythingOff)
                        LoopsCompleted++;

                    //sent = SendFanOff(); if (!sent) SendFanOff();
                    //sent = SendHeatersOn(); ; if (!sent) SendHeatersOn();
                    await SendFanOff();
                    await SendHeatersOn();

                    CurrentHeaterSupplyStep = 1;
                    nextAutomationStepTime = DateTime.Now.AddSeconds(AutomationSettings.HeatersOnSupplyOffTimeInSeconds);
                    CurrentAutomationStep = AutomationStep.HeatersOnSupplyOff;
                    break;

                case AutomationStep.HeatersOnSupplyOff:
                    //sent = SendSupplyOn(); if (!sent) SendSupplyOn();
                    await SendSupplyOn();
                    nextAutomationStepTime = DateTime.Now.AddSeconds(AutomationSettings.HeatersOnSupplyOnTimeInSeconds);
                    CurrentAutomationStep = AutomationStep.HeatersOnSupplyOn;
                    break;

                case AutomationStep.HeatersOnSupplyOn:
                    //sent = SendSupplyOff(); if (!sent) SendSupplyOff();
                    await SendSupplyOff();
                    if (CurrentHeaterSupplyStep < AutomationSettings.HeaterSupplySteps)
                    {
                        nextAutomationStepTime = DateTime.Now.AddSeconds(AutomationSettings.HeatersOnSupplyOffTimeInSeconds);
                        CurrentAutomationStep = AutomationStep.HeatersOnSupplyOff;
                        CurrentHeaterSupplyStep++;
                    }
                    else //get to the fan stage
                    {
                        //sent = SendHeatersOff(); if (!sent) SendHeatersOff();
                        await SendHeatersOff();
                        //sent = SendFanOn(); if (!sent) SendFanOn();
                        await SendFanOn();
                        nextAutomationStepTime = DateTime.Now.AddSeconds(AutomationSettings.FanTimeInSeconds);
                        CurrentAutomationStep = AutomationStep.HeatersOffFanOn;
                    }
                    break;
                case AutomationStep.HeatersOffFanOn:
                    //just close the fan
                    await SendFanOff();
                    nextAutomationStepTime = DateTime.Now.AddSeconds(AutomationSettings.EverythingOffTimeInSeconds);
                    CurrentAutomationStep = AutomationStep.EverythingOff;
                    break;
            }

        }

        #region Pausing Automation

        TimeSpan timeUntilNextEvent;

        public void PauseAutomation()
        {
            //ignore the call if the automation timer is not enabled
            if (!tmrAutomation.Enabled) return;

            tmrAutomation.Stop();
            timeUntilNextEvent = nextAutomationStepTime - DateTime.Now;
            IsAutomationPaused = true;
        }

        public void ResumeAutomation()
        {
            //ignore the call if the automatio is not paused
            if (!IsAutomationPaused) return;

            IsAutomationPaused = false;
            nextAutomationStepTime = DateTime.Now.AddSeconds(timeUntilNextEvent.TotalSeconds);
            tmrAutomation.Start();
        }

        public bool IsAutomationPaused { get; private set; }

        public event EventHandler AutomationPaused;
        protected void OnAutomationPaused()
        {
            CurrentAutomationStep = AutomationStep.Paused;
            AutomationPaused?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Stopping Automation

        public async Task<bool> StopAutomation(bool closeAll)
        {
            tmrAutomation.Stop();
            if (closeAll)
                return (await SendFanOff()) &&
                    (await SendHeatersOff()) && (await SendSupplyOff());
            return true;
        }

        public event EventHandler AutomationStopped;
        protected void OnAutomationStopped()
        {
            CurrentAutomationStep = AutomationStep.Paused;

            AutomationStopped?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        //protected override void OnMessageReceived()
        //{
        //    if (LastSerialMessage == null) return;

        //    //it is more consistent to supply the message to the (non-simulated) device
        //    this.HeatersFanSupply.UpdateDeviceValuesBySerialMessage(LastSerialMessage.ReceivedFilteredMessage);

        //    //if (LastSerialMessage.ReceivedFilteredMessage.StartsWith("[TEMPERATURE]"))
        //    //{
        //    //    string message = LastSerialMessage.ReceivedFilteredMessage;
        //    //    string[] tokens = message.Split('\t');
        //    //    float temperature;
        //    //    bool parsed = float.TryParse(tokens[1], out temperature);
        //    //    if (parsed)
        //    //        HeatersFanSupply.Temperature.ActualValue = temperature;
        //    //}

        //    base.OnMessageReceived();
        //}

    }

}
