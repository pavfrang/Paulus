using Paulus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial.GasMixer
{
    public class CylinderComponent
    {
        public string GasName { get; }
        public float Concentration { get; }
        public float Tolerance { get; }


        public ConcentrationUnit ConcentrationUnit { get; }

        public CylinderComponent(string gasName, float concentration, float tolerance, ConcentrationUnit concentrationUnit)
        {
            GasName = gasName;
            Concentration = concentration;
            Tolerance = tolerance;
            ConcentrationUnit = concentrationUnit;
        }

        public GasComponent ToGasComponent()
        {
            float concentrationInPpm = Concentration;
            if (ConcentrationUnit == ConcentrationUnit.PerCent) concentrationInPpm *= 10000.0f;
            return new GasComponent(SimpleGas.CommonGases[GasName], concentrationInPpm);
        }

        public string GetConcentrationString()
        {
            string unitNameWithSpace = ConcentrationUnit == ConcentrationUnit.PPM ? " ppm" : "%";

            if (Tolerance == 0)
                return $"{Concentration}{unitNameWithSpace}";
            else
                return $"{Concentration}±{Tolerance}{unitNameWithSpace}";

        }

        public override string ToString()
            => $"{GasName} {GetConcentrationString()}";


    }


}
