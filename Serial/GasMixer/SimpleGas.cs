using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial.GasMixer
{
    public class SimpleGas : Gas
    {
        public SimpleGas(string shortName, string fullName, float kFactor)
        {
            ShortName = shortName;
            FullName = fullName;
            //Declaration = declaration;
            KFactor = kFactor;
        }
        public SimpleGas(string shortName, float kFactor) :
            this(shortName, shortName, kFactor) //string.Format("{0,-35}", shortName)
        { }

        public static Dictionary<string, SimpleGas> CommonGases = new List<SimpleGas>
        {
            new SimpleGas("Air",1.005996f),
            new SimpleGas("NH3","Ammonia",0.718913f),
            new SimpleGas("CO2","Carbon Dioxide",0.73738f),
            new SimpleGas("CO","Carbon Monoxide",0.999f),
            new SimpleGas("COS","Carbonyl Sulfide",0.659772f),
            new SimpleGas("C2H4","Ethylene",0.597575f),
            new SimpleGas("He","Helium",1.41514f),
            new SimpleGas("H2","Hydrogen",1.009998f),
            new SimpleGas("H2S","Hydrogen Sulfide",0.84361f),
            new SimpleGas("CH4","Methane",0.719183f),
            new SimpleGas("Ne","Neon",1.415303f),
            new SimpleGas("NO","Nitric Oxide",0.97593f),

            new SimpleGas("N2","Nitrogen",1.0f),
            new SimpleGas("NO2","Nitrogen Dioxide",0.740257f),
            new SimpleGas("N2O","Nitrous Oxide",0.708911f),
            new SimpleGas("O2","Oxygen",0.991642f),
            new SimpleGas("C3H8","Propane",0.348344f),
            new SimpleGas("C3H6","Propylene",0.398169f),
            new SimpleGas("SO2","Sulfur Dioxide",0.686408f),

            new SimpleGas("Ar","Argon",1.414671f)
        }.ToDictionary(g => g.ShortName, g => g);
    }

}
