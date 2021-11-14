using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Serial.DownTino
{
    public class DownTino : Device
    {
        public DownTino(bool isSimulated) : base(isSimulated)
        {
            signals.Add("Relative Pressure 1", new Signal("Relative Pressure 1", isSimulated));
            signals.Add("Relative Pressure 2", new Signal("Relative Pressure 2", isSimulated));
            signals.Add("Differential Pressure 1", new Signal("Differential Pressure 1", isSimulated));
            signals.Add("Differential Pressure 2", new Signal("Differential Pressure 2", isSimulated));
            signals.Add("Temperature 1", new Signal("Temperature 1", isSimulated));
            signals.Add("Temperature 2", new Signal("Temperature 2", isSimulated));
        }

        public Signal RelativePressure1 { get { return signals["Relative Pressure 1"]; } }
        public Signal RelativePressure2 { get { return signals["Relative Pressure 2"]; } }
        public Signal DifferentialPressure1 { get { return signals["Differential Pressure 1"]; } }
        public Signal DifferentialPressure2 { get { return signals["Differential Pressure 2"]; } }

        public Signal Temperature1 { get { return signals["Temperature 1"]; } }
        public Signal Temperature2 { get { return signals["Temperature 2"]; } }
    }
}
