using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Paulus.IO
{
    public static class StreamReaderExtensions
    {
        [Serializable]
        public class FileEmptyException : IOException
        {
            public FileEmptyException() { }
            public FileEmptyException(string message) : base(message) { }
            public FileEmptyException(string message, Exception inner) : base(message, inner) { }

            public FileEmptyException(string message, string fileName) : base(message)
            {
                _fileName = fileName;
            }

            public FileEmptyException(string message, string fileName, Exception innerException) :this(message,innerException)
            {
                _fileName = fileName;
            }

            protected FileEmptyException(
              global::System.Runtime.Serialization.SerializationInfo info,
              global::System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }

            protected string _fileName;
            public string FileName { get { return _fileName; } }
        }

        /// <summary>
        /// Returns specific lines from a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="encoding">The encoding of the file.</param>
        /// <param name="lineNumbers">The numbers of the lines to be exported.</param>
        /// <returns>A dictionary with the requested lines.</returns>
        /// <exception cref="Paulus.System.StreamReaderExt.FileEmptyException"/>
        public static Dictionary<int, string> ReadLines(string path, Encoding encoding, params int[] lineNumbers)
        {
            if (new FileInfo(path).Length == 0) throw new FileEmptyException(path);

            Dictionary<int, string> lines = new Dictionary<int, string>();

            //check if the file size is zero!
            if (lineNumbers.Length > 0)
            {
                //sort the lines
                List<int> sortedLineNumbers = lineNumbers.ToList<int>();
                sortedLineNumbers.Sort();

                using (StreamReader reader = new StreamReader(path, encoding))
                {
                    int firstLineNumber = sortedLineNumbers[0];
                    //reader.OmitLines(firstLineNumber - 1);
                    for (int iLine = 0; iLine < firstLineNumber - 1; iLine++) reader.ReadLine();
                    lines.Add(firstLineNumber, reader.ReadLine().Trim('\0'));

                    int lineNumber = firstLineNumber;

                    //read the remaining lines
                    for (int i = 1; i < sortedLineNumbers.Count; i++)
                    {
                        int previousLineNumber = lineNumber;
                        lineNumber = sortedLineNumbers[i];

                        //omit duplicate keys
                        if (!lines.ContainsKey(lineNumber))
                        {
                            // reader.OmitLines(lineNumber - previousLineNumber - 1);
                            for (int iLine = 0; iLine < lineNumber - previousLineNumber - 1; iLine++) reader.ReadLine();
                            string line = reader.ReadLine().Trim('\0');
                            lines.Add(lineNumber, line);
                        }
                    }

                }
            }

            return lines;
        }
        public static Dictionary<int, string> ReadLines(string path, params int[] lineNumbers)
        {
            return ReadLines(path, Encoding.Default, lineNumbers);
        }

        /// <summary>
        /// Jumps the number of lines that is specified.
        /// </summary>
        /// <param name="reader">The reader object.</param>
        /// <param name="count">The number of lines to jump.</param>
        public static void OmitLines(this StreamReader reader, int count)
        {
            for (int iLine = 0; iLine < count; iLine++) reader.ReadLine();
        }

        public static string ReadLine(string path, int lineNumber, Encoding encoding)
        {
            string line = "";
            using (StreamReader reader = new StreamReader(path, encoding))
            {
                if (lineNumber > 1) OmitLines(reader, lineNumber - 1);
                line = reader.ReadLine();
            }
            return line;
        }

        public static string ReadFirstNonEmptyOrCommentLine(this StreamReader reader, char commentCharacter)
        {
            while (!reader.EndOfStream)
            {
                string ln = reader.ReadLine().Trim();
                int commentCharacterPosition = ln.IndexOf(commentCharacter);
                if (ln.Length > 0)
                {
                    if (commentCharacterPosition >= 1) return ln.Substring(commentCharacterPosition + 1);
                    else if (commentCharacterPosition == -1) return ln;
                }
            }
            return "";
        }

        public static string ReadLastNonEmptyLine(string path)
        {
            string lastNonEmptyLine = "";
            using (StreamReader reader = new StreamReader(path))
            {

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line)) lastNonEmptyLine = line.Trim();
                }
            }
            return lastNonEmptyLine;
        }

        public static string ReadFirstNonEmptyLine(this StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                string ln = reader.ReadLine().Trim();
                if (ln.Length > 0) return ln;
            }
            return "";
        }

        ////read consecutive entries until an empty line or the end of file is met (DUPLICATES ARE IGNORED)
        ////project=...
        ////sourcefile=...
        //public static Dictionary<string, string> ReadDictionary(this StreamReader reader, char keyValueSeparator = '=', bool untilAnEmptyLine = false, char? commentCharacter = null)
        //{
        //    Dictionary<string, string> dic = new Dictionary<string, string>();
        //    while (!reader.EndOfStream)
        //    {
        //        string line = reader.ReadLine().Trim();
        //        if (commentCharacter.HasValue)
        //            line = line.SubstringBeforeChar(0, commentCharacter.Value, true).Trim();

        //        if (untilAnEmptyLine && line.Length == 0) break;
        //        if (line.Contains(keyValueSeparator))
        //        {
        //            string key = line.SubstringBeforeChar(0, keyValueSeparator, false);
        //            if (key.Length > 0 && !dic.ContainsKey(key))
        //            {
        //                string value = line.SubstringAfterChar(0, keyValueSeparator);
        //                dic.Add(key, value);
        //            }
        //        }
        //    }
        //    return dic;
        //}

        /// <summary>
        /// Returns a specific line from the file using the default ANSI code page.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public static string ReadLine(string path, int lineNumber)
        {
            return ReadLine(path, lineNumber, Encoding.Default);
        }

        /// <summary>
        /// Returns a block of consecutive lines.
        /// </summary>
        /// <param name="path">The path of the file to read the lines from.</param>
        /// <param name="fromLineNumber">The starting line number. Line counting begins at 1 and it is measured from the current position of the StreamReader.</param>
        /// <param name="toLineNumber">The ending line number. This number must not be less than the <paramref name="fromLineNumber"/>.</param>
        /// <param name="encoding">The encoding to be used</param>
        /// <returns></returns>
        public static string[] ReadLineBlock(string path, Encoding encoding, int fromLineNumber = 1, int toLineNumber = int.MaxValue)
        {
            string[] lineBlock;
            try
            {
                using (StreamReader reader = new StreamReader(path, encoding)) lineBlock = reader.ReadLineBlock(fromLineNumber, toLineNumber);
            }
            catch { throw; }

            return lineBlock;
        }

        public static string[] ReadLineBlock(string path, int fromLineNumber = 1, int toLineNumber = int.MaxValue)
        {
            return ReadLineBlock(path, Encoding.Default, fromLineNumber, toLineNumber);
        }

        /// <summary>
        /// Returns a line block by defining optionally a starting and an ending line.
        /// </summary>
        /// <param name="reader">The StreamReader object.</param>
        /// <param name="fromLineNumber">The starting line number. Line counting begins at 1 and it is measured from the current position of the StreamReader.</param>
        /// <param name="toLineNumber">The ending line number. This number must not be less than the <paramref name="fromLineNumber"/>.</param>
        /// <returns>A string array of the lines.</returns>
        public static string[] ReadLineBlock(this StreamReader reader, int fromLineNumber = 1, int toLineNumber = int.MaxValue)
        {
            #region Error checking
            if (reader == null) throw new ArgumentNullException("reader", "The reader cannot be null.");

            if (fromLineNumber < 1) throw new ArgumentOutOfRangeException("fromLineNumber", fromLineNumber,
                    string.Format("The starting line number is equal to {0}. The starting line number must be 1 or greater.", fromLineNumber));

            if (toLineNumber < fromLineNumber) throw new ArgumentOutOfRangeException("toLineNumber", toLineNumber,
                   string.Format(@"The ending line number is equal to {0} and the starting line number is equal to {1}.
The ending line number must be more than or equal to the starting line number.", toLineNumber, fromLineNumber));
            #endregion

            int lineCount = toLineNumber - fromLineNumber + 1;
            List<string> lineBlock = new List<string>();

            if (fromLineNumber > 1) OmitLines(reader, fromLineNumber - 1);

            int iLine = 0;
            while (!reader.EndOfStream)
                if (iLine++ < lineCount) lineBlock.Add(reader.ReadLine());

            return lineBlock.ToArray();
        }

        public static int LinesCount(string path, bool untilAnyEmptyLine = false)
        {
            int iLine = 0;
            using (StreamReader reader = new StreamReader(path))
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (untilAnyEmptyLine && line.Trim().Length == 0) break;
                    iLine++;
                }
            return iLine;
        }

       
        /// <summary>
        /// Copies the number of lines specified from one stream to another.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="target">The target stream.</param>
        /// <param name="count">The number of lines to copy.</param>
        public static void CopyLinesTo(this StreamReader source, StreamWriter target, int count)
        {
            for (int iLine = 0; iLine < count; iLine++) target.WriteLine(source.ReadLine());
        }

        public static void CopyLinesTo(this StreamReader source, StreamWriter target, string untilYouFindThis, bool copyLastLine = true)
        {
            while (!source.EndOfStream)
            {
                string line = source.ReadLine();
                if (!line.Contains(untilYouFindThis))
                    target.WriteLine(line);
                else //found stop condition
                {
                    if (copyLastLine) target.WriteLine(line);
                    break;
                }
            }
        }

        //1253 windows-1253 Greek (Windows) 
        //28597 iso-8859-7 Greek (ISO) 
        public static Encoding GreekWindowsEncoding
        {
            get { return Encoding.GetEncoding(1253); }
        }

        public static Encoding GreekISOEncoding
        {
            get { return Encoding.GetEncoding(28597); }
        }

        //used when reading from files
        public static Encoding WestEuropeanEncoding { get { return Encoding.GetEncoding(1252); } }

        //used in conversions from string to numeric values
        public static CultureInfo EnglishCulture { get { return CultureInfo.InvariantCulture; } }

        public static IEnumerable<EncodingInfo> EncodingsSortedByDisplayName
        {
            get
            {
                EncodingInfo[] encodings = Encoding.GetEncodings();
                //encodings.OrderBy((x) => { return x.DisplayName.ToLower(); }).ToArray();
                return  from e in encodings orderby e.DisplayName select e;
            }
        }


    }
}
