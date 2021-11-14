using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial.GasMixer
{
    public class GasComponent
    {
        /// <summary>
        /// Initializes a gas component in a mixture.
        /// </summary>
        /// <param name="gas"></param>
        /// <param name="concentration">Concentration in ppm</param>
        public GasComponent(Gas gas, float concentration)

        { Gas = gas; Concentration = concentration; }


        /// <summary>
        /// Gas concentration in ppm.
        /// </summary>
        public float Concentration { get; set; }

        public Gas Gas { get; set; }
    }

}
