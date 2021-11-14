using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Paulus.Serial.Kern
{
    public class KernScaleAgent : SerialDevice
    {
        //Communicates with the Kern scales.
        public KernScaleAgent(string portName, int baudRate = 9600) 
            //only the baudRate is configurable, the other features are fixed
            : this(new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)) { }

        public KernScaleAgent(SerialPort port)
            : base(port,ResponseMode.Monitor)
        {
            ReceiveMessageSuffix = "\r\n";
            //SendMessageSuffix = "\n";

            KernScale = new KernScale(false);
        }


        public KernScale KernScale { get; private set; }

        #region Send commands

        //public bool SendMeasure()
        //{
        //    return SendMessage("*IDN?");
        //}
        #endregion

        protected override void OnMessageReceived()
        {
            base.OnMessageReceived();

            try
            {
                KernScale.UpdateDeviceValuesBySerialMessage(LastSerialMessage.ReceivedFilteredMessage, LastSerialMessage.MessageSent);
            }
            catch { }
        }

    }
}
