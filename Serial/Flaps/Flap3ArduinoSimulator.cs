using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;


using System.Globalization;

using System.Timers;
using System.Threading;

namespace Paulus.Serial.Flaps
{
    //TODO: Simulate MinVoltageValue, MaxVoltageValue
    //TODO: Add the schematics of the Arduino Board!
    //TODO: Implement the Save, Setup (e.g. simulate reading from memory → reading from registry)

    /// <summary>
    /// Simulates the Arduino board that manages the three flaps FlapA, FlapB and FlapC.
    /// </summary>
    public class Flap3ArduinoSimulator : SerialDevice
    {
        public Flap3ArduinoSimulator(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) :
            base(portName, ResponseMode.Monitor,"", baudRate, parity, dataBits, stopBits)
        {
            //commandReceivedDelay = 50;
            flapA = new Flap(true);
            flapB = new Flap(true);
            flapC = new Flap(true);
        }

        public override bool Reset()
        {
            return SendMessage("Welcome to the LAT Klapetino by George and Paul") &&
                   SendMessage("-----------------------------------------------") &&
                   SendMessage("[Status] Initializing...") &&
                   SendMessage("[Status] Initialized");
        }

        //The Simulated Flap devices
        Flap flapA, flapB, flapC;

        protected override void OnMessageReceived()
        {
            base.OnMessageReceived();

            if (LastSerialMessage.ReceivedFilteredMessage.StartsWith("FLAPA"))
                flapA.Position.TargetValue = float.Parse(LastSerialMessage.ReceivedFilteredMessage.Substring(5), en);
            else if (LastSerialMessage.ReceivedFilteredMessage.StartsWith("FLAPB"))
                flapB.Position.TargetValue = float.Parse(LastSerialMessage.ReceivedFilteredMessage.Substring(5), en);
            else if (LastSerialMessage.ReceivedFilteredMessage.StartsWith("FLAPC"))
                flapC.Position.TargetValue = float.Parse(LastSerialMessage.ReceivedFilteredMessage.Substring(5), en);
            else if (LastSerialMessage.ReceivedFilteredMessage.StartsWith("READALL"))
            {
                SendMessage(
                    string.Format(en,
                     "[Read A]{0:0.00},{1:0.00},{2:0.00},{3:0.00}|[Read B]{4:0.00},{5:0.00},{6:0.00},{7:0.00}|[Read C]{8:0.00},{9:0.00},{10:0.00},{11:0.00}",
                     flapA.Voltage.ActualValue, flapA.Position.ActualValue, flapA.MinimumVoltage.ActualValue, flapA.MaximumVoltage.ActualValue,
                     flapB.Voltage.ActualValue, flapB.Position.ActualValue, flapB.MinimumVoltage.ActualValue, flapB.MaximumVoltage.ActualValue,
                     flapC.Voltage.ActualValue, flapC.Position.ActualValue, flapC.MinimumVoltage.ActualValue, flapC.MaximumVoltage.ActualValue));
            }
            else if (LastSerialMessage.ReceivedFilteredMessage.StartsWith("READA"))
            {
                SendMessage(
                    string.Format(en,
                     "[Read A]{0:0.00},{1:0.00},{2:0.00},{3:0.00}",
                     flapA.Voltage.ActualValue, flapA.Position.ActualValue, flapA.MinimumVoltage.ActualValue, flapA.MaximumVoltage.ActualValue));
            }
            else if (LastSerialMessage.ReceivedFilteredMessage.StartsWith("READB"))
            {
                SendMessage(
                    string.Format(en,
                     "[Read B]{0:0.00},{1:0.00},{2:0.00},{3:0.00}",
                     flapB.Voltage.ActualValue, flapB.Position.ActualValue, flapB.MinimumVoltage.ActualValue, flapB.MaximumVoltage.ActualValue));
            }
            else if (LastSerialMessage.ReceivedFilteredMessage.StartsWith("READC"))
            {
                SendMessage(
                    string.Format(en,
                     "[Read C]{0:0.00},{1:0.00},{2:0.00},{3:0.00}",
                     flapC.Voltage.ActualValue, flapC.Position.ActualValue, flapC.MinimumVoltage.ActualValue, flapC.MaximumVoltage.ActualValue));
            }
            else if (LastSerialMessage.ReceivedFilteredMessage == "RESET")
            {
                Reset();
            }
            else if (LastSerialMessage.ReceivedFilteredMessage.StartsWith("SAVE"))
            {
                SendMessage("[Status] Saved states to internal memory.");
            }
            else if (LastSerialMessage.ReceivedFilteredMessage.StartsWith("SETUP"))
            {
                serialPort.WriteLine("[Status] Flap calibration started...");
                Thread.Sleep(1000);
                SendMessage("[Status] Flap calibration finished");
            }
        }


    }
}
