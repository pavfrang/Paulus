using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Paulus.Serial.LatNI
{
    /// <summary>
    /// Simulates the NI system that measures the temperatures and the pressures.
    /// </summary>

    public class LatNISystemSimulator : SerialDevice
    {
        public LatNISystemSimulator(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) :
            base(portName, ResponseMode.Monitor,"", baudRate, parity, dataBits, stopBits)
        {
            //commandReceivedDelay = 50;
            latNI = new LatNI(true);

        }

        protected override void setMessagePrefixSuffix()
        {
            SendMessageSuffix = "\n\r";
        }

        //The Simulated Flap devices
        LatNI latNI;

        protected override void OnMessageReceived()
        {
            base.OnMessageReceived();

            if (LastSerialMessage.ReceivedFilteredMessage.StartsWith("P0"))
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 1; i <= 16; i++)
                    sb.AppendFormat($"{latNI.Temperature(i).ActualValue:000.0},");
                for (int i = 1; i <= 16; i++)
                    sb.AppendFormat($"{latNI.Pressure(i).ActualValue:000.0},");

                SendMessage(sb.ToString().TrimEnd(','));
            }
        }
    }
}
