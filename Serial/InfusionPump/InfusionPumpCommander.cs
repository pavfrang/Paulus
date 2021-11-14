using Paulus.Serial;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Paulus.Serial.InfusionPump
{
    [Description("Cole-Parmer 200")]

    public class InfusionPumpCommander : DeviceCommander
    {


        /// <summary>
        /// This constructor is needed for debugging purposes (e.g. testing connection). SerialDevice.IsDeviceConnectedAt is using this constructor.
        /// </summary>
        public InfusionPumpCommander() : base(ResponseMode.Immediate, "")
        { }

        /// <summary>
        /// This constructor is useful for testing purposes. The device manager is not needed in this case.
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="name"></param>
        public InfusionPumpCommander(string portName, string name = "")
           : base(portName, ResponseMode.Immediate, name)
        {
            EditSettings = new InfusionPumpSettings();
            RuntimeSettings = new InfusionPumpSettings();
        }

        /// <summary>
        /// This constructor is useful for testing purposes. The device manager is not needed in this case.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="name"></param>
        public InfusionPumpCommander(SerialPort port, string name = "")
           : base(port, ResponseMode.Immediate, name)
        {
            EditSettings = new InfusionPumpSettings();
            RuntimeSettings = new InfusionPumpSettings();
        }

        /// <summary>
        /// The constructor is needed when the device manager is loaded via an xml configuration file.
        /// </summary>
        /// <param name="deviceManager"></param>
        /// <param name="xmlDevice"></param>
        public InfusionPumpCommander(DeviceManager deviceManager, XmlElement xmlDevice)
            : base(deviceManager, ResponseMode.Immediate, "") //name is set by the xml element
        {
            EditSettings = new InfusionPumpSettings(deviceManager, xmlDevice);
            RuntimeSettings = new InfusionPumpSettings(deviceManager);

            InfusionPumpSettings es = (InfusionPumpSettings)EditSettings;
            InfusionPumpSettings rs = (InfusionPumpSettings)RuntimeSettings;
            //copy the libraries and the units to the runtime units
            rs.SyringeLibrary = es.SyringeLibrary;
            rs.LiquidLibrary = es.LiquidLibrary;
            rs.TargetVolumeUnit = es.TargetVolumeUnit;
            rs.DeliveredVolumeUnit = es.DeliveredVolumeUnit;
            rs.InfusionRateUnit = es.InfusionRateUnit;
            rs.InitialVolumeUnit = es.InitialVolumeUnit;
            rs.RemainingTimeThresholdUnit = es.RemainingTimeThresholdUnit;
            rs.RemainingVolumeUnit = es.RemainingVolumeUnit;

            rs.DeviceId = es.DeviceId;
            rs.DeviceName = es.DeviceName;

            Name = es.DeviceName;
            es.Commander = rs.Commander = this;
        }



        /// <summary>
        /// The constructor is needed when the device manager and the current settings need to be set manually (without an xml file).
        /// </summary>
        /// <param name="deviceManager"></param>
        /// <param name="xmlDevice"></param>
        public InfusionPumpCommander(DeviceManager deviceManager, string name = "")
            : base(deviceManager, ResponseMode.Immediate, name) //name is set by the xml element
        {
            EditSettings = new InfusionPumpSettings(deviceManager);
            RuntimeSettings = new InfusionPumpSettings(deviceManager);
            EditSettings.DeviceName = RuntimeSettings.DeviceName = name;
        }

        static int infusionPumpsCount = 0;

        protected override string SetNextAvailableName()
        {
            return Name = $"Infusion Pump #{++infusionPumpsCount}";
        }

        //public static async Task<bool> IsDeviceConnectedAt(string portName)
        //{
        //    InfusionPumpCommander commander = new InfusionPumpCommander(portName);
        //    bool connected = await commander.Connect(false, true);
        //    if (connected) commander.Disconnect();
        //    return connected;
        //}

        //same as default
        //public override SerialPort GetDefaultSerialPortSettings(string portName)
        //{
        //    return new SerialPort(portName,9600,Parity.None,8,StopBits.One);
        //}

        public override Bitmap Image
        {
            get
            {
                return DeviceImages.InfusionPump;
            }
        }

        #region Commands

        protected override void setMessagePrefixSuffix()
        {
            base.setMessagePrefixSuffix();

            //watch: the manual wrongly states that the suffix is \r\n!!!
            //if \r\n is used then serial overrun error occurs!!! 
            SendMessageSuffix = "\r";
            // ReceiveMessagePrefix = "\r\n"; //not used

        }

        public bool IgnoreErrors = false;

        private async Task<SimpleSerialCommandWithResponse<Status?>> sendGetPrompt(
            SimpleSerialCommandWithResponse<Status?> command, Action actionOnSuccess = null) =>
             await SendGetCommand(command,
                response =>
                {
                    if (IgnoreErrors && response.Reply == null) return;

                    var s = RuntimeSettings as InfusionPumpSettings;
                    if (s != null)
                    {
                        s.Status = response.Reply;

                        if (s.Status != Status.NotApplicable && s.Status != Status.Error)
                            actionOnSuccess?.Invoke();
                    }
                    else
                        actionOnSuccess?.Invoke();
                });

        public async Task<SimpleSerialCommandWithResponse<Status?>> Start() =>
            await sendGetPrompt(InfusionPumpCommands.Start());

        public async Task<SimpleSerialCommandWithResponse<Status?>> Stop() =>
            await sendGetPrompt(InfusionPumpCommands.Stop());

        public async Task<SimpleSerialCommandWithResponse<Status?>> GetRunStatus() =>
            await sendGetPrompt(InfusionPumpCommands.GetRunStatus());


        public async Task<SimpleSerialCommandWithResponse<Status?>> SetSyringe(Syringe syringe) =>
            await sendGetPrompt(InfusionPumpCommands.SetSyringeDiameter(syringe.DiameterInMillimeters),
                () => (RuntimeSettings as InfusionPumpSettings).Syringe = syringe);

        public async Task<SimpleSerialCommandWithResponse<Tuple<float?, Status?>>> GetSyringe() =>
            await SendGetCommand(InfusionPumpCommands.GetSyringeDiameterInMillimeters(), (response) =>
             {
                 if (IgnoreErrors && response.Reply == null) return;

                 var settings = RuntimeSettings as InfusionPumpSettings;
                 var syringeLibrary = settings.SyringeLibrary;
                 settings.Syringe = syringeLibrary.FirstOrDefault(s => s.DiameterInMillimeters == response.Reply.Item1);
                 settings.Status = response.Reply.Item2;
             });

        public async Task<SimpleSerialCommandWithResponse<Tuple<float?, string, Status?>>> GetDeliveredVolume() =>
               await SendGetCommand(InfusionPumpCommands.GetDeliveredVolume(), (response) =>
                {
                    if (IgnoreErrors && response.Reply == null) return;

                    var settings = RuntimeSettings as InfusionPumpSettings;

                    settings.DeliveredVolume = response.Reply.Item1.Value;
                    settings.DeliveredVolumeUnit = settings.RemainingVolumeUnit = response.Reply.Item2;

                    settings.RemainingVolume = settings.TargetVolume - settings.DeliveredVolume;

                    settings.Status = response.Reply.Item3;
                });

        public async Task<SimpleSerialCommandWithResponse<Tuple<float?, string, Status?>>> GetTargetVolume() =>
                    await SendGetCommand(InfusionPumpCommands.GetTargetVolume(), (response) =>
                     {
                         if (response.Reply == null) return;

                         var settings = RuntimeSettings as InfusionPumpSettings;

                         settings.DeliveredVolume = response.Reply.Item1.Value;
                         settings.DeliveredVolumeUnit = response.Reply.Item2;
                         settings.Status = response.Reply.Item3;
                     });
        public async Task<SimpleSerialCommandWithResponse<Tuple<float, string, Status?>>> GetInfusionRate() =>
                    await SendGetCommand(InfusionPumpCommands.GetInfusionRate(), (response) =>
                     {
                         if (IgnoreErrors && response.Reply == null) return;

                         var settings = RuntimeSettings as InfusionPumpSettings;

                         settings.InfusionRate = response.Reply.Item1;
                         settings.InfusionRateUnit = response.Reply.Item2;
                         settings.Status = response.Reply.Item3;
                     });
        public async Task<SimpleSerialCommandWithResponse<Status?>> SetInfusionRate(float value, string unit) =>
             await sendGetPrompt(InfusionPumpCommands.SetInfusionRate(value, unit), () =>
                  {
                      var settings = RuntimeSettings as InfusionPumpSettings;
                      settings.InfusionRate = value;
                      settings.InfusionRateUnit = unit;
                  });


        public async Task<SimpleSerialCommandWithResponse<Status?>> SetTargetVolume(float value, string unit) =>
             await sendGetPrompt(InfusionPumpCommands.SetTargetVolume(value, unit), () =>
              {
                  var settings = RuntimeSettings as InfusionPumpSettings;
                  settings.TargetVolume = value;
                  settings.TargetVolumeUnit = unit;
              });


        public async Task<SimpleSerialCommandWithResponse<Tuple<int?, Status?>>> GetError() =>
                    await SendGetCommand(InfusionPumpCommands.GetError(), response =>
                     {
                         if (IgnoreErrors && response.Reply == null) return;

                         var settings = RuntimeSettings as InfusionPumpSettings;
                         if (settings != null)
                         {
                             settings.LastException = new InfusionPumpException((InfusionPumpErrorCode)response.Reply.Item1, response.CommandText);
                             settings.Status = response.Reply.Item2;
                         }
                     });

        public async Task<SimpleSerialCommandWithResponse<Tuple<string, Status?>>> GetVersion() =>
            await SendGetCommand(InfusionPumpCommands.GetVersion(), response =>
             {
                 if (IgnoreErrors && response.Reply == null) return;


                 (RuntimeSettings as InfusionPumpSettings).Version =
                 (EditSettings as InfusionPumpSettings).Version = response.Reply.Item1;

                 (RuntimeSettings as InfusionPumpSettings).Status = response.Reply.Item2;
             });


        #endregion

        protected override SerialMessage receiveMessage()
        {
            try
            {
                //an IO exception can still be thrown here
                // if (LastSerialMessage.MessageSent.Contains("5000.00")) Debugger.Break();

                //Thread.Sleep(1000);
                //string test = serialPort.ReadExisting();

                //after the \r\n the message begins!
                serialPort.ReadTo("\r\n");

                if (LastSerialCommandSent is SimpleSerialCommandWithResponse<Status?>)
                {
                    char nextChar = (char)serialPort.ReadChar();
                    if (nextChar == 'N')
                    {
                        serialPort.ReadChar(); //A
                        //only the status is returned
                        LastSerialMessage.ReceivedFilteredMessage = LastSerialMessage.ReceivedFullMessage = "NA";
                    }
                    else
                        LastSerialMessage.ReceivedFilteredMessage = LastSerialMessage.ReceivedFullMessage = nextChar.ToString();
                }
                else //in all other cases there is another \r\n 
                {
                    LastSerialMessage.ReceivedFilteredMessage = serialPort.ReadTo("\r\n");
                    char nextChar = (char)serialPort.ReadChar();
                    if (nextChar == 'N')
                    {
                        serialPort.ReadChar(); //A
                        //only the status is returned
                        LastSerialMessage.ReceivedFilteredMessage += "\r\nNA";
                    }
                    else
                        LastSerialMessage.ReceivedFilteredMessage += $"\r\n{nextChar}";
                    LastSerialMessage.ReceivedFullMessage = LastSerialMessage.ReceivedFilteredMessage;
                }

                Debug.WriteLine($"{PortName} [{DateTime.Now:HH:mm:ss.f}]: '{LastSerialMessage.ReceivedFilteredMessage}'");


                OnMessageReceived();
                OnDataRead();
            }
            catch (Exception exception)
            {
                if (LastSerialMessage != null)
                {
                    LastSerialMessage.IsError = true;
                    LastSerialMessage.Exception = exception;
                }
                OnErrorReceived();
            }
            return LastSerialMessage;
        }

        public override async Task<bool> TestCommunication()
        {
            return (await GetVersion()).Success;
            //return (await GetError()).Success;
            //return (await GetRunStatus()).Success;
            //try
            //{
            //    await syncSemaphore.WaitAsync();
            //    serialPort.Write("prom?\r\n");
            //    string response1 = serialPort.ReadTo("\r\n");
            //    char responseStatus = (char)serialPort.ReadChar();
            //    return true;
            //}
            //catch { return false; }
            //finally
            //{
            //    syncSemaphore.Release();
            //}
        }

        public override async Task<bool> ReadDeviceInformation()
        {
            return (await GetVersion()).Success;
        }

        public override List<Variable> GetVariables()
        {
            var s = EditSettings as InfusionPumpSettings;

            return new List<Variable> {
                new Variable("Syringe"),
                new Variable("Liquid"),
                new Variable("Initial Volume","ml"),
                new Variable("Target Volume","ul"),
                new Variable("Delivered Volume","ul"),
               // new Variable("Remaining Volume","ul"),
                new Variable("Infusion Rate","ul/min"),
            };
        }

        public override object[] GetVariableValues()
        {
            var s = IsConnected ?
                            RuntimeSettings as InfusionPumpSettings :
                            EditSettings as InfusionPumpSettings;

            return new object[]
                {
                    s.Syringe.ToString(),
                    s.Liquid.ID,
                    s.GetInitialVolumeInMicroLiters()/1000,
                    s.GetTargetVolumeInMicroLiters(),
                    s.GetDeliveredVolumeInMicroLiters(),
                    s.GetInfusionRateInMicrolitersPerMinute()
                };
        }
    }
}