using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Serial.UI
{
    public static class UnitConversionsLibrary
    {
        public static float ConvertVolume(string fromUnit, string toUnit, float fromValue)
        {
            if (fromUnit == "ml" && (toUnit == "ul" || toUnit == "μl"))
                return fromValue * 1000.0f;
            else if ((fromUnit == "ul" || fromUnit == "μl") && toUnit == "ml")
                return fromValue / 1000.0f;
            else throw new InvalidUnitException();
        }

    }
}
