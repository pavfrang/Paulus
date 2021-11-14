using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Paulus.Serial.Fluke
{
    /// <summary>
    /// Simulates the NI system that measures the temperatures and the pressures.
    /// </summary>

    public class FlukeSimulator : SerialDevice
    {
        public FlukeSimulator(string portName) :
            base(portName, ResponseMode.Monitor, "",115200, Parity.None, 8, StopBits.One)
        {
            //commandReceivedDelay = 50;
            fluke = new Fluke(true);

        }
        protected override void setMessagePrefixSuffix()
        {
            //the command syntax is 
            ReceiveMessageSuffix = SendMessageSuffix = "\r";
        }

        //The Simulated Flap devices
        private Fluke fluke;

        public void StartResistanceDecreaseSimulation()
        {
            fluke.startResistanceLoading();
        }

        public void SetOverload(bool value)
        {
            fluke.setOverload(value);
        }

        protected override void OnMessageReceived()
        {
            base.OnMessageReceived();

            //CMD_ACK: 0-OK, 1-Syntax error, 2-Execution Error
            switch (LastSerialMessage.ReceivedFilteredMessage.ToUpper())
            {
                case "DS": //default setup
                case "RI": //reset instrument (resets all instrument settings except calibration constants)
                case "RMP": //reset meter properties
                    SendMessage("0"); break; //we always assume sending ok
                case "ID":
                    SendMessage("0\rFLUKE 289,V1.00,95081087"); break;
                case "QM":
                    if (fluke.IsOverload)
                        SendMessage("0\r+9.99999999E+37,OHM,OL,NONE");
                    else
                        SendMessage($"0\r{fluke.Resistance.ActualValue:##0.0##E+00},OHM,NORMAL,NONE");
                    break;
            }
        }

    }
}
