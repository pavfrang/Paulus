using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Paulus.Serial.Kern
{
    public class KernScale : Device
    {
        public KernScale(bool isSimulated = false) : base(isSimulated)
        {
            signals.Add("Weight", new Signal("Weight"));
            Weight.IsSimulated = isSimulated;
        }

        public Signal Weight { get { return signals["Weight"]; } }

        private bool _isOverloaded;
        public bool IsOverloaded { get { return _isOverloaded; } }

        public const float WeightInvalidValue = 99999.0f;

        public event EventHandler NewWeightReceived;
        protected void OnNewWeightReceived()
        {
            NewWeightReceived?.Invoke(this, EventArgs.Empty);
        }

        public override void UpdateDeviceValuesBySerialMessage(string message, string messageType = "")
        {
            message = message.Trim();

            if (message.Contains("_"))
            {
                Weight.ActualValue = 0.0f;
                _isOverloaded = false;
                OnNewWeightReceived();
            }
            else if (message.Contains("="))
            {
                Weight.ActualValue = WeightInvalidValue;
                _isOverloaded = true;
                OnNewWeightReceived();
            }
            else
            {
                _isOverloaded = false;

                if (message.Contains("g"))
                    message = message.Substring(0, message.Length - 1);
                try
                {
                    //ignore any parse error and keep the last value
                    Weight.ActualValue = float.Parse(message, CultureInfo.InvariantCulture);
                    OnNewWeightReceived();
                }
                catch
                {
                    Weight.ActualValue = WeightInvalidValue;
                }
            }
        }
    }

}