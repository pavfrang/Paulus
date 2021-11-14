using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial.GasMixer
{
    public abstract class Gas
    {

        //e.g. "O2","OXYGEN","OXYGEN                      O2     "
        //the declaration is used in the communication with the device
        public string ShortName { get; protected set; }
        public string FullName { get; protected set; }
        //public string Declaration { get; protected set; }
        public float KFactor { get; protected set; }


        public override string ToString()
            => ShortName;
    }


}
