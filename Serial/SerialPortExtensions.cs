using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Paulus.Serial
{
    public static class SerialPortExtensions
    {
        public static SerialPort ToSerialPort(this XmlElement node)
        {
            //<serialport name="COM13" device="Gas Mixer #1" baudrate="9600" parity="None" databits ="8" stopbits ="One" />
            var a = node.Attributes;
            string name = a["name"].Value;
            int baudrate = int.Parse(a["baudrate"].Value);
            Parity parity = (Parity)Enum.Parse(typeof(Parity), a["parity"].Value, true);
            int dataBits = int.Parse(a["databits"].Value);
            StopBits stopBits = (StopBits)Enum.Parse(typeof(StopBits), a["stopbits"].Value, true);

            return new SerialPort(name, baudrate, parity, dataBits, stopBits);
        }

        public static string ToXmlString(this SerialPort port, string additionalAttributes = "")
            => $"<serialport name=\"{port.PortName}\" baudrate=\"{port.BaudRate}\" parity = \"{port.Parity}\" databits=\"{port.DataBits}\" stopbits=\"{port.StopBits}\" {additionalAttributes} />";
        

    }
}
