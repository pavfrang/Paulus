using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Paulus.Serial.InfusionPump
{

    public static class InfusionPumpCommands
    {
        static SimpleSerialCommandWithResponse<Status?> SimpleSerialCommandWithPromptResponse(string command) =>
            new SimpleSerialCommandWithResponse<Status?>(command, @"E|<|>|:|NA", (string sStatus) =>
              (statusCodes.ContainsKey(sStatus) ? (Status?)statusCodes[sStatus] : null));

        static Dictionary<string, Status> statusCodes = new Dictionary<string, Status>
        {
            { "E",Status.Error},
            { "<",Status.Infusing},
            { ">",Status.Withdrawing},
            { "NA",Status.NotApplicable},
            { ":",Status.Stopped}
        };

        static CultureInfo en = CultureInfo.CreateSpecificCulture("en-us");

        public static SimpleSerialCommandWithResponse<Status?> Start() =>
            SimpleSerialCommandWithPromptResponse("run");
        public static SimpleSerialCommandWithResponse<Status?> Stop() =>
            SimpleSerialCommandWithPromptResponse("stop");

        public static SimpleSerialCommandWithResponse<Status?> GetRunStatus() =>
            SimpleSerialCommandWithPromptResponse("run?");

        public static SimpleSerialCommandWithResponse<Status?> SetSyringeDiameter(float diameterInMillimeters) =>
            SimpleSerialCommandWithPromptResponse($"dia {diameterInMillimeters:#0.00}");

        public static SimpleSerialCommandWithResponse<Tuple<float?, Status?>> GetSyringeDiameterInMillimeters() =>
           new SimpleSerialCommandWithResponse<Tuple<float?, Status?>>("dia?",
               $@"(?<value>{SerialCommands.FloatPattern})\r\n(?<status>E|<|>|:|NA)",
               (Match response) =>
                   new Tuple<float?, Status?>(float.Parse(response.Groups["value"].Value, en),
                       (statusCodes.ContainsKey(response.Groups["status"].Value) ? (Status?)statusCodes[response.Groups["status"].Value] : null))
               );


        public static SimpleSerialCommandWithResponse<Tuple<float?, string, Status?>> GetDeliveredVolume() =>
           new SimpleSerialCommandWithResponse<Tuple<float?, string, Status?>>("del?",
               $@"(?<value>{SerialCommands.FloatPattern}) (?<unit>\w{{2}})\r\n(?<status>E|<|>|:|NA)",
               (Match response) =>
                   new Tuple<float?, string, Status?>(
                       float.Parse(response.Groups["value"].Value, en),
                       response.Groups["unit"].Value,
                       (statusCodes.ContainsKey(response.Groups["status"].Value) ? (Status?)statusCodes[response.Groups["status"].Value] : null))
               );

        public static SimpleSerialCommandWithResponse<Tuple<float?, string, Status?>> GetTargetVolume() =>
           new SimpleSerialCommandWithResponse<Tuple<float?, string, Status?>>("voli?",
               $@"(?<value>{SerialCommands.FloatPattern}) (?<unit>\w{{2}})\r\n(?<status>E|<|>|:|NA)",
               (Match response) =>
                   new Tuple<float?, string, Status?>(
                       float.Parse(response.Groups["value"].Value, en),
                       response.Groups["unit"].Value,
                       (statusCodes.ContainsKey(response.Groups["status"].Value) ? (Status?)statusCodes[response.Groups["status"].Value] : null))
               );

        private static string getFormattedOutputValue(float value)
        {
            string format = "";
            if (value >= 1000) //until 5 characters are allowed
                format = "0";
            else if (value > 100)
                format = "0.0";
            else
                format = "0.00";
            return value.ToString(format);

        }

        public static SimpleSerialCommandWithResponse<Status?> SetTargetVolume(float targetVolume, string unit)
        {
            string sTargetVolume = getFormattedOutputValue(targetVolume);
            return SimpleSerialCommandWithPromptResponse($"voli {sTargetVolume} {unit}"); //unit should be ul or ml
        }

        public static SimpleSerialCommandWithResponse<Status?> SetInfusionRate(float rate, string unit) //maximum value 99999
        {
            string sRate = getFormattedOutputValue(rate);
            return SimpleSerialCommandWithPromptResponse($"ratei {sRate} {unit}"); //unit should be ul or ml
        }

        public static SimpleSerialCommandWithResponse<Tuple<float, string, Status?>> GetInfusionRate() =>
           new SimpleSerialCommandWithResponse<Tuple<float, string, Status?>>("ratei?",
               $@"(?<value>{SerialCommands.FloatPattern}) (?<unit>\w{{2}}/\w)\r\n(?<status>E|<|>|:|NA)",
               (Match response) =>
                   new Tuple<float, string, Status?>(
                       float.Parse(response.Groups["value"].Value, en),
                       response.Groups["unit"].Value,
                       (statusCodes.ContainsKey(response.Groups["status"].Value) ? (Status?)statusCodes[response.Groups["status"].Value] : null))
               );

        public static SimpleSerialCommandWithResponse<Tuple<int?, Status?>> GetError() =>
            new SimpleSerialCommandWithResponse<Tuple<int?, Status?>>("error?",
               $@"(?<value>{SerialCommands.IntPattern})\r\n(?<status>E|<|>|:|NA)",
               (Match response) =>
                    new Tuple<int?, Status?>(
                        int.Parse(response.Groups["value"].Value),
                        (statusCodes.ContainsKey(response.Groups["status"].Value) ? (Status?)statusCodes[response.Groups["status"].Value] : null))
                );
        public static SimpleSerialCommandWithResponse<Tuple<string, Status?>> GetVersion() =>
            new SimpleSerialCommandWithResponse<Tuple<string, Status?>>("prom?",
                $@"(?<value>{SerialCommands.FloatPattern})\r\n(?<status>E|<|>|:|NA)",
                (Match response) =>
                    new Tuple<string, Status?>(
                       response.Groups["value"].Value,
                        (statusCodes.ContainsKey(response.Groups["status"].Value) ? (Status?)statusCodes[response.Groups["status"].Value] : null))
                );
    }
}
