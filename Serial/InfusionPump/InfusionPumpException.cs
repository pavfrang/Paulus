using Paulus.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Serial.InfusionPump
{
    [Flags]
    public enum InfusionPumpErrorCode
    {
        [Description("No error")]
        NoError = 0,
        [Description("Serial error")]
        SerialError = 1,
        Stall = 2,
        [Description("Serial overrun")]
        SerialOverrun = 4,
        [Description("Over pressure")]
        OverPressure = 8,
        Undefined = 16
    }

    public class InfusionPumpException : System.Exception
    {
        public InfusionPumpException(InfusionPumpErrorCode code, string commandSent) :
            base(GetShortErrorMessageSafe(code))
        {
            CommandSent = commandSent;
            Code = code;
        }

        public InfusionPumpException(Exception innerException, string commandSent) :
            base(innerException.Message, innerException)
        {
            CommandSent = commandSent;
            Code = InfusionPumpErrorCode.Undefined;
        }
        public InfusionPumpException(string message, Exception innerException, string commandSent) :
            base(message, innerException)
        {
            CommandSent = commandSent;
            Code = InfusionPumpErrorCode.Undefined;
        }

        public InfusionPumpErrorCode Code { get; }

        public string CommandSent { get; }


        public static string GetShortErrorMessageSafe(InfusionPumpErrorCode code) =>
            Enum.IsDefined(typeof(InfusionPumpErrorCode), code) ? code.GetDescription() : "Undefined Infusion Pump Exception";

    }
}
