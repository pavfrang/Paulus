using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Paulus.Serial.Dropletino
{
    public class SensorRelaysArduinoSimulator : SerialDevice
    {
        public SensorRelaysArduinoSimulator(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) :
            base(portName, ResponseMode.Monitor, "", baudRate, parity, dataBits, stopBits)
        {
        }

        protected override void setMessagePrefixSuffix()
        {
            SendMessageSuffix = "\r\n";
            ReceiveMessageSuffix = "\n";
        }

        public override bool Reset()
        {
            return SendMessage("Welcome to the LAT Dropetino by Paul and Dimitris");
        }


        Random rnd = new Random();
        protected override void OnMessageReceived()
        {
            base.OnMessageReceived();

            switch (LastSerialMessage.ReceivedFilteredMessage.ToUpper())
            {
                case "S1 ON":
                    SendMessage("[COMMAND] Pin 10 set to LOW.");
                    SendMessage("[S1 ON OK]");
                    break;
                case "FAN OFF":
                    SendMessage("[COMMAND] Pin 10 set to HIGH."); break;
                case "SUPPLY ON":
                    SendMessage("[COMMAND] Pin 11 set to LOW."); break;
                case "SUPPLY OFF":
                    SendMessage("[COMMAND] Pin 11 set to HIGH."); break;
                case "TEMP":
                    float temperature = 25.0f + (float)rnd.NextDouble() * 2.0f;
                    SendMessage($"[TEMPERATURE]\t{temperature}"); break;
                case "RESET":
                    Reset(); break;
                default:
                    SendMessage("[ERROR] Unknown command."); break;
            }
        }
    }
}
