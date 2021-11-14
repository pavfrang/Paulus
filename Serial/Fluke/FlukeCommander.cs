using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Paulus.Serial.Fluke
{
    /// <summary>
    /// Implements the PC communication agent which communicates with the Fluke 289 device.
    /// </summary>

    public class FlukeCommander : SerialDevice
    {
        public FlukeCommander(string portName)
            : base(portName, ResponseMode.Monitor, "")
        {
            Fluke = new Fluke(false);
        }

        public FlukeCommander(SerialPort port)
            : base(port, ResponseMode.Monitor)
        {
            Fluke = new Fluke(false);
        }

        protected override void setMessagePrefixSuffix()
        {
            SendMessageSuffix = "\r";
            ReceiveMessageSuffix = "\r";
            ReceiveMessage2Suffix = "\r";
        }

        public override SerialPort GetDefaultSerialPortSettings(string portName)
        {
            return new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
        }

        public Fluke Fluke { get; private set; }

        #region Send Commands

        public bool Read()
        {
            lastSerialMessageSent = "QM";
            return SendMessage("QM");
        }

        public bool ReadVersion()
        {
              lastSerialMessageSent = "ID";
          return SendMessage("ID");
        }

        string lastSerialMessageSent;

        #endregion

        protected override SerialMessage receiveMessage()
        {
            try
            {
                LastSerialMessage = new SerialMessage();

                //an IO exception can still be thrown here
                LastSerialMessage.ReceivedFilteredMessage = serialPort.ReadTo(ReceiveMessageSuffix);
                LastSerialMessage.ReceivedFilteredMessage2 = serialPort.ReadTo(ReceiveMessage2Suffix);

                //there is a case where garbage lines set the LastMessage2Received to 0
                //and so we need to take again the messages
                if (LastSerialMessage.ReceivedFilteredMessage2.Split(',').Length == 1)
                {
                    if (!string.IsNullOrEmpty(ReceiveMessage2Suffix))
                    {
                        LastSerialMessage.ReceivedFilteredMessage = LastSerialMessage.ReceivedFilteredMessage2;
                        LastSerialMessage.ReceivedFilteredMessage2 = serialPort.ReadTo(ReceiveMessage2Suffix);
                    }

                    //MessagesReceived.Enqueue(LastSerialMessage);
                }

                Debug.WriteLine($"{PortName} [{DateTime.Now:HH:mm:ss.f}]: '{LastSerialMessage.ReceivedFilteredMessage},{LastSerialMessage.ReceivedFilteredMessage2}'");

                OnMessageReceived();
                OnDataRead();
            }
            catch (Exception exception)
            {
                LastSerialMessage.IsError = true;
                LastSerialMessage.Exception = exception;
            }
            return LastSerialMessage;
        }
        /// <summary>
        /// Receive commands manager.
        /// </summary>
        protected override void OnMessageReceived()
        {
            base.OnMessageReceived();

            try
            {
                Fluke.UpdateDeviceValuesBySerialMessage(this.LastSerialMessage.ReceivedFilteredMessage2, lastSerialMessageSent);
            }
            catch { }
        }
    }
}
