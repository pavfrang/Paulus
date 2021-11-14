using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Paulus.Serial.Arduino
{
    [Description("Arduino Uno Rev3")]
    public class ArduinoRev3Commander : DeviceCommander
    {
        #region Constructors

        public ArduinoRev3Commander(
            string settingsPath, string name = "") : base(ResponseMode.Immediate, name)
        { }

        public ArduinoRev3Commander(SerialPort port, string name = "")
            : base(port, ResponseMode.Immediate, name)
        { }

        //the default constructor is needed in order to pass the port name later
        public ArduinoRev3Commander(string name = "") :
            base(ResponseMode.Immediate, name)
        { }
        public ArduinoRev3Commander(DeviceManager deviceManager,
            XmlNode xmlDevice) : base(deviceManager, ResponseMode.Immediate, "")
        {
            
        }

        



        protected static int arduinosCount;

        protected override string SetNextAvailableName()
        {
            return Name = $"Arduino #{++arduinosCount}";
        }
        #endregion

        public static async Task<bool> IsDeviceConnectedAt(string portName)
        {
            ArduinoRev3Commander agent = new ArduinoRev3Commander();
            agent.SerialPort = agent.GetDefaultSerialPortSettings(portName);
            bool connected = await agent.Connect(false, true);
            if (connected) agent.Disconnect();
            return connected;
        }

        public override Task<bool> ReadDeviceInformation()
        {
            throw new NotImplementedException();
        }

        public override Bitmap Image
        {
            get
            {
                return DeviceImages.Arduino;
            }
        }
    }
}
