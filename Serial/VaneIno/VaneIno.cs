using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Serial.VaneIno
{
    public class VaneIno : Device
    {
        public VaneIno(bool isSimulated = false) : base(isSimulated)
        {
            signals.Add("Analog", new Signal("Analog", isSimulated) { SimulatorSignalType = SimulatorSignalType.Instant });


            signals.Add("Analog1", new Signal("Analog1", isSimulated) { SimulatorSignalType = SimulatorSignalType.Instant });
            signals.Add("TimeBetween", new Signal("TimeBetween", isSimulated) { SimulatorSignalType = SimulatorSignalType.Instant });
            signals.Add("Analog2", new Signal("Analog2", isSimulated) { SimulatorSignalType = SimulatorSignalType.Instant });

            for (int i = 1; i <= 4; i++)
                signals.Add($"Vane{i}", new Signal($"Vane{i}", isSimulated) { SimulatorSignalType = SimulatorSignalType.Instant });
        }

        public Signal Vane(int i) => signals[$"Vane{i}"];

        public Signal Analog { get { return signals["Analog"]; } }

        //the following are used if we are getting 2 values instead of 1
        public Signal Analog1 { get { return signals["Analog1"]; } }
        public Signal Analog2 { get { return signals["Analog2"]; } }

        public Signal TimeBetween { get { return signals["TimeBetween"]; } }
    }
}
