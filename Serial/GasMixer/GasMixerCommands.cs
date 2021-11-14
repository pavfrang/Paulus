using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Paulus.Serial.GasMixer
{
    public static class GasMixerCommands
    {
        /// <summary>
        /// Ports must be read prior to MFCs. All concentrations are returned in PPM. Concentrations over 6000 PPM are preferred to be shown in %.
        /// Example response:
        /// 1000000.,5000.0,5000.0,5000.0,5000.0,5000.0,5000.0,5000.0,5000.0,5000.0,5000.0,5000.0
        /// 800000.0,5000.0,5000.0,5000.0,5000.0,5000.0,5000.0,5000.0,5000.0,5000.0,5000.0,5000.0
        /// </summary>
        /// <returns></returns>
        public static SimpleSerialCommandWithResponse<List<float>> ReadPortCylinderConcentrations() =>
            SerialCommands.CreateWithListFloatResponse("PORT ALL CONC ?");

        /// <summary>
        /// Read list of K factors assigned to all ports.
        /// </summary>
        /// <returns></returns>
        public static SimpleSerialCommandWithResponse<List<float>> ReadPortKFactors() =>
            SerialCommands.CreateWithListFloatResponse("PORT ALL K ?");

        public static SimpleSerialCommandWithResponse<int?> ReadMfcCount() =>
            SerialCommands.CreateWithIntResponse("NUMBER MFC ?");
        //new SimpleSerialCommandWithResponse<int?>("NUMBER MFC ?", intPattern, getInt);

        /// <summary>
        /// Read the MFC size in CCM.
        /// Example response: 30000.0
        /// </summary>
        public static SimpleSerialCommandWithResponse<float?> ReadMfcSize(int mfc) =>
            SerialCommands.CreateWithFloatResponse($"SIZE {mfc} ?");
        //new SimpleSerialCommandWithResponse<float?>($"SIZE {mfc} ?", floatPattern, getFloat);



        #region Concentration mode
        //flow in ccm
        public static SimpleSerialCommandWithResponse<float?> ReadFlowTotalTarget() =>
            SerialCommands.CreateWithFloatResponse("CONC TOT TARGET ?");
        //new SimpleSerialCommandWithResponse<float?>("CONC TOT TARGET ?", floatPattern, getFloat);

        public static SimpleSerialCommand ConcentrationUpdate() => new SimpleSerialCommand("CONC UPDATE"); //tested

        public static SimpleSerialCommandWithResponse<List<float>> ReadConcentrationAllActual() =>
            SerialCommands.CreateWithListFloatResponse("CONC ALL ACTUAL ?");
        //new SimpleSerialCommandWithResponse<List<float>>("CONC ALL ACTUAL ?", floatListPattern, getFloats);

        //flow in ccm always
        public static SimpleSerialCommandWithResponse<float?> ReadFlowTotalActual() =>
            SerialCommands.CreateWithFloatResponse("CONC TOT ACTUAL ?");
        //new SimpleSerialCommandWithResponse<float?>("CONC TOT ACTUAL ?", floatPattern, getFloat);

        public static SimpleSerialCommand AssignTargetConcentration(int mfc, float targetConcentrationInPpm) =>
                new SimpleSerialCommand($"CONC {mfc} TARGET = {targetConcentrationInPpm:0.0}");
        public static SimpleSerialCommand AssignBalance(int mfc) =>
                new SimpleSerialCommand($"CONC BALANCE = {mfc}");
        public static SimpleSerialCommand AssignTotalTargetFlow(float flowInCcm) =>
                new SimpleSerialCommand($"CONC TOT TARGET = {flowInCcm:0.0}");

        public static SimpleSerialCommandWithResponse<float?> ReadTargetConcentration(int mfc) =>
            SerialCommands.CreateWithFloatResponse($"CONC {mfc} TARGET ?");



        #endregion

        #region Purge Mode
        public static SimpleSerialCommand PurgeUpdate() =>
            new SimpleSerialCommand("PURGE UPDATE");

        public static SimpleSerialCommand SetPurgeOn(int mfc) =>
            new SimpleSerialCommand($"PURGE {mfc} ON");
        public static SimpleSerialCommand SetPurgeOff(int mfc) =>
            new SimpleSerialCommand($"PURGE {mfc} OFF");

        public static SimpleSerialCommand SetTargetPurgeFlow(int mfc, float flowInCcm) =>
                new SimpleSerialCommand($"PURGE {mfc} TARGET = {flowInCcm:0.0}");

        #endregion

        #region Miscellaneous commands
        //Example response:
        //1,2,3,4,5,10,12,15,2
        public static SimpleSerialCommandWithResponse<List<int>> ReadPortAssignments() =>
            SerialCommands.CreateWithListIntResponse("PORT ALL MFC ?");
        //new SimpleSerialCommandWithResponse<List<int>>("PORT ALL MFC ?", intListPattern, getInts);

        public static SimpleSerialCommandWithResponse<List<int>> ReadValidPorts(int mfc) =>
            SerialCommands.CreateWithListIntResponse($"VALID PORT {mfc} ?");
        // new SimpleSerialCommandWithResponse<List<int>>($"VALID PORT {mfc} ?", intListPattern, getInts);

        public static SimpleSerialCommand AssignPortToMfc(int port, int mfc) =>
            new SimpleSerialCommand($"PORT {port} MFC = {mfc}");
        public static SimpleSerialCommand AssignPortCylinderName(int port, string cylinderName)
        {
            return cylinderName.Length <= 35 ?
                new SimpleSerialCommand($"PORT {port} TYPE = \"{cylinderName}\"") :
                new SimpleSerialCommand($"PORT {port} TYPE = \"{cylinderName.Substring(0, 35)}\"");
        }

        public static SimpleSerialCommand AssignPortCylinderKFactor(int port, float kFactor) =>
            new SimpleSerialCommand($"PORT {port} K = {kFactor:#0.000}");
        public static SimpleSerialCommand AssignPortCylinderConcentration(int port, float concentrationInPpm) =>
            concentrationInPpm < 1000000.0 ?
                new SimpleSerialCommand($"PORT {port} CONC = {concentrationInPpm:#0.0}") :
                new SimpleSerialCommand($"PORT {port} CONC = 1000000.0");


        /// <summary>
        /// Read Warnings for each MFC.
        /// </summary>
        /// <returns></returns>
        public static SimpleSerialCommandWithResponse<List<int>> ReadWarnings() =>
             SerialCommands.CreateWithListIntResponse("WARNINGS ?");
        //new SimpleSerialCommandWithResponse<List<int>>("WARNINGS ?", intListPattern, getInts);

        public static SimpleSerialCommand Home() => new SimpleSerialCommand("HOME"); //tested

        public static SimpleSerialCommand Stop() => new SimpleSerialCommand("STOP"); //tested

        public static SimpleSerialCommandWithResponse<bool?> ReadIsLowFlow() => new SimpleSerialCommandWithResponse<bool?>(
            "LOWFLOW ?", "0|1", (string response) => { return response == "1"; });

        public static SimpleSerialCommandWithResponse<string> ReadPortCylinderId(int port) =>
            new SimpleSerialCommandWithResponse<string>($"PORT {port} TYPE ?",
                @"#(?<id>(\w|\d)+)", (Match match) => match.Success ? match.Groups["id"].Value : "");


        #region Date and Time
        //Example response:
        //3,53,7

        public static SimpleSerialCommandWithResponse<TimeSpan?> ReadTime() => new SimpleSerialCommandWithResponse<TimeSpan?>(
            "TIME ?", "(?<hour>\\d{1,2}),(?<minute>\\d{1,2}),(?<second>\\d{1,2})",
            (Match mTime) =>
            {
                return new TimeSpan(
                    int.Parse(mTime.Groups["hour"].Value),
                    int.Parse(mTime.Groups["minute"].Value),
                    int.Parse(mTime.Groups["second"].Value));
            }); //tested

        //Example response:
        //19,12,16
        public static SimpleSerialCommandWithResponse<DateTime?> ReadDate() => new SimpleSerialCommandWithResponse<DateTime?>(
            "DATE ?", "(?<day>\\d{2}),(?<month>\\d{1,2}),(?<year>\\d{2})",
            (Match mDate) =>
            {
                return new DateTime(
                    2000 + int.Parse(mDate.Groups["year"].Value),
                    int.Parse(mDate.Groups["month"].Value),
                    int.Parse(mDate.Groups["day"].Value));
            }); //tested

        public static SerialDoubleCommandWithResponse<TimeSpan?, DateTime?, DateTime?> ReadTimeAndDate() =>
            new SerialDoubleCommandWithResponse<TimeSpan?, DateTime?, DateTime?>(
                ReadTime(), ReadDate(), (time, date) => date + time); //tested


        #endregion




        #endregion
    }
}
