using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial.HeatersFanSupply
{
    //Used by the Acidino Project
    public class HeatersFanSupply : Device
    {
        public HeatersFanSupply(bool isSimulated = false) : base(isSimulated)
        {
            signals.Add("Heaters", new Signal("Heaters", isSimulated) { SimulatorSignalType = SimulatorSignalType.Instant });
            signals.Add("Fan", new Signal("Fan", isSimulated) { SimulatorSignalType = SimulatorSignalType.Instant });
            signals.Add("Supply", new Signal("Supply", isSimulated) { SimulatorSignalType = SimulatorSignalType.Instant });
            signals.Add("Temperature", new Signal("Temperature", isSimulated));
        }

        public Signal Heaters { get { return signals["Heaters"]; } }
        public Signal Fan { get { return signals["Fan"]; } }
        public Signal Supply { get { return signals["Supply"]; } }

        public Signal Temperature { get { return signals["Temperature"]; } }

        //public override void UpdateDeviceValuesBySerialMessage(string message, string messageType = "")
        //{
        //    if (message.StartsWith("[TEMPERATURE]"))
        //    {
        //        string[] tokens = message.Split('\t');
        //        float temperature;
        //        bool parsed = float.TryParse(tokens[1], out temperature);
        //        if (parsed)
        //            Temperature.ActualValue = temperature;
        //    }
        //}

    }
}
