using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Globalization;
using System.IO;
using System.Reflection;
using Paulus.Common;

namespace Paulus.Excel
{
    public struct ExcelSimpleColumnInfo
    {
        public string Header;
        public int Column;
        public string ExcelLetters;

        public override string ToString()
        {
            return string.Format("{0} (Column: {1}-{2})", Header, ExcelLetters,Column );
        }
    }

    public static class ExcelColumnHeaders
    {
        //used by getcolumnheaders
        public static CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        public static Dictionary<string, ExcelSimpleColumnInfo> GetColumnSimpleInfos(this Worksheet sheet,
            int row = 1, int startColumn = 1, bool rangeContainsEmptyColumns = false, int lastColumn = 100, CultureInfo cultureInfo = null)
        {
            Dictionary<string, ExcelSimpleColumnInfo> dic = new Dictionary<string, ExcelSimpleColumnInfo>();

            Range startCell = sheet.Cells[row, startColumn];
            Range nextCell = startCell.Offset[0, 1];

            if (!rangeContainsEmptyColumns)
                lastColumn = nextCell.Value != null ? startCell.End[XlDirection.xlToRight].Column : startColumn;

            Range lastCell = sheet.Cells[row, lastColumn];

            for (int iColumn = startColumn; iColumn <= lastColumn; iColumn++)
                if (sheet.Cells[row, iColumn].Value != null) //empty cells have null Value
                {
                    dynamic value = sheet.Cells[row, iColumn].Value;
                    if (value is string)
                    {
                        if (!dic.ContainsKey(value))
                            dic.Add(value, new ExcelSimpleColumnInfo() { Column = iColumn, ExcelLetters = iColumn.ToExcelColumnLetters(), Header = value });
                    }
                    else if (value is double || value is bool || value is DateTime)
                    {
                        if (cultureInfo == null) cultureInfo = InvariantCulture; //null info means the invariantculture (not the current one)
                        string sValue = value.ToString(cultureInfo);
                        if (!dic.ContainsKey(sValue))
                            dic.Add(sValue, new ExcelSimpleColumnInfo() { Column = iColumn, ExcelLetters = iColumn.ToExcelColumnLetters(), Header = value });
                    }
                }

            return dic;
        }


        //cell.Value is null when empty or if it contains an error
        //cell.Value is string if it contains a string
        //cell.Value is a double/date when it contains a number/date. If a date is used you may take the date from DateTime.FromOADate(value)
        //cell.Value is bool if it contains a bool value
        public static List<string> GetColumnHeaders(this Worksheet sheet,
            int row = 1, int startColumn = 1, bool rangeContainsEmptyColumns = false, int lastColumn = 100, CultureInfo cultureInfo = null)
        {
            Range startCell = sheet.Cells[row, startColumn];
            Range nextCell = startCell.Offset[0, 1];

            if (!rangeContainsEmptyColumns)
                lastColumn = nextCell.Value != null ? startCell.End[XlDirection.xlToRight].Column : startColumn;

            Range lastCell = sheet.Cells[row, lastColumn];

            List<string> headers = new List<string>();
            for (int iColumn = startColumn; iColumn <= lastColumn; iColumn++)
                if (sheet.Cells[row, iColumn].Value != null) //empty cells have null Value
                {
                    dynamic value = sheet.Cells[row, iColumn].Value;
                    if (value is string)
                        headers.Add(value);
                    else if (value is double || value is bool || value is DateTime)
                    {
                        if (cultureInfo == null) cultureInfo = InvariantCulture; //null info means the invariantculture (not the current one)
                        headers.Add(value.ToString(cultureInfo));
                    }
                }
            return headers;
        }

        public static List<string> ExportHeadersToFile(string[] files, object sheetNameOrIndex, string headersFile)
        {
            Application xlApp = new Application();
            xlApp.ScreenUpdating = false;
            xlApp.DisplayAlerts = false;
            List<string> headers = new List<string>();
            using (StreamWriter writer = new StreamWriter(headersFile))
            {
                for (int iFile = 0; iFile < files.Length; iFile++)
                {
                    Workbook wb = xlApp.Workbooks.Open(files[iFile]);
                    Worksheet ws = wb.Worksheets[sheetNameOrIndex];
                    List<string> columnHeaders = ws.GetColumnHeaders();
                    foreach (string columnHeader in columnHeaders)
                        if (!headers.Contains(columnHeader))
                        {
                            headers.Add(columnHeader);
                            writer.WriteLine(columnHeader);
                        }
                }
            }

            xlApp.Quit();
            return headers;
        }

        /// <summary>
        /// Reads a file that contains labels all separated by \r\n. All empty lines are ignored. Optionally comments are omitted.
        /// </summary>
        /// <param name="path">The path of the text file to read.</param>
        /// <param name="ignoreComments">If true then comment lines are omitted. Lines that contain partial comments are trimmed.</param>
        /// <param name="commentCharacter">The comment character which is taken into account if the ignoreComments is set to true.</param>
        /// <returns>A string list of the labels in the file.</returns>
        public static List<string> ReadLabelsFromTextFile(string path, bool ignoreComments = true, char commentCharacter = ';')
        {
            List<string> headers = new List<string>();
            using (StreamReader reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine().Trim();
                    if (ignoreComments && line.Contains(';')) line = line.Substring(0, line.IndexOf(commentCharacter));
                    if (line.Length > 0) headers.Add(line);
                }
            }
            return headers;
        }

        /// <summary>
        /// Returns true if a header is used at the specified worksheet.
        /// </summary>
        /// <param name="sheet">The worksheet to be checked for the header existence.</param>
        /// <param name="headerToCheck">The header text to check.</param>
        /// <param name="isPartial">If true then it searches for partial match.</param>
        /// <param name="headersRow">The row that contains the headers (default is 1).</param>
        /// <param name="startColumn">The start column of the headers to be checked (default is 1).</param>
        /// <param name="endColumn">The end column of the headers to be checked. If omitted then the last column in the same region is taken.</param>
        /// <param name="caseSensitive">If true then the comparison is case sensitive (default is false).</param>
        /// <returns>true if the header to check is included in the sheet.</returns>
        public static bool HeaderIsUsed(this Worksheet sheet, string headerToCheck, bool isPartial = false, int headersRow = 1, int startColumn = 1, int endColumn = -1, bool caseSensitive = false)
        {
            if (endColumn < startColumn) endColumn = sheet.GetLastColumn((Range)sheet.Cells[headersRow, startColumn]);

            StringComparison comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            for (int iColumn = endColumn; iColumn >= startColumn; iColumn--)
            {
                Range headerCell = sheet.Cells[headersRow, iColumn];
                if (headerCell.Value != null) //ignore empty headers
                {
                    string header = headerCell.Value;
                    bool found = isPartial ? header.IndexOf(headerToCheck, comparison) >= 0 : header.Equals(headerToCheck, comparison);
                    if (found) return true;
                }
            }
            return false;
        }

        public static int GetHeaderColumn(this Worksheet sheet, string headerToCheck, bool isPartial = false, int headersRow = 1, int startColumn = 1, int endColumn = -1, bool caseSensitive = false)
        {
            if (endColumn < startColumn) endColumn = sheet.GetLastColumn((Range)sheet.Cells[headersRow, startColumn]);

            StringComparison comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            for (int iColumn = endColumn; iColumn >= startColumn; iColumn--)
            {
                Range headerCell = sheet.Cells[headersRow, iColumn];
                if (headerCell.Value != null) //ignore empty headers
                {
                    string header = headerCell.Value;
                    bool found = isPartial ? header.IndexOf(headerToCheck, comparison) >= 0 : header.Equals(headerToCheck, comparison);
                    if (found) return iColumn;
                }
            }
            return -1;
        }

        public static T GetFileTypeBasedOnHeader<T>(this Worksheet shData, Dictionary<string, T> headersToFileType, T defaultIfNotFound, bool isPartial = false, int headersRow = 1, int startColumn = 1, int endColumn = -1, bool caseSensitive = false)
        {
            foreach (var entry in headersToFileType)
            {
                if (shData.HeaderIsUsed(entry.Key, isPartial, headersRow, startColumn, endColumn, caseSensitive))
                    return entry.Value;
            }
            return defaultIfNotFound;
        }

        /// <summary>
        /// Used by KeepOrRemoveColumns. The mode defines whether columns are kept or removed.
        /// </summary>
        public enum KeepColumnsMode
        {
            /// <summary>
            /// Keep the column with the header.
            /// </summary>
            Keep,
            /// <summary>
            /// Remove the column with the header.
            /// </summary>
            Remove
        }

        /// <summary>
        /// Keeps or removes columns from a worksheet. Columns with null values as headers are not removed.
        /// </summary>
        /// <param name="sheet">The worksheet of which the columns are to be removed.</param>
        /// <param name="headers">The list of the headers to kept or removed.</param>
        /// <param name="toDoWithHeaders">If Keep then the headers are the columns to be kept. Otherwise, the headers are the columns to be removed.</param>
        /// <param name="headersRow">The row that contains the headers (default is 1).</param>
        /// <param name="startColumn">The start column of the headers to be checked (default is 1).</param>
        /// <param name="endColumn">The end column of the headers to be checked. If omitted then the last column in the same region is taken.</param>
        /// <param name="caseSensitive">If true then the comparison is case sensitive (default is false).</param>
        private static void keepOrRemoveColumns(this Worksheet sheet, List<string> headers, KeepColumnsMode toDoWithHeaders,
            int headersRow = 1, int startColumn = 1, int endColumn = -1, bool caseSensitive = false)
        {
            if (endColumn < startColumn) endColumn = sheet.GetLastColumn((Range)sheet.Cells[headersRow, startColumn]);

            for (int iColumn = endColumn; iColumn >= startColumn; iColumn--)
            {
                Range headerCell = sheet.Cells[headersRow, iColumn];
                if (headerCell.Value != null) //ignore empty headers
                {
                    string header = headerCell.Value;

                    bool contains = headers.Contains(header);

                    bool remove = header.Length > 0 &&
                        (toDoWithHeaders == KeepColumnsMode.Remove ? contains : !contains);

                    if (remove)
                    {
                        string c = iColumn.ToExcelColumnLetters();
                        Range column = sheet.Range[c + ":" + c];
                        column.Delete(XlDeleteShiftDirection.xlShiftToLeft);
                    }
                }
            }
        }


        public static void KeepColumns(this Worksheet sheet, List<string> headers, int headersRow = 1, int startColumn = 1, int endColumn = -1, bool caseSensitive = false)
        {
            sheet.keepOrRemoveColumns(headers, KeepColumnsMode.Keep, headersRow, startColumn, endColumn, caseSensitive);
        }

        public static void RemoveColumns(this Worksheet sheet, List<string> headers, int headersRow = 1, int startColumn = 1, int endColumn = -1, bool caseSensitive = false)
        {
            sheet.keepOrRemoveColumns(headers, KeepColumnsMode.Remove, headersRow, startColumn, endColumn, caseSensitive);
        }

        /// <summary>
        /// Imports a text file to a range.
        /// </summary>
        /// <param name="target">The cell to import the data to.</param>
        /// <param name="inputFile">The file to import.</param>
        /// <param name="delimiter">The delimiter that is used in the file (default is tab).</param>
        public static void ImportTextFile(this Range target, string inputFile, char delimiter = '\t')
        {
            QueryTable tbl = target.Worksheet.QueryTables.Add("TEXT;" + inputFile, target);

            tbl.TextFileParseType = XlTextParsingType.xlDelimited;
            //tbl.TextFileFixedColumnWidths (used for fixed columns)
            tbl.TextFilePlatform = (int)XlPlatform.xlWindows;
            tbl.TextFileStartRow = 1;
            tbl.TextFileTextQualifier = XlTextQualifier.xlTextQualifierNone;
            switch (delimiter)
            {
                case '\t':
                    tbl.TextFileTabDelimiter = true; break;
                case ';':
                    tbl.TextFileSemicolonDelimiter = true; break;
                case ' ':
                    tbl.TextFileSpaceDelimiter = true; break;
                case ',':
                    tbl.TextFileCommaDelimiter = true; break;
                default:
                    tbl.TextFileOtherDelimiter = delimiter.ToString(); break;
            }
            tbl.TextFileConsecutiveDelimiter = false;
            //tbl.TextFileColumnDataTypes=XlColumnDataType.xlGeneralFormat
            tbl.Refresh();
            tbl.Delete(); //remove the data connection at the end
        }

        public static void ImportTextFile(this Worksheet sheet, string inputFile, char delimiter = '\t')
        {
            sheet.Range["a1"].ImportTextFile(inputFile, delimiter);
        }

        #region Find rows with values

        public enum ComparisonType
        {
            Equal, GreaterThan, LowerThan
        }

        private static bool FindNextRow(this Range startCell, ComparisonType comparisonType,
            out int row, out double valueFound, params double[] values)
        {
            Worksheet sh = startCell.Worksheet; int c = startCell.Column;
            Range lastCellInSheet = sh.Cells[sh.Rows.Count, c];
            Range lastCell = lastCellInSheet.End[XlDirection.xlUp];

            //get the first non empty cell
            if (startCell.Value == null) startCell = startCell.End[XlDirection.xlDown];
            int startRow = startCell.Row, lastRow = lastCell.Row;

            //select the find function
            Func<double, bool> fGT = currentValue => currentValue > values[0];
            Func<double, bool> fLT = currentValue => currentValue < values[0];
            Func<double, bool> fEQ = currentValue => values.Contains(currentValue);

            Func<double, bool> checkIfFound;
            switch (comparisonType)
            {
                case ComparisonType.GreaterThan:
                    checkIfFound = fGT; break;
                case ComparisonType.LowerThan:
                    checkIfFound = fLT; break;
                default:
                case ComparisonType.Equal:
                    checkIfFound = fEQ; break;
            }

            for (int iRow = startRow; iRow <= lastRow; iRow++)
            {
                Range currentCell = sh.Cells[iRow, c];

                if (currentCell.Value != null && currentCell.Value is double)
                {
                    double currentValue = currentCell.Value;
                    bool found = checkIfFound(currentValue);
                    if (found) { row = iRow; valueFound = currentValue; return true; }
                }
            }

            row = 0; //if not found then zero is returned
            valueFound = 0.0;
            return false;
        }

        public static bool FindNextRowGT(this Range startCell, double value, out int row, out double valueFound)
        {
            return startCell.FindNextRow(ComparisonType.GreaterThan, out row, out valueFound, value);
        }

        public static bool FindNextRowLT(this Range startCell, double value, out int row, out double valueFound)
        {
            return startCell.FindNextRow(ComparisonType.LowerThan, out row, out valueFound, value);
        }

        public static bool FindNextRowEQ(this Range startCell, double value, out int row, out double valueFound, params double[] values)
        {
            return startCell.FindNextRow(ComparisonType.Equal, out row, out valueFound, value);
        }

        #endregion

        public static List<T> GetObjectList<T>(this Worksheet sh) where T:new()
        {
            List<T> list = new List<T>();

            //get field/column info
            Type type = typeof(T);
            //special field that contains the row number
            FieldInfo rowFieldInfo = null;

            Dictionary<FieldInfo, int> fields = new Dictionary<FieldInfo, int>();
            foreach (FieldInfo fi in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                List<ExcelColumnAttribute> attrs = fi.GetAttributes<ExcelColumnAttribute>();

                if (attrs != null)
                {
                    int column = -1;
                    //search each attribute (multiple equivalent column names)
                    foreach (ExcelColumnAttribute attr in attrs)
                    {
                        column = !string.IsNullOrWhiteSpace(attr.ColumnHeader) ?
                        sh.GetHeaderColumn(attr.ColumnHeader) :
                        sh.GetHeaderColumn(fi.Name);
                        if (column > 0) break;
                    }

                    if (column != -1)
                        fields.Add(fi, column);
                }

                //the last row that stores the attribute has the row number
                ExcelRowAttribute rowAttribute = fi.GetAttribute<ExcelRowAttribute>();
                if (rowAttribute != null)
                    rowFieldInfo = fi;
            }


            PropertyInfo rowPropertyInfo = null;
            Dictionary<PropertyInfo, int> properties = new Dictionary<PropertyInfo, int>();
            foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                ExcelColumnAttribute attr = pi.GetAttribute<ExcelColumnAttribute>();

                if (attr != null)
                {
                    int column = !string.IsNullOrWhiteSpace(attr.ColumnHeader) ?
                    sh.GetHeaderColumn(attr.ColumnHeader) :
                    sh.GetHeaderColumn(pi.Name);

                    if (column != -1)
                        properties.Add(pi, column);
                }

                //the last property that has the attribute stores the row number
                ExcelRowAttribute rowAttribute = pi.GetAttribute<ExcelRowAttribute>();
                if (rowAttribute != null)
                    rowPropertyInfo = pi;
            }

            int lastRow = sh.GetLastRow();
            for (int i = 2; i <= lastRow; i++)
            {
                T obj = new T();
                //set the row number at the field that contains the ExcelRow attribute
                if (rowFieldInfo != null) rowFieldInfo.SetValue(obj, i);

                //set the value
                foreach (var entry in fields)
                {
                    FieldInfo field = entry.Key;
                    int column = entry.Value;

                    dynamic value = sh.Cells[i, column].Value;
                    if (value != null)
                        if (value.GetType() == field.FieldType)
                            field.SetValue(obj, value);
                }

                //set the row number at the property that contains the ExcelRow attribute
                if (rowPropertyInfo != null) rowPropertyInfo.SetValue(obj, i, null);

                foreach (var entry in properties)
                {
                    PropertyInfo property = entry.Key;
                    int column = entry.Value;
                    dynamic value = sh.Cells[i, column].Value;
                    if (value != null)
                        if (value.GetType() == property.PropertyType)
                            property.SetValue(obj, value, null);

                }

                list.Add(obj);
            }
            return list;
        }

        public static List<T> GetObjectList<T>(string workbookFilePath, string worksheet) where T : new()
        {
            ExcelInfo info; Workbook wb;
            Application Excel = ExcelExtensions.OpenExcel(workbookFilePath, out info, out wb, ChangeSettingsMode.DontChangeSettings);
            Worksheet sh = wb.Worksheets[worksheet];

            List<T> list = sh.GetObjectList<T>();

            if (info.IsNewInstance) wb.Close();

            Excel.QuitOrRestoreSettings(ref info);

            return list;
        }


    }

}
