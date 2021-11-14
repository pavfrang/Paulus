using Paulus.Serial;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Paulus.Serial.Flaps
{
    /// <summary>
    /// Implements the PC communication agent which communicates with the Arduino board in order to control the three flaps FlapA, FlapB and FlapC.
    /// </summary>
    public class Flap3Commander : SerialDevice
    {
        public Flap3Commander(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
            : base(portName, ResponseMode.Monitor,"", baudRate, parity, dataBits, stopBits)
        {
            // The internal Flaps are needed for measurement and their internal values are set based on the serial input.
            FlapA = new Flap(false);
            FlapB = new Flap(false);
            FlapC = new Flap(false);
        }

        int flap3Count = 0;
        protected override string SetNextAvailableName()
        {
            return $"Klapetino #{++flap3Count}";
        }

        public Flap FlapA { get; private set; }
        public Flap FlapB { get; private set; }
        public Flap FlapC { get; private set; }


        #region Send Commands
        public bool Setup()
        {
            return SendMessage("SETUP");
        }

        public bool Save()
        {
            return SendMessage("SAVE");
        }

        public override bool Reset()
        {
            return SendMessage("RESET");
        }

        public bool SetFlapATargetPosition(float position)
        {
            FlapA.Position.TargetValue = position;
            return SendMessage("FLAPA" + position.ToString(en));
        }
        public bool SetFlapBTargetPosition(float position)
        {
            FlapB.Position.TargetValue = position;
            return SendMessage("FLAPB" + position.ToString(en));
        }
        public bool SetFlapCTargetPosition(float position)
        {
            FlapC.Position.TargetValue = position;
            return SendMessage("FLAPC" + position.ToString(en));
        }

        public bool ReadAll()
        {
            return SendMessage("READALL");
        }
        #endregion

        /// <summary>
        /// Receive commands manager.
        /// </summary>
        protected override void OnMessageReceived()
        {
            base.OnMessageReceived();
            
            if (LastSerialMessage.ReceivedFilteredMessage.Contains("|"))
            {
                string[] tokens = LastSerialMessage.ReceivedFilteredMessage.Split('|');

                FlapA.UpdateDeviceValuesBySerialMessage(tokens[0]);
                FlapB.UpdateDeviceValuesBySerialMessage(tokens[1]);
                FlapC.UpdateDeviceValuesBySerialMessage(tokens[2]);
            }

        }
    }
}
