using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial.Dropletino
{
    //Used by the Acidino Project
    public class SensorRelays : Device
    {
        public SensorRelays(bool isSimulated = false) : base(isSimulated)
        {
            signals.Add("Sensor 1 Relay", new Signal("Sensor 1 Relay", isSimulated) { SimulatorSignalType = SimulatorSignalType.Instant });
            signals.Add("Sensor 2 Relay", new Signal("Sensor 2 Relay", isSimulated) { SimulatorSignalType = SimulatorSignalType.Instant }); 
        }

        public Signal Sensor1Relay { get { return signals["Sensor 1 Relay"]; } }
        public Signal Sensor2Relay { get { return signals["Sensor 2 Relay"]; } }

    }
}
