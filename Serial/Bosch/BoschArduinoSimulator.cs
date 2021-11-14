using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Paulus.Serial.Bosch
{
    /// <summary>
    /// Simulates the Arduino board that manages the vanes which operate the Bosch/Fluke relays.
    /// </summary>
    public class BoschArduinoSimulator : SerialDevice
    {
        public BoschArduinoSimulator(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) :
            base(portName, ResponseMode.Monitor,"", baudRate, parity, dataBits, stopBits)
        {
        }


        protected override void setMessagePrefixSuffix()
        {
            SendMessageSuffix = "\r\n";
            ReceiveMessageSuffix = "\n";
        }

        public override bool Reset()
        {
            return SendMessage("Welcome to the LAT Boschino by Paul, Elias, George") &&
                   SendMessage("--------------------------------------------------");
        }

        protected override void OnMessageReceived()
        {
            base.OnMessageReceived();

            switch (LastSerialMessage.ReceivedFilteredMessage.ToUpper())
            {
                case "START REGEN 1":
                    SendMessage("[COMMAND] Pin 2 set to LOW."); break;
                case "START REGEN 2":
                    SendMessage("[COMMAND] Pin 5 set to LOW."); break;
                case "STOP REGEN 1":
                    SendMessage("[COMMAND] Pin 2 set to HIGH."); break;
                case "STOP REGEN 2":
                    SendMessage("[COMMAND] Pin 5 set to HIGH."); break;
                case "START MEASURE 1":
                    SendMessage("[COMMAND] Pin 3 set to LOW.");
                    SendMessage("[COMMAND] Pin 4 set to LOW.");
                    break;
                case "START MEASURE 2":
                    SendMessage("[COMMAND] Pin 6 set to LOW.");
                    SendMessage("[COMMAND] Pin 7 set to LOW.");
                    break;
                case "STOP MEASURE 1":
                    SendMessage("[COMMAND] Pin 3 set to HIGH.");
                    SendMessage("[COMMAND] Pin 4 set to HIGH.");
                    break;
                case "STOP MEASURE 2":
                    SendMessage("[COMMAND] Pin 6 set to HIGH.");
                    SendMessage("[COMMAND] Pin 7 set to HIGH.");
                    break;
                case "RESET":
                    Reset(); break;
                default:
                    SendMessage("[ERROR] Unknown command."); break;
            }
        }
    }


}
