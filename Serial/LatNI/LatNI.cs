using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Paulus.Serial.LatNI
{
    /// <summary>
    /// Represents the National Instruments LAT program interface.
    /// </summary>

    public class LatNI : Device
    {
        /// <summary>
        /// For non-simulated Flaps change the IsSimulated behavior to false.
        /// </summary>
        public LatNI(bool isSimulated = false) : base(isSimulated)
        {
            for (int i = 1; i <= 16; i++)
            {
                signals.Add($"T{i}", new Signal($"T{i}"));
                signals.Add($"P{i}", new Signal($"P{i}"));
            }

            if(!isSimulated)
            {
                foreach (var entry in signals)
                    entry.Value.IsSimulated = false;
            }
            else if (isSimulated)
            {
                Random rnd = new Random();
                for (int i = 1; i <= 16; i++) Temperature(i).TargetValue = rnd.Next(20, 35);
                for (int i = 1; i <= 16; i++) Pressure(i).TargetValue = rnd.Next(0, 5);
            }
        }


        #region Device signals
        /// <summary>
        /// Returns a temperature signal.
        /// </summary>
        /// <param name="iChannel">iChannel is in the range 1-16</param>
        /// <returns></returns>
        public Signal Temperature(int iChannel) { return signals[$"T{iChannel}"]; }

        /// <summary>
        /// Returns a pressure signal.
        /// </summary>
        /// <param name="iChannel">iChannel is in the range 1-16</param>
        /// <returns></returns>
        public Signal Pressure(int iChannel) { return signals[$"P{iChannel}"]; }

        #endregion

        /// <summary>
        /// The function must be manually called each time. The message updates all the signals internally.
        /// </summary>
        /// <param name="message"></param>
        public override void UpdateDeviceValuesBySerialMessage(string message, string messageType = "")
        {
            //0026.3,0026.0,0025.5,0025.5,0025.9,0027.0,0026.5,0027.7,0026.5,0025.8,0026.1,0026.3,0025.3,0027.3,0025.6,0024.5,0001.5,0000.9,0000.0,0000.0,0000.0,0000.0,0000.0,0000.0,0002.2,0000.3,0004.3,0002.4,0001.8,0000.0,0000.0,0000.0
            string[] tokens = message.Split(',');
            if (tokens.Length < 32)
                return;

            CultureInfo en = CultureInfo.InvariantCulture;
            //get all temperatures
            for (int iToken = 0; iToken < 16; iToken++)
                Temperature(iToken + 1).ActualValue = float.Parse(tokens[iToken], NumberStyles.Number, en);

            //get all pressures
            for (int iToken = 16; iToken < 32; iToken++)
                Pressure(iToken - 15).ActualValue = float.Parse(tokens[iToken], en);
        }
    }

}
