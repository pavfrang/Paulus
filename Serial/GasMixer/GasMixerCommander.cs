using Paulus.Serial;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO.Ports;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using Paulus.Common;

namespace Paulus.Serial.GasMixer
{
    [Description("Environics Series 2000")]
    /// <summary>
    /// The agent is the command which sends commands to the gas mixer.
    /// </summary>
    public class GasMixerCommander : DeviceCommander
    {
        /// <summary>
        /// This constructor is needed for debugging purposes (e.g. testing connection). SerialDevice.IsDeviceConnectedAt is using this constructor.
        /// </summary>
        public GasMixerCommander() : base(ResponseMode.Immediate, "")
        { }

        /// <summary>
        /// This constructor is useful for testing purposes. The settings and the device manager are not typically needed in this case.
        /// </summary>
        /// <param name="portName"></param>
        public GasMixerCommander(string portName, string name = "") : base(portName, ResponseMode.Immediate, name)
        { }

        /// <summary>
        /// This constructor is useful for testing purposes. The settings and the device manager are not typically needed in this case.
        /// </summary>
        /// <param name="port"></param>
        public GasMixerCommander(SerialPort port, string name = "") : base(port, ResponseMode.Immediate, name)
        { }


        /// <summary>
        /// The constructor is needed when the device manager is loaded via an xml configuration file.
        /// </summary>
        /// <param name="deviceManager"></param>
        /// <param name="xmlDevice"></param>
        public GasMixerCommander(DeviceManager deviceManager, XmlElement xmlDevice)
            : base(deviceManager, ResponseMode.Immediate, "") //name is set by the xml element
        {
            EditSettings = new GasMixerSettings(deviceManager, xmlDevice);
            RuntimeSettings = new GasMixerSettings(deviceManager);
            RuntimeSettings.DeviceName = Name = EditSettings.DeviceName;
            (RuntimeSettings as GasMixerSettings).CylinderLibrary = (EditSettings as GasMixerSettings).CylinderLibrary;
        }

        /// <summary>
        /// The constructor is needed when the device manager and the current settings need to be set manually (without an xml file).
        /// </summary>
        /// <param name="deviceManager"></param>
        /// <param name="xmlDevice"></param>
        public GasMixerCommander(DeviceManager deviceManager, string name = "")
            : base(deviceManager, ResponseMode.Immediate, name) //name is set by the xml element
        {
            EditSettings = new GasMixerSettings(deviceManager);
            EditSettings.DeviceName = name;

            RuntimeSettings = new GasMixerSettings(deviceManager);
            RuntimeSettings.DeviceName = name;
            (RuntimeSettings as GasMixerSettings).CylinderLibrary = (EditSettings as GasMixerSettings).CylinderLibrary;
        }

        protected override void setMessagePrefixSuffix()
        {
            SendMessagePrefix = $"{STX} ";
            SendMessageSuffix = $" {ETX}";
            ReceiveMessageSuffix = $"{ETX}";
        }

        public bool IgnoreErrors = false;


        //public CylinderLibrary CylinderLibrary
        //{
        //    get
        //    {
        //        return
        //            (CurrentSettings as GasMixerSettings).CylinderLibrary;
        //    }
        //}


        static int gasMixersCount = 0;

        protected override string SetNextAvailableName()
        {
            return Name = $"Gas Mixer #{++gasMixersCount}";
        }

        /// <summary>
        /// This is useful for fast connection.
        /// </summary>
        public void CopyEditToRuntimeSettings()
        {
            GasMixerSettings es = EditSettings as GasMixerSettings;
            GasMixerSettings rs = RuntimeSettings as GasMixerSettings;

            rs.CreateMFCs(es.MFCs.Count);
            rs.CreatePorts(es.Ports.Count);

            foreach (Port p in es.Ports.Values)
                rs.Ports[p.ID].Cylinder = p.Cylinder;

            foreach (MFC mfc in es.MFCs.Values)
            {
                MFC runtimeMfc = rs.MFCs[mfc.ID];
                runtimeMfc.CurrentPort = rs.Ports[mfc.CurrentPort.ID];
                runtimeMfc.IsPurgeOn = mfc.IsPurgeOn;
                foreach (Port p in mfc.Ports)
                    runtimeMfc.Ports.Add(rs.Ports[p.ID]);
                runtimeMfc.SizeInCcm = mfc.SizeInCcm;
                runtimeMfc.TargetConcentrationInPpm = mfc.TargetConcentrationInPpm;
                runtimeMfc.TargetFlowInCcm = mfc.TargetFlowInCcm;
                runtimeMfc.TargetPurgeFlowInCcm = mfc.TargetPurgeFlowInCcm;
            }
            rs.Mode = es.Mode;
        }

        //public static async Task<bool> IsDeviceConnectedAt(string portName)
        //{
        //    GasMixerCommander agent = new GasMixerCommander(portName);
        //    bool connected = await agent.Connect(false, true);
        //    if (connected) agent.Disconnect();
        //    return connected;
        //}



        protected override void OnSerialPortChanged()
        {
            //force timeout to 3 seconds
            serialPort.ReadTimeout = serialPort.WriteTimeout = 3000;
            base.OnSerialPortChanged();
        }

        //public GasMixer GasMixer { get; } = new GasMixer(false);


        private const char STX = '\x2';
        private const char ACK = '\x6';
        private const char NAK = '\x15';
        private const char ETX = '\x3';


        public override async Task<bool> ReadDeviceInformation()
        {
            //read number of ports
            if ((await ReadPortCylinderConcentrations()).IsError) return false;

            //tstStatus.Text = "Reading MFCs information...";

            //read the number of MFCs
            if ((await ReadMfcCount()).IsError) return false;

            //get the MFC size and valid port assignments
            int mfcCount = (RuntimeSettings as GasMixerSettings).MFCs.Count; // GasMixer.MFCs.Count;
            for (int iMfc = 1; iMfc <= mfcCount; iMfc++)
            {
                if ((await ReadMfcSize(iMfc)).IsError) return false;
                if ((await ReadValidPortsPerMfc(iMfc)).IsError) return false;
            }
            //return true only if no error has occured
            return true;
        }

        /// <summary>
        /// The function will override both the 
        /// </summary>
        /// <param name="deviceManager"></param>
        /// <returns></returns>
        public async Task QueryCylinderInformation()
        {
            var cylinders = CylinderLibrary.Default;

            //we assume that the ports ids are retrievable (the last part of the name ends to #id)
            //as well as the Cylinder.Default cylinder library
            GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
            for (int iPort = 1; iPort <= rs.Ports.Count; iPort++)
            {
                var response = await ReadPortCylinderId(iPort);
                string id = response.Reply;

                Cylinder matchCylinder = cylinders[id];
                if (matchCylinder != null)
                    rs.Ports[iPort].Cylinder = matchCylinder;
            }
        }


        /// <summary>
        /// Tests communication synchronously.
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> TestCommunication()
        {
            //bool success = await GasMixerCommands.ReadTime().SendAndGetReply(this) != null;
            //return success;

            var response = await GasMixerCommands.ReadTime().SendAndGetCommand(this);
            return response.Success;
        }

        #region Concentration mode
        public async Task<SimpleSerialCommand> ConcentrationUpdate() =>
            await GasMixerCommands.ConcentrationUpdate().SendAndGetCommand(this);


        public async Task<SimpleSerialCommand> AssignTargetConcentration(int mfcId, float targetConcentrationInPpm) =>
            await SendSetCommand(GasMixerCommands.AssignTargetConcentration(mfcId, targetConcentrationInPpm),
                () =>
                {
                    GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                    MFC mfc = rs.MFCs[mfcId];
                    mfc.TargetConcentrationInPpm = targetConcentrationInPpm;
                    mfc.TargetFlowInCcm = rs.TotalTargetFlowInCcm * mfc.TargetConcentrationInPpm / mfc.CurrentPort.ConcentrationInPpm / mfc.CurrentPort.KFactor;
                });


        public async Task<SimpleSerialCommand> AssignBalance(int mfc) =>//tested
            await SendSetCommand(GasMixerCommands.AssignBalance(mfc),
                () =>
                {
                    GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                    rs.BalanceMfc = rs.MFCs[mfc];
                });



        public async Task<SimpleSerialCommand> AssignTotalTargetFlow(float flowInCcm) =>
            await SendSetCommand(GasMixerCommands.AssignTotalTargetFlow(flowInCcm),
                () =>
                {
                    GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                    rs.TotalTargetFlowInCcm = flowInCcm;

                    //update the actual flow for each MFC controller!
                    foreach (MFC mfc in rs.MFCs.Values)
                        mfc.TargetFlowInCcm = rs.TotalTargetFlowInCcm * mfc.TargetConcentrationInPpm / mfc.CurrentPort.ConcentrationInPpm / mfc.CurrentPort.KFactor;

                });

        public async Task<SimpleSerialCommandWithResponse<float?>> ReadFlowTotalActual() =>
                //return await GasMixerCommands.ReadFlowTotalActual().SendAndGetCommand(this);
                await SendGetCommand<float?>(GasMixerCommands.ReadFlowTotalActual(),
                    (response) =>
                    {
                        if (IgnoreErrors && response.Reply == null) return;
                        GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                        rs.TotalActualFlowInCcm = response.Reply.Value;

                        //update the actual flow for each MFC controller!
                        foreach (MFC mfc in rs.MFCs.Values)
                            mfc.ActualFlowInCcm = rs.TotalActualFlowInCcm * mfc.ActualConcentrationInPpm / mfc.CurrentPort.ConcentrationInPpm / mfc.CurrentPort.KFactor;
                    });


        public async Task<SimpleSerialCommandWithResponse<List<int>>> ReadWarnings() =>
            await SendGetCommand<List<int>>(GasMixerCommands.ReadWarnings(),
                (response) =>
                {
                    GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                    int mfcsCount = rs.MFCs.Count;
                    if (IgnoreErrors && (response.Reply == null ||
                            response.Reply.Count != mfcsCount)) return;

                    List<int> codes = response.Reply;

                    //if (codes.Count != GasMixer.MFCs.Count)
                    //{
                    //    serialPort.DiscardInBuffer();
                    //    return; //THIS SHOULD BE REMOVED to track any error
                    //}

                    //there is an additional 0 at the end of the message (we should check it)
                    //for (int i = 0; i < GasMixer.MFCs.Count; i++)
                    for (int i = 0; i < Math.Min(codes.Count, mfcsCount); i++)
                    {
                        rs.MFCs[i + 1].Warning =
                        (EditSettings as GasMixerSettings).MFCs[i + 1].Warning = (Warning)codes[i];
                    }
                });

        public async Task<SimpleSerialCommandWithResponse<List<float>>> ReadConcentrationAllActual() //for each MFC
        {
            //return await GasMixerCommands.ReadConcentrationAllActual().SendAndGetCommand(this);
            return await SendGetCommand<List<float>>(GasMixerCommands.ReadConcentrationAllActual(),
                (response) =>
                {
                    GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                    int mfcsCount = rs.MFCs.Count;

                    List<float> concentrations = response.Reply;
                    if (IgnoreErrors && (response.Reply == null ||
                    response.Reply.Count != mfcsCount)) return;

                    //if (concentrations.Count != GasMixer.MFCs.Count)
                    //{
                    //    serialPort.DiscardInBuffer();
                    //    return; //THIS SHOULD BE REMOVED to track any error
                    //}

                    //for (int i = 0; i < GasMixer.MFCs.Count; i++)
                    for (int i = 0; i < Math.Min(concentrations.Count, mfcsCount); i++)
                        rs.MFCs[i + 1].ActualConcentrationInPpm = concentrations[i];

                    //update the actual flow for each MFC controller!
                    foreach (MFC mfc in rs.MFCs.Values)
                        mfc.ActualFlowInCcm = rs.TotalActualFlowInCcm * mfc.ActualConcentrationInPpm / mfc.CurrentPort.ConcentrationInPpm / mfc.CurrentPort.KFactor;

                });
        }

        public async Task<SimpleSerialCommandWithResponse<float?>> ReadTargetConcentration(int mfc)
        {
            return await SendGetCommand(GasMixerCommands.ReadTargetConcentration(mfc),
                (response) =>
                {
                    GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                    rs.MFCs[mfc].TargetConcentrationInPpm = response.Reply.Value;
                });
        }


        #endregion

        #region Purge mode

        public async Task<SimpleSerialCommand> PurgeUpdate()
        {
            return await GasMixerCommands.PurgeUpdate().SendAndGetCommand(this);
        }

        public async Task<SimpleSerialCommand> AssignPurgeTargetFlow(int mfc, float targetFlowInCcm) =>
           await SendSetCommand(GasMixerCommands.SetTargetPurgeFlow(mfc, targetFlowInCcm),
                () =>
                {
                    GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                    rs.MFCs[mfc].TargetPurgeFlowInCcm = targetFlowInCcm;
                });


        public async Task<SimpleSerialCommand> SetPurgeOn(int mfc) =>
            await SendSetCommand(GasMixerCommands.SetPurgeOn(mfc),
                () =>
                {
                    GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                    rs.MFCs[mfc].IsPurgeOn = true;
                });


        public async Task<SimpleSerialCommand> SetPurgeOff(int mfc) =>
            await SendSetCommand(GasMixerCommands.SetPurgeOff(mfc),
                () =>
                {
                    GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                    rs.MFCs[mfc].IsPurgeOn = false;
                });


        #endregion

        /// <summary>
        /// Updates the GasMixer device time and returns the result. On error the GasMixer device time is not updated.
        /// </summary>
        /// <returns></returns>
        public async Task<SerialDoubleCommandWithResponse<TimeSpan?, DateTime?, DateTime?>> ReadDeviceTime()
        {
            return await GasMixerCommands.ReadTimeAndDate().SendAndGetCommand(this);

            //TimeSpan? time = await GasMixerCommands.ReadTime.Send(this);
            //if (time == null) return null;
            //DateTime? date = await GasMixerCommands.ReadDate.Send(this);
            //if (date == null) return null;
            //return GasMixer.DeviceTime = date + time;

            ////"\u00068,40,35" (8:40:35)
            //SerialMessage timeResponse = await SendMessageAndReadResponse("TIME ?");
            //if (timeResponse.IsError) return null;
            //string timePattern = "\x6(?<hour>\\d{1,2}),(?<minute>\\d{1,2}),(?<second>\\d{1,2})";
            //Match mTime = Regex.Match(timeResponse.ReceivedFilteredMessage, timePattern);
            //if (!mTime.Success) return null;
            //int hour = int.Parse(mTime.Groups["hour"].Value);
            //int minute = int.Parse(mTime.Groups["minute"].Value);
            //int second = int.Parse(mTime.Groups["second"].Value);

            ////"\u000616,12,16" (16-DEC-2016)
            //SerialMessage dateResponse = await SendMessageAndReadResponse("DATE ?");
            //if (dateResponse.IsError) return null;
            //string datePattern = "\x6(?<day>\\d{2}),(?<month>\\d{1,2}),(?<year>\\d{2})";
            //Match mDate = Regex.Match(dateResponse.ReceivedFilteredMessage, datePattern);
            //if (!mDate.Success) return null;
            //int day = int.Parse(mDate.Groups["day"].Value);
            //int month = int.Parse(mDate.Groups["month"].Value);
            //int year = int.Parse(mDate.Groups["year"].Value);

            ////also update the internal gas mixer
            //return GasMixer.DeviceTime = new DateTime(2000 + year, month, day, hour, minute, second);
        }

        /// <summary>
        /// Stop all operation of the system (except the communication) and return the system to an idle state with no flow and all port shut off.
        /// </summary>
        /// <returns></returns>
        public async Task<SimpleSerialCommand> Stop() =>
            await SendSetCommand(GasMixerCommands.Stop());


        public async Task<SimpleSerialCommand> Home() =>
            await SendSetCommand(GasMixerCommands.Home(),
                () =>
                {
                    GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                    rs.Mode = Mode.MaintainPorts;
                });



        public async Task<SimpleSerialCommandWithResponse<int?>> ReadMfcCount()
        {
            var response = await GasMixerCommands.ReadMfcCount().SendAndGetCommand(this);
            if (!response.IsError)
            {
                GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                if (rs.MFCs.Count == 0)
                    rs.CreateMFCs(response.Reply.Value);
            }
            return response;
        }

        public async Task<SimpleSerialCommandWithResponse<float?>> ReadMfcSize(int iMfc)
        {
            //the MFCs must be set by calling the readmfccount function before calling this function
            var response = await GasMixerCommands.ReadMfcSize(iMfc).SendAndGetCommand(this);


            if (!response.IsError)
            {
                GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                rs.MFCs[iMfc].SizeInCcm = response.Reply.Value;
            }

            return response;
        }

        /// <summary>
        /// Ports are initialized via this command.
        /// </summary>
        /// <returns></returns>
        public async Task<SerialCommandWithResponse<List<float>>> ReadPortCylinderConcentrations()
        {
            var response = await GasMixerCommands.ReadPortCylinderConcentrations().SendAndGetCommand(this);
            if (!response.IsError)
            {
                List<float> concentrations = response.Reply;
                if (IgnoreErrors && response.Reply == null) return null;

                GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                rs.CreatePorts(concentrations.Count);
                //GasMixer.createPorts(portsCount); //command cannot be used in current object state

                ////and set the concentrations (currently retrieved)
                //for (int i = 0; i < portsCount; i++)
                //    GasMixer.Ports[i + 1].Concentration = concentrations[i];
            }

            return response;
        }

        public async Task<SerialCommandWithResponse<string>> ReadPortCylinderId(int port) =>
            await GasMixerCommands.ReadPortCylinderId(port).SendAndGetCommand(this);

        public async Task<SimpleSerialCommandWithResponse<List<int>>> ReadValidPortsPerMfc(int mfc)
        {
            //the MFCs must be set by calling the ReadMfcCount function BEFORE calling this function
            var response = await GasMixerCommands.ReadValidPorts(mfc).SendAndGetCommand(this);

            if (!response.IsError)
            {
                GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                //a port assignment should be here NOT a new port
                rs.MFCs[mfc].Ports = response.Reply.Select(i => rs.Ports[i]).ToList();
            }

            return response;
        }


        public async Task<SerialCommandWithResponse<List<int>>> ReadPortAssignments()
        {
            //the MFC and Ports must be initialized before calling this command
            //typically the commands GetMfcCount, ReadPortCylinderConcentrations and ReadValidPorts must be called prior to calling this command
            var response = await GasMixerCommands.ReadPortAssignments().SendAndGetCommand(this);

            if (!response.IsError)
            {
                List<int> portIds = response.Reply;
                if (IgnoreErrors && response.Reply == null) return null;

                GasMixerSettings rs = RuntimeSettings as GasMixerSettings;

                foreach (int id in portIds)
                    if (!rs.Ports.ContainsKey(id))
                    {
                        //check that all port ids are valid!
                        response.SerialMessage.IsError = true;
                        break;
                    }

                //if no error, then assign port to MFCs
                if (!response.SerialMessage.IsError)
                {
                    for (int i = 0; i < portIds.Count; i++)
                        rs.MFCs[i + 1].CurrentPort = rs.Ports[portIds[i]];
                }
            }

            return response;
        }
        public async Task<SimpleSerialCommandWithResponse<bool?>> ReadIsLowFlow() =>
            await GasMixerCommands.ReadIsLowFlow().SendAndGetCommand(this);


        //the ReadMfcCount and ReadValidPorts must be called before calling this function
        //it is assumed that only a valid port is assigned to the mfc
        public async Task<SimpleSerialCommand> AssignPortToMfc(int port, int mfc) =>
            await SendSetCommand(
                GasMixerCommands.AssignPortToMfc(port, mfc),
                () =>
                {
                    GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                    rs.MFCs[mfc].CurrentPort = rs.Ports[port];
                });

        #region Assign Port Cylinder

        public async Task<SimpleSerialCommand> AssignPortCylinder(int port, Cylinder cylinder)
        {

            SimpleSerialCommand assignNameReply = await AssignPortCylinderName(port, cylinder.GetSafeString());
            if (assignNameReply.IsError) return assignNameReply;

            SimpleSerialCommand assignConcentration = await AssignPortCylinderConcentration(port, cylinder.MainComponentConcentrationInPpm);
            if (assignConcentration.IsError) return assignConcentration;

            SimpleSerialCommand assignKFactor = await AssignPortCylinderKFactor(port, cylinder.GasMixture.KFactor);

            if (assignKFactor.Success)
            {
                GasMixerSettings rs = RuntimeSettings as GasMixerSettings;
                rs.Ports[port].Cylinder = cylinder;
            }

            return assignKFactor;
        }

        public async Task<SimpleSerialCommand> AssignPortCylinderName(int port, string name) =>
            await SendSetCommand(GasMixerCommands.AssignPortCylinderName(port, name));

        public async Task<SimpleSerialCommand> AssignPortCylinderKFactor(int port, float kFactor) =>
            await GasMixerCommands.AssignPortCylinderKFactor(port, kFactor).SendAndGetCommand(this);

        public async Task<SimpleSerialCommand> AssignPortCylinderConcentration(int port, float cylinderConcentration) =>
            await GasMixerCommands.AssignPortCylinderConcentration(port, cylinderConcentration).SendAndGetCommand(this);

        #endregion

        //   string[] acknowledgeCommands = new string[] { "STOP", "CONC UPDATE", "HOME" };

        protected override SerialMessage receiveMessage()
        {
            try
            {
                //an IO exception can still be thrown here
                //READ THE FIRST CHAR
                char firstChar = (char)serialPort.ReadChar();

                if (firstChar == ACK)
                {
                    //all non-acknowledge commands will receive ACK...ETX or NAK...ETX messages
                    //if (!acknowledgeCommands.Contains(LastSerialMessage.MessageSent)
                    //    && !LastSerialMessage.MessageSent.Contains("=")) //all assignment commands contain '='
                    if (!(LastSerialCommandSent is SimpleSerialCommand))
                    {
                        LastSerialMessage.ReceivedFullMessage =
                        LastSerialMessage.ReceivedFilteredMessage =
                            serialPort.ReadTo(ReceiveMessageSuffix); //ETX
                    }

                    //queue of messages is disabled for the moment
                    //MessagesReceived.Enqueue(LastSerialMessage);
                }
                else if (firstChar == NAK)
                {
                    LastSerialMessage.ReceivedFullMessage =
                    LastSerialMessage.ReceivedFilteredMessage =
                        serialPort.ReadTo(ReceiveMessageSuffix); //ETX

                    LastSerialMessage.IsError = true;
                    LastSerialMessage.Exception = new GasMixerException(int.Parse(LastSerialMessage.ReceivedFullMessage), LastSerialMessage.MessageSent);
                    OnErrorReceived();
                }
                else
                {
                    //dispose the serial port buffers
                    LastSerialMessage.ReceivedFullMessage = serialPort.ReadExisting();

                    LastSerialMessage.IsError = true;
                    LastSerialMessage.Exception = new GasMixerException(GasMixerException.UndefinedCode, LastSerialMessage.MessageSent);
                    OnErrorReceived();
                }

                OnMessageReceived();
                OnDataRead();
            }
            catch (Exception exception)
            {
                LastSerialMessage.IsError = true;
                LastSerialMessage.Exception = new GasMixerException(exception, LastSerialMessage.MessageSent);
                OnErrorReceived();
            }
            return LastSerialMessage;
        }

        public override Bitmap Image
        {
            get
            {
                return DeviceImages.GasMixer;
            }
        }

        public override List<Variable> GetVariables()
        {
            List<Variable> v = new List<Variable>();
            var s = EditSettings as GasMixerSettings;
            for (int i = 1; i <= s.Ports.Count; i++)
            {
                v.Add(new Variable($"{Name} Port {i} Cylinder Gases"));
                v.Add(new Variable($"{Name} Port {i} Cylinder Concentrations"));
                v.Add(new Variable($"{Name} Port {i} Cylinder Balance"));
                v.Add(new Variable($"{Name} Port {i} Cylinder Number"));
                v.Add(new Variable($"{Name} Port {i} Cylinder Code"));
            }
            for (int i = 1; i <= s.MFCs.Count; i++)
                v.Add(new Variable($"{Name} MFC {i} Current Port"));

            v.Add(new Variable($"{Name} Mode"));
            v.Add(new Variable($"{Name} Balance MFC"));

            for (int i = 1; i <= s.MFCs.Count; i++)
            {
                v.Add(new Variable($"{Name} MFC {i} Target Concentration", "ppm"));
                v.Add(new Variable($"{Name} MFC {i} Actual Concentration", "ppm"));
                v.Add(new Variable($"{Name} MFC {i} Warning Status"));

                v.Add(new Variable($"{Name} MFC {i} Target Flow", "ccm"));
                v.Add(new Variable($"{Name} MFC {i} Actual Flow", "ccm"));
            }
            v.Add(new Variable($"{Name} Target Total Flow", "l/min"));
            v.Add(new Variable($"{Name} Actual Total Flow", "l/min"));

            return v;
        }

        public override object[] GetVariableValues()
        {
            ArrayList list = new ArrayList();

            GasMixerSettings s =
                IsConnected ? RuntimeSettings as GasMixerSettings : EditSettings as GasMixerSettings;

            for (int i = 1; i <= s.Ports.Count; i++)
            {
                var cyl = s.Ports[i].Cylinder;
                list.Add(string.Join("/", cyl.Components.Select(c => c.GasName)));
                list.Add(string.Join("/", cyl.Components.Select(c => c.GetConcentrationString())));
                list.Add(cyl.Components[0].GetConcentrationString() != "100%" ? cyl.BalanceGasName : "");
                list.Add(cyl.ID);
                list.Add(cyl.CylinderCode);
            }

            for (int i = 1; i <= s.MFCs.Count; i++)
                list.Add(s.MFCs[i].CurrentPort.ID);

            //list.Add(Mode.Disconnected);
            list.Add(s.Mode);
            list.Add(s.BalanceMfc?.ID ?? 0);

            for (int i = 1; i <= s.MFCs.Count; i++)
            {
                MFC mfc = s.MFCs[i];
                list.Add(mfc.TargetConcentrationInPpm);
                list.Add(mfc.ActualConcentrationInPpm);
                list.Add(mfc.Warning.GetDescription());

                list.Add(mfc.TargetFlowInCcm);
                list.Add(mfc.ActualFlowInCcm);
            }

            list.Add(s.TotalTargetFlowInCcm / 1000.0);
            list.Add(s.TotalActualFlowInCcm / 1000.0);

            return list.ToArray();
        }
    }


}
