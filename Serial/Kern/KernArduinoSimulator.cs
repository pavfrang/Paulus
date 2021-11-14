using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

using System.Windows.Forms;

namespace Paulus.Serial.Kern
{
    public  class KernArduinoSimulator : SerialDevice
    {
        public KernArduinoSimulator(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) :
            base(portName,ResponseMode.Monitor, "", baudRate, parity, dataBits, stopBits)
        {

            Temperature = new Signal("Temperature");
            Temperature.NoiseRange = 1.0f;
        }

        protected override void setMessagePrefixSuffix()
        {
            SendMessageSuffix = "\r\n";
            ReceiveMessageSuffix = "\n";
        }

        Timer tmrTemperature;

        public override bool Reset()
        {
            tmrTemperature = new Timer();
            tmrTemperature.Interval = 1000;
            tmrTemperature.Tick += tmrTemperature_Tick;
            tmrTemperature.Start();
            return true;
        }

        public Signal Temperature;

        private void tmrTemperature_Tick(object sender, EventArgs e)
        {
            SendMessage(Temperature.ActualValue.ToString("0.00"));
        }
    }
}
