using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Paulus.Serial.HeatersFanSupply
{
    public class HeatersFanSupplyArduinoSimulator : SerialDevice
    {
        public HeatersFanSupplyArduinoSimulator(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) :
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
            return SendMessage("Welcome to the LAT Acidino by Paul and Elias") &&
                   SendMessage("--------------------------------------------");
        }


        Random rnd = new Random();
        protected override void OnMessageReceived()
        {
            base.OnMessageReceived();

            switch (LastSerialMessage.ReceivedFilteredMessage.ToUpper())
            {
                case "HEATERS ON":
                    SendMessage("[COMMAND] Pin 8 set to LOW.");
                    SendMessage("[COMMAND] Pin 9 set to LOW.");
                    break;
                case "HEATERS OFF":
                    SendMessage("[COMMAND] Pin 8 set to HIGH.");
                    SendMessage("[COMMAND] Pin 9 set to HIGH.");
                    break;
                case "FAN ON":
                    SendMessage("[COMMAND] Pin 10 set to LOW."); break;
                case "FAN OFF":
                    SendMessage("[COMMAND] Pin 10 set to HIGH."); break;
                case "SUPPLY ON":
                    SendMessage("[COMMAND] Pin 11 set to LOW."); break;
                case "SUPPLY OFF":
                    SendMessage("[COMMAND] Pin 11 set to HIGH."); break;
                case "TEMP":
                    float temperature = 25.0f + (float)rnd.NextDouble() * 2.0f;
                    SendMessage($"[TEMPERATURE]\t{temperature}");break;
                case "RESET":
                    Reset(); break;
                default:
                    SendMessage("[ERROR] Unknown command."); break;
            }
        }
    }
}
