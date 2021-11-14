using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace Paulus.IO
{
    public static class TraceExtensions
    {
        #region Logging
        public static string GetLogLine(string filename, string message)
        {
            return string.Format(CultureInfo.InvariantCulture,
              "{0:yyyy-MMM-dd HH:mm:ss.fff} {1} '{2}'", DateTime.Now, message, filename);
        }

        public static string GetLogLine(string action)
        {
            return string.Format(CultureInfo.InvariantCulture,
                          "{0:yyyy-MMM-dd HH:mm:ss.fff} {1}", DateTime.Now, action);
        }

        public static void WriteStampedLine(this TextWriterTraceListener writer, string action)
        {
            writer.WriteLine(GetLogLine(action));
        }

        public static void WriteStampedLine(this TextWriterTraceListener writer, string fileName, string message)
        {
            writer.WriteLine(GetLogLine(fileName, message));
        }
        #endregion
    }
}
