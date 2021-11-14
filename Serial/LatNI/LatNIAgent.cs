using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Paulus.Serial.LatNI
{
    /// <summary>
    /// Implements the PC communication agent which communicates with the LatNI measurement system.
    /// </summary>

    public class LatNIAgent : SerialDevice
    {
        public LatNIAgent(string portName,
                int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
            : this(new SerialPort(portName, baudRate, parity, dataBits, stopBits)) { }

        public LatNIAgent(SerialPort port)
            : base(port,ResponseMode.Monitor)
        {
            ReceiveMessageSuffix = "\n\r";

            // The internal Flaps are needed for measurement and their internal values are set based on the serial input.
            LatNI = new LatNI(false);
        }

        public LatNI LatNI { get; private set; }

        #region Send Commands

        public bool Read()
        {
            return SendMessage("P0");
        }

        #endregion

        /// <summary>
        /// Receive commands manager.
        /// </summary>
        protected override void OnMessageReceived()
        {
            base.OnMessageReceived();
            LatNI.UpdateDeviceValuesBySerialMessage(this.LastSerialMessage.ReceivedFilteredMessage);
        }
    }
}
