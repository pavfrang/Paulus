using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Paulus.Serial.Bosch;
using System.Threading;

namespace Paulus.Serial.Fluke
{
    public class Fluke : Device
    {
        /// <summary>
        /// For non-simulated Flaps change the IsSimulated behavior to false.
        /// </summary>
        public Fluke(bool isSimulated = false) : base(isSimulated)
        {

            signals.Add("Resistance", new Signal("Resistance"));
            Resistance.IsSimulated = isSimulated;
            if (isSimulated)
            {
                Resistance.SimulatorSignalType = SimulatorSignalType.Linear;
                Resistance.TimeConstant = 1.0f;
                Resistance.NoiseRange = 1e3f;
            }

            //signals.Add("Voltage", new Signal("Voltage"));
            //Voltage.SimulatorSignalType = SimulatorSignalType.Manual;
            ////calculate the value only if the device is simulated
            //if (isSimulated)
            //    Voltage.RequestingActualValue += Voltage_RequestingActualValue;

            //signals.Add("Minimum Voltage", new Signal("Minimum Voltage"));
            //MinimumVoltage.SimulatorSignalType = SimulatorSignalType.Manual;
            //if (isSimulated)
            //    MinimumVoltage.ActualValue = 2.0f;
        }


        #region Device signals
        public Signal Resistance { get { return signals["Resistance"]; } }

        //Used only if the device is simulated
        internal void startResistanceLoading()
        {
            Resistance.TargetValue = 20e6f;
            Resistance.ActualValue = 20e6f;

            Resistance.TargetValue = 2e4f;
            Resistance.UseTimer = true;
        }

        //Used only if the device is simulated
        internal void setOverload(bool overload)
        {
            _isOverload = overload;
            Resistance.UseTimer = !overload;
        }

        private bool _isOverload;
        public bool IsOverload { get { return _isOverload; } }

        private string _model;
        public string Model { get { return _model; } }

        private string _softwareVersion;
        public string SoftwareVersion { get { return _softwareVersion; } }

        private string _serialNumber;
        public string SerialNumber { get { return _serialNumber; } }

        #endregion


        private readonly object syncObject = new object();
        public bool TryLockForExclusiveUse(ISensorWithResistance sensor)
        {
            bool succeeded = false;
            lock (syncObject)
            {
                if (currentSensor == null)
                {
                    currentSensor = sensor;
                    succeeded = true;
                }
                else
                    succeeded = false;
            }

            return succeeded;
        }

        public void UnlockForExclusiveUse()
        {
            lock (syncObject)
            {
                if (currentSensor != null)
                    currentSensor = null;
            }
        }

        public bool IsLockedForExclusiveUse()
        {
            return currentSensor != null;
        }


        public double MinimumMeasureDelayInMs { get; set; } = 200.0;

        public DateTime NextAvailableMeasureTime
        {
            get
            {
                if (currentSensor == null)
                    return DateTime.Now;

                else
                    return currentSensor.NextStepStartTime.AddMilliseconds(200.0);
            }
        }


        private ISensorWithResistance currentSensor;

        public ISensorWithResistance CurrentSensor
        {
            get
            {
                return currentSensor;
            }
        }



        private List<float> resistances;
        bool isStoringValues;
        public void StartStoringNormalValues()
        {
            isStoringValues = true;
            resistances = new List<float>();
        }

        //stops storing values and return the last valid resistance
        public float StopStoringNormalValues()
        {
            isStoringValues = false;
            if (resistances != null && resistances.Count > 0)
            {
                float lastNormalValue = resistances[resistances.Count - 1];
                resistances = null;
                return lastNormalValue;
            }
            else return 0.0f;
        }



        /// <summary>
        /// The function must be manually called each time typically from an agent.
        /// </summary>
        /// <param name="message"></param>
        public override void UpdateDeviceValuesBySerialMessage(string message, string messageType = "")
        {
            //+9.99999999E+37,OHM,OL,NONE
            //2.37E0,OHM,NORMAL,NONE
            string[] tokens = message.Split(',');

            if (messageType.Equals("QM", StringComparison.CurrentCultureIgnoreCase))
            {
                if (tokens.Length != 4)
                    return;

                _isOverload = tokens[2] != "NORMAL";

                float resistance;
                if (float.TryParse(tokens[0], out resistance))
                {
                    Resistance.ActualValue = resistance;

                    if (isStoringValues && !_isOverload)
                        resistances.Add(resistance);
                }
            }
            else if (messageType.Equals("ID", StringComparison.CurrentCultureIgnoreCase))
            {
                if (tokens.Length != 3)
                    return;

                _model = tokens[0];
                _softwareVersion = tokens[1];
                _serialNumber = tokens[2];
            }
        }
    }

    public interface ISensorWithResistance
    {
        double Resistance { get; set; }

        DateTime NextStepStartTime { get; }
        void measure();
    }
}
