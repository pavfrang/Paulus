using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Serial
{
    [Serializable]

    public class InvalidUnitException : System.Exception
    {
        public InvalidUnitException() { }

        public InvalidUnitException (string variableName, string variableUnit, params string[] allowedUnits) :
            base ($"Unrecognized {variableName} unit ({variableUnit}). Allowed values: {string.Join(", ", allowedUnits)}.")
        {
            VariableName = variableName;
            VariableUnit = variableUnit;
            AllowedUnits = allowedUnits;
        }

        //public InvalidUnitException(string message) : base(message) { }
        //public InvalidUnitException(string message, Exception inner) : base(message, inner) { }
        protected InvalidUnitException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string VariableName { get; protected set; }
        public string VariableUnit { get; protected set; }

        public string[] AllowedUnits { get; protected set; }

        protected string getMessage() {
            string sAllowedUnits = string.Join(", ", AllowedUnits);
            return $"Unrecognized {VariableName} unit ({VariableUnit}). Allowed values: {sAllowedUnits}.";
        }
    }
}
