using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Timers;

namespace Paulus.Serial.Flaps
{

    public class Flap : Device
    {
        //TODO: implement minimumVoltage, maximumVoltage SETUP as a signal

        /// <summary>
        /// Implements both simulated and non-simulated Flap device. By default the device is simulated (all its signals are simulated).
        /// For non-simulated Flaps change the IsSimulated behavior to false.
        /// </summary>
        public Flap(bool isSimulated = false) : base(isSimulated)
        {
            signals.Add("Position", new Signal("Position"));

            signals.Add("Voltage", new Signal("Voltage"));
            Voltage.SimulatorSignalType = SimulatorSignalType.Manual;
            //calculate the value only if the device is simulated
            if (isSimulated)
                Voltage.RequestingActualValue += Voltage_RequestingActualValue;

            signals.Add("Minimum Voltage", new Signal("Minimum Voltage"));
            MinimumVoltage.SimulatorSignalType = SimulatorSignalType.Manual;
            if (isSimulated)
                MinimumVoltage.ActualValue = 2.0f;

            signals.Add("Maximum Voltage", new Signal("Maximum Voltage"));
            MaximumVoltage.SimulatorSignalType = SimulatorSignalType.Manual;
            if (isSimulated)
                MaximumVoltage.ActualValue = 7.0f;

            Position.IsSimulated = isSimulated;
        }


        #region Device signals
        public Signal Position { get { return signals["Position"]; } }


        public Signal Voltage { get { return signals["Voltage"]; } }

        /// <summary>
        /// The function is internally called when the (simulated) voltage value is requested.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Voltage_RequestingActualValue(object sender, EventArgs e)
        {
            Voltage.ActualValue = Position.ActualValue / 100.0f * (7.0f - 2.0f) + 2.0f;
        }

        public Signal MinimumVoltage { get { return signals["Minimum Voltage"]; } }
        public Signal MaximumVoltage { get { return signals["Maximum Voltage"]; } }

        #endregion

        /// <summary>
        /// The function must be manually called each time.
        /// </summary>
        /// <param name="message"></param>
        public override void UpdateDeviceValuesBySerialMessage(string message, string messageType = "")
        {
            string[] tokens = message.Split(',');
            if (tokens.Length != 4)
                return;

            tokens[0] = tokens[0].Substring(tokens[0].IndexOf(']') + 1);

            CultureInfo en = CultureInfo.InvariantCulture;
            float actualVoltage;
            if (float.TryParse(tokens[0], NumberStyles.Any, en, out actualVoltage))
                Voltage.ActualValue = actualVoltage;

            float actualPosition;
            if (float.TryParse(tokens[1], NumberStyles.Any, en, out actualPosition))
                Position.ActualValue = actualPosition;

            float minimumVoltage;
            if (float.TryParse(tokens[2], NumberStyles.Any, en, out minimumVoltage))
                MinimumVoltage.ActualValue = minimumVoltage;

            float maximumVoltage;
            if (float.TryParse(tokens[3], NumberStyles.Any, en, out maximumVoltage))
                MaximumVoltage.ActualValue = maximumVoltage;

        }
    }
}
