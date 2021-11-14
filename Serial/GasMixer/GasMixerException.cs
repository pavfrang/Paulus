using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Paulus.Serial.GasMixer
{
    public class GasMixerException : System.Exception
    {
        public GasMixerException(int code, string commandSent) :
            base(GetShortErrorMessageSafe(code))
        {
            CommandSent = commandSent;
            Code = code;
        }

        public GasMixerException(Exception innerException, string commandSent) :
            base(innerException.Message, innerException)
        {
            CommandSent = commandSent;
            Code = UndefinedCode;
        }
        public GasMixerException(string message, Exception innerException, string commandSent) :
            base(message, innerException)
        {
            CommandSent = commandSent;
            Code = UndefinedCode;
        }

        public const int UndefinedCode = -1;

        public int Code { get; }

        public string CommandSent { get; }

        //// This constructor is needed for serialization.
        //protected GasMixerException(SerializationInfo info, StreamingContext context)
        //{
        //    // Add implementation.
        //}

        public string LongDescription { get { return GetLargeErrorMessageSafe(Code); } }

        public static string GetShortErrorMessageSafe(int code) =>
            ShortErrorMessages.ContainsKey(code) ? ShortErrorMessages[code] : "Undefined Gas Mixer Exception";

        public static Dictionary<int, string> ShortErrorMessages = new Dictionary<int, string>
        {
            {3,"Double STX Received"},
            {4,"Command Overflow"},
            {5,"Save Out Of Range"},
            {6,"Recall Out Of Range"},
            {7,"Zero Flow Out Of Range"},
            {8,"Divider Mode Span Or Diluent Flow Out Of Range"},
            {9,"Illegal Span Port Selection"},
            {10,"Crossover Point Out Of Range"},
            {11,"Illegal Port Selection"},
            {12,"Unknown Command"},
            {13,"Divider Step Number Is Out Of Range"},
            {14,"Divide Step Time Out Of Range"},
            {15,"Divider Step Total Flow Out Of Range"},
            {16,"Divider Step Percent Out Of Range"},
            {17,"Divider Step Is Undefined"},
            {18,"Illegal Command"},
            {19,"Undefined Error"},
            {20,"Attempt To Initialize An Unknown Step"},
            {21,"Unknown Divider Init Command"},
            {22,"Illegal Port Change"},
            {23,"Ozone Error"},
            {24,"Illegal Port Selected"},
            {25,"Flow Out Of Range"},
            {26,"Purge Or Flow Operating"},
            {27,"Purge Or Concentration Mode Operating"},
            {28,"Bad Flow All Command"},
            {29,"Total Flow Out Of Range"},
            {30,"Bad Total Flow Command"},
            {31,"MFC Selection Out Of Range"},
            {32,"Bad Flow Command"},
            {33,"Bad Conc All Command"},
            {34,"Concentration Out Of Range"},
            {35,"Bad Concentration Out Of Range"},
            {36,"Bad Ozone Command"},
            {37,"Concentration Or Flow Mode Active"},
            {38,"Bad Purge Command"},
            {39,"Port Selection Is Out Of Range"},
            {40,"MFC Cannot Be Assigned To Requested Port"},
            {41,"Invalid Port Select"},
            {42,"K Factor Out Of Range"},
            {43,"Concentration Out Of Range"},
            {44,"Bad Port All Command"},
            {45,"Bad Port Command"},
            {46,"Calibration Point Number Out Of Range"},
            {47,"Bad Calibration Command"},
            {48,"Bad Valid Command"},
            {49,"Bad Time Data"},
            {50,"Bad Time Command"},
            {51,"Bad Time Command"},
            {52,"Bad Date Data"},
            {53,"Bad Date Command"},
            {54,"Item Number Out Of Range"},
            {55,"Run Time Out Of Range"},
            {56,"Mode Request Out Of Range"},
            {57,"Bad Function Command"},
            {58,"Starting Hour Is Out Of Range"},
            {59,"Starting Minute Out Of Range"},
            {60,"Starting Day Out Of Range"},
            {61,"Start Time Over 24 Hours"},
            {62,"Bad Procedure Command"},
            {63,"Bad Calibration Target Command"},
            {64,"Calibration Set Or True Out Of Range"},
            {65,"Bad NOx Step Command"},
            {66,"Ppm Out Or Range"},
            {67,"NOx Step Percentage Is Out Of Range"},
            {68,"NOx Step Out Of Range"},
            {69,"NOx Scale Out Of Range"},
            {70,"NOx Duplicate MFC Selected"},
            {71,"NOx Duplicate Balance No MFC"},
            {72,"Component Select Out Of Range"},
            {192,"Illegal Character Received"},
            {998,"Device Write Timeout"},
            {999,"Device Response Timeout"},
            {128,"Data Transmission Errors"},
            {129,"Data Transmission Errors"},
            {130,"Data Transmission Errors"},
            {131,"Data Transmission Errors"},
            {132,"Data Transmission Errors"},
            {133,"Data Transmission Errors"},
            {134,"Data Transmission Errors"},
            {135,"Data Transmission Errors"},
            {136,"Data Transmission Errors"},
            {137,"Data Transmission Errors"},
            {138,"Data Transmission Errors"},
            {139,"Data Transmission Errors"},
            {140,"Data Transmission Errors"},
            {141,"Data Transmission Errors"},
            {142,"Data Transmission Errors"},
            {143,"Data Transmission Errors"}
        };

        public static string GetLargeErrorMessageSafe(int code) =>
            LargeErrorMessages.ContainsKey(code) ? LargeErrorMessages[code] : "Undefined gas mixer exception.";


        public static Dictionary<int, string> LargeErrorMessages = new Dictionary<int, string>
        {
            {3,"Two consecutive <STX> codes were received."},
            {4,"The input buffer of the remote command processor has overflowed."},
            {5,"Save location out of range."},
            {6,"Recall or query location out of range."},
            {7,"The flow specified for divider ZERO is out of range."},
            {8,"The flow for span or diluent in a divider step is illegal."},
            {9,"The port designated as Span port in divider mode is illegal."},
            {10,"The divider crossover percentage point is out of range."},
            {11,"An illegal port selection has been detected."},
            {12,"Unknown command."},
            {13,"The step number specified for a divider step is out of range."},
            {14,"The time value specified for a divider step is out of range."},
            {15,"The total flow for a divider step is out or range."},
            {16,"The percentage specified for a divider step is out of range."},
            {17,"The specified DIVIDER STEP does not exist."},
            {18,"May be returned if an attempt is made to perform an operation which conflicts with the current operating mode."},
            {19,"Unknown error."},
            {20,"An attempt has been made to reference an undefined step."},
            {21,"The instrument received a command to initalize an unknown item. Check command for valid syntax."},
            {22,"An illegal port assignment has been attempted of an attempt has been made to change a port setting while in an mode other than HOME."},
            {23,"An error has been detected in the last Ozone command. Chek for proper syntax and values in command string."},
            {24,"The attempted MFC/Port assignment is invalid."},
            {25,"An attempt has been made to perform a flow assignment which is out of range."},
            {26,"An attempt has been made to perform a non Purge of Flow command while a Purge or Flow mode is operating."},
            {27,"An attempt has ben made to perform a non Purge or Concentration command while a Purge or Concentration mode is operating."},
            {28,"An error has been detected in the construction of a FLOW ALL command. Check for proper command syntax."},
            {29,"The requested Total Flow is out of range."},
            {30,"An error has been detected in the construction of a Total Flow command. Check command for proper syntax."},
            {31,"Either an attempt has been made to select an MFC which is out of range or the MFC number field contains an invalid character. Recheck MFC number and command syntax."},
            {32,"An error in a FLOW command has been detected. Chack command syntax."},
            {33,"An error has been detected in the CONC ALL command. Check command syntax."},
            {34,"The commanded concentration is out of range."},
            {35,"An error has been detected in the construction of a concentration mode command. Check command syntax."},
            {36,"An error has been detected in an OZONE command. Check command syntax."},
            {37,"A command has been received to perform a non concentration of flow mode operation while concentration of flow mode is operating."},
            {38,"An error has been detected in a PURGE command. Check command syntax."},
            {39,"A port selection command has been received with a port selection which is out of range."},
            {40,"An attempt has been made to assign an MFC to a port which is illegal. Recheck legal Port/MFC combinations."},
            {41,"An unknown or illegal port selection command has been received."},
            {42,"An out of range K factor has been received. Recheck K factor setting and command syntax."},
            {43,"An out of range concentration has been received. Recheck K factor setting and command syntax."},
            {44,"An error has been detected in the construction of a PORT ALL command. Check for proper command syntax."},
            {45,"An error has been detected in the construction of a PORT command. Check for proper command syntax."},
            {46,"A calibration point number has been received which is out of range. Recheck command syntax and point number being requested."},
            {47,"A calibration point number has been received which is out of range. Recheck command syntax and point number being requested."},
            {48,"An error has been detected in the construction of a VALID command. Check for proper command syntax."},
            {49,"Time data is invalid or out of range."},
            {50,"Time date is invalid or out of range."},
            {51,"An error has been detected in the construction of a TIME command. Check for proper command syntax."},
            {52,"DATE data is invalid or out of range."},
            {53,"An error has been detected in the cinstruction of a DATE command. Check for proper command syntax."},
            {54,"Requested ITEM Number is out of range."},
            {55,"Run Time setting is out of range."},
            {56,"Requested Mode is out of range."},
            {57,"An errror has been detected in the construction of a FUNCTION command. Check for proper command syntax."},
            {58,"The requested starting hour is out of range."},
            {59,"The requested starting day is out of range."},
            {60,"The requested starting day is out of range."},
            {61,"The requested start time is over 24 hours."},
            {62,"An error has been detected in the construction of a PROCEDURE command. Check for proper command syntax."},
            {63,"An error has been detected in the construction of a CALIBRATION command. Check for proper command syntax."},
            {64,"A calibration command has been received with the set or true point out of range."},
            {65,"Step number out of range, or incorrect syntax for NOx command."},
            {66,"A requested concentration setting is out of range."},
            {67,"A requested NOx step percentage is out of range."},
            {68,"The requested NOx Step number is out of range."},
            {69,"The requested NOx Scale Setting is out of range."},
            {70,"The NOx test has detected an attempt to use an MFC for more than one purpose. A duplicate MFC may be assigned to NO flow and Diluent flow only when no diluent is required to decrease concentration of NO from that supplied to the instrument."},
            {71,"The NOx test has detected an attempt to use an MFC for more then one purpose. A duplicate MFC may be assigned to NO flow and Diluent flow only when no diluent is required to decrease concentration of NO from that supplied to the instrument."},
            {72,"(9100 only) The requested gas component is out of range. The gas component must be within the range from 1 to 6."},
            {192,"An illegal character has been received within the command issued by the host. This error may be caused by invalid commands or data transmission errors."},
            {998,"The device write operation is timed out."},
            {999,"The device response is timed out."},
            {128,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {129,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {130,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {131,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {132,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {133,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {134,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {135,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {136,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {137,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {138,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {139,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {140,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {141,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {142,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."},
            {143,"An error has been detected by the instrument's serial communication hardware. Check serial communications settings for proper baud rate, number of bits, and number of stop bits."}

        };

    }
}
