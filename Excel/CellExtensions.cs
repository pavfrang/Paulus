using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.Collections;

namespace Paulus.Excel
{
    public static class CellExtensions
    {
        #region Cell extensions
        /// <summary>
        /// Returns true if the cell contains an error value.
        /// </summary>
        /// <param name="cell">The cell to be checked for errors.</param>
        /// <returns>true if the cell has an error value.</returns>
        /// <example>
        /// Range cell1 = wb.Worksheets[1].Range["a1"];
        /// if(cell1.CellContainsError()) System.Console.WriteLine(cell1.GetCellErrorType());
        ///</example>
        public static bool CellContainsError(this Range cell)
        {
            foreach (XlErrorChecks ch in Enum.GetValues(typeof(XlErrorChecks)))
                if (cell.Errors[ch].Value) return true;
            return false;
        }

        /// <summary>
        /// Returns the first error of the cell if it exists.
        /// </summary>
        /// <param name="cell">The cell to be checked for errors.</param>
        /// <returns>One of the XlErrorChecks values or 0 if no error is found.</returns>
        /// <example>
        /// Range cell1 = wb.Worksheets[1].Range["a1"];
        /// if(cell1.CellContainsError()) System.Console.WriteLine(cell1.GetCellErrorType());
        ///</example>
        public static XlErrorChecks GetFirstCellError(this Range cell)
        {
            foreach (XlErrorChecks ch in Enum.GetValues(typeof(XlErrorChecks)))
                if (cell.Errors[ch].Value) return ch;
            return (XlErrorChecks)0;
        }

        /// <summary>
        /// Returns all the errors of the cell.
        /// </summary>
        /// <param name="cell">The cell to be checked for errors.</param>
        /// <returns>A list of the XlErrorChecks values or an empty list if no errors exist.</returns>
        public static List<XlErrorChecks> GetAllCellErrors(this Range cell)
        {
            List<XlErrorChecks> errors = new List<XlErrorChecks>();

            foreach (XlErrorChecks errorCheck in Enum.GetValues(typeof(XlErrorChecks)))
                if (cell.Errors[errorCheck].Value) errors.Add(errorCheck);

            return errors;
        }

        /// <summary>
        /// Returns the Excel letters that correspond to a specific column index.
        /// </summary>
        /// <param name="columnIndex">The number of the column which is in the range [1,18278].</param>
        /// <returns>The Excel name of the column if the column number is in the valid range, or the empty string.</returns>
        public static string ToExcelColumnLetters(this int columnIndex)
        {
            //16384 is the last column (XFD)
            int offset, c1, c2, c3;
            if (columnIndex >= 1 && columnIndex <= 26)
            {
                offset = 1; c1 = columnIndex - offset; //1st digit
                return ((char)(c1 + 65)).ToString();
            }
            else if (columnIndex > 26 && columnIndex <= 702)
            {
                offset = 27; c2 = columnIndex - offset;
                c1 = c2 % 26; //1st digit
                c2 = (c2 - c1) / 26 % 26; //2nd digit
                return new string(new char[] { 
                (char)(c2+65),(char)(c1+65)
                });
            }
            else if (columnIndex > 702 && columnIndex < 18278)
            {
                offset = 703; c3 = columnIndex - offset;
                c1 = c3 % 26;//1st digit
                c2 = (c3 - c1) / 26 % 26; //2nd digit
                c3 = (c3 - c2 * 26) / 676 % 26; //3rd digit
                return new string(new char[] {
                (char)(c3+65),(char)(c2+65),(char)(c1+65)
                });
            }
            else
                return "";
        }

        /// <summary>
        /// Returns the column number from an Excel column.
        /// </summary>
        /// <param name="excelColumn">The Excel column (eg. "XFD", "AZ", "F").</param>
        /// <returns>The index of the column which is in the range [1,18278] or 0 if the column label is not valid. </returns>
        public static int ExcelColumnToInt(this string excelColumn)
        {
            excelColumn = excelColumn.ToUpper();
            int len = excelColumn.Length;
            int offset = 0; int[] v = new int[len];

            for (int i = 0; i < len; i++)
            {
                v[i] = (int)excelColumn[len - 1 - i] - 65;
                //offset += (int)Math.Pow(26, i);
            }

            switch (len)
            {
                case (1):
                    offset = 1;
                    return v[0] + offset;
                case (2):
                    offset = 27; //=26+1
                    return v[0] + v[1] * 26 + offset;
                case (3):
                    offset = 703; //=26*26+26+1 (S 26^i i=0,1,...,n-1)
                    return v[0] + v[1] * 26 + v[2] * 676 + offset;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Retrieves the last cell in the same column by using the next empty cell as a boundary.
        /// If the current cell is empty then the next non empty cell is the boundary.
        /// </summary>
        /// <param name="cell">The cell to begin the search of the last cell.</param>
        /// <returns>The last cell in the same column.
        /// If the next cell is empty then the same cell is returned. If the current cell is empty then the last empty cell is returned.</returns>
        public static Range GetLastCellDown(this Range cell)
        {
            Worksheet sheet = cell.Worksheet;
            //if (cell == null) cell = sheet.Cells[1, 1];
            bool startCellHasValue = cell.Value != null;
            bool nextCellHasValue = cell.Offset[1, 0].Value != null;

            if (startCellHasValue && nextCellHasValue)
                return cell.End[XlDirection.xlDown];
            else if (startCellHasValue)
                return cell;
            else //if (!startCellHasValue) //if it has null then the last null cell is returned
                return cell.End[XlDirection.xlDown].Offset[-1];
        }

        /// <summary>
        /// Retrieves the last column by using the 
        /// </summary>
        /// <param name="cell">The cell to begin the search of the last cell.</param>
        /// <returns></returns>
        public static Range GetLastCellRight(this Range cell)
        {
            Worksheet sheet = cell.Worksheet;
            bool startCellHasValue = cell.Value != null;
            bool nextCellHasValue = cell.Offset[0, 1].Value != null;

            if (startCellHasValue && nextCellHasValue)
                return cell.End[XlDirection.xlToRight];
            else if (startCellHasValue)
                return cell;
            else //if (!startCellHasValue) //if it has null then the last null cell is returned
                return cell.End[XlDirection.xlToRight].Offset[0, -1];
        }

        /// <summary>
        /// Retrieves the last row of a worksheet by using the next empty cell as a boundary.
        /// If the current cell is empty then the next non empty cell is the boundary.
        /// </summary>
        /// <param name="sheet">The worksheet of the search area.</param>
        /// <param name="startCell">The cell to begin the search of the last cell. If it is omitted then the first cell of the sheet is used.</param>
        /// <returns>The last row in the contiguous area of the startCell.
        /// If the next cell is empty then the same cell is returned. If the current cell is empty then the last empty cell is returned.</returns>
        public static int GetLastRow(this Worksheet sheet, Range startCell = null)
        {
            return (startCell??sheet.Range["a1"]).GetLastCellDown().Row;
        }

        /// <summary>
        /// Retrieves the last cell in the same row by using the next empty cell as a boundary.
        /// If the current cell is empty then the next non empty cell is the boundary.
        /// </summary>
        /// <param name="sheet">The worksheet of the search area.</param>
        /// <param name="startCell">The cell to begin the search of the last cell. If it is omitted then the first cell of the sheet is used.</param>
        /// <returns>The last column in the contiguous area of the startCell.
        /// If the next cell is empty then the same cell is returned. If the current cell is empty then the last empty cell is returned.</returns>
        public static int GetLastColumn(this Worksheet sheet, Range startCell = null)
        {
            return (startCell??sheet.Range["a1"]).GetLastCellRight().Column;
        }

        //works only if values are numbers and only the header section contains one row
        public static Range FindHeaderCell(this Range startCell)
        {
            Range previousYCell = null;

            if (startCell.Row > 1)
            {
                //check the previous cell for a string value
                previousYCell = startCell.Offset[-1];
                if (previousYCell.Value != null && previousYCell.Value is string) return previousYCell;

                //check the previous cell that has a string value
                previousYCell = startCell.End[XlDirection.xlUp];
                if (previousYCell.Value != null && previousYCell.Value is string) return previousYCell;
            }

            //check the first cell that has a string value
            previousYCell = startCell.Cells[1, startCell.Column];
            if (previousYCell.Value != null && previousYCell.Value is string) return previousYCell;

            //check the previous cell that has a string value
            previousYCell = previousYCell.End[XlDirection.xlDown];
            if (previousYCell.Value != null && previousYCell.Value is string) return previousYCell;

            //at the end search for all values before the startCell and return the first one from the end that is of type string
            Worksheet sh = startCell.Worksheet;
            int column = startCell.Column;
            for (int iRow = startCell.Row; iRow <= 1; iRow--)
            {
                previousYCell = sh.Cells[iRow, column];
                if (previousYCell.Value != null && previousYCell.Value is string) return previousYCell;
            }

            return previousYCell;
        }

        public static string ToString2(this Range cell)
        {
            dynamic value = cell.Value;
            if (value != null)
                return value is string ? value : value.ToString();
            else
                return "";
        }
        public static string ToString2(this Range cell, IFormatProvider provider)
        {
            dynamic value = cell.Value;
            if (value != null)
                return value is string ? value : value.ToString(provider);
            else
                return "";
        }

        public static double Average(this Range cells)
        {
            return cells.Application.WorksheetFunction.Average(cells);
        }

        public static double Minimum(this Range cells)
        {
            return cells.Application.WorksheetFunction.Min(cells);
        }

        public static double Maximum(this Range cells)
        {
            return cells.Application.WorksheetFunction.Max(cells);
        }

        public static int CountEmpty(this Range cells)
        {
            return (int)cells.Application.WorksheetFunction.CountBlank(cells);
        }

        public static int CountNonEmpty(this Range cells)
        {
            return (int)cells.Application.WorksheetFunction.CountA(cells);
        }


        public static void ClearColorFormats(this Range cells)
        {
            Interior interior = cells.Interior;
            interior.Pattern = XlPattern.xlPatternNone;
            interior.TintAndShade = 0;
            interior.PatternTintAndShade = 0;
        }

        #endregion

        #region Retrieve special values
        /// <summary>
        /// Returns distinct values from values in a specified range by searching in a column.
        /// </summary>
        /// <typeparam name="T">The type of the values to be retrieved.</typeparam>
        /// <param name="worksheet">The worksheet that contains the values.</param>
        /// <param name="column">The column that contains the values.</param>
        /// <param name="startRow">The start row of the data (default is 2).</param>
        /// <param name="lastRow">The last row of the data. If it is not given then the last row in the contiguous region in the column is assumed.</param>
        /// <returns></returns>
        public static List<T> GetDistinctValuesInColumn<T>(this Worksheet worksheet, int column, int startRow = 2, int lastRow = -1)
        {

            List<T> list = new List<T>();
            if (lastRow < startRow) lastRow = worksheet.GetLastRow(worksheet.Cells[startRow, column] as Range);

            for (int iRow = startRow; iRow <= lastRow; iRow++)
            {
                Range cell = worksheet.Cells[iRow, column];
                if (cell.Value != null && cell.Value is T)
                    list.Add(cell.Value);
            }
            return list;
        }

        /// <summary>
        /// Returns distinct values from values in a specified range by searching in a row.
        /// </summary>
        /// <typeparam name="T">The type of the values to be retrieved.</typeparam>
        /// <param name="worksheet">The worksheet that contains the values.</param>
        /// <param name="row">The row that contains the values.</param>
        /// <param name="startColumn">The start column of the data (default is 1).</param>
        /// <param name="lastColumn">The last column of the data. If it is not given then the last column in the contiguous region in the row is assumed.</param>
        /// <returns></returns>
        public static List<T> GetDistinctValuesInRow<T>(this Worksheet worksheet, int row, int startColumn = 1, int lastColumn = -1)
        {
            List<T> list = new List<T>();
            if (lastColumn < startColumn) lastColumn = worksheet.GetLastColumn(worksheet.Cells[row, startColumn] as Range);

            for (int iColumn = startColumn; iColumn <= lastColumn; iColumn++)
            {
                Range cell = worksheet.Cells[row, iColumn];
                if (cell.Value != null && cell.Value is T)
                    list.Add(cell.Value);
            }
            return list;
        }

        /// <summary>
        /// Returns the visible cells of a range. Useful when a filter is used.
        /// </summary>
        /// <param name="range">The range to select the visible cells from.</param>
        /// <returns>The visible cells range.</returns>
        public static Range GetVisibleRange(this Range range)
        {
            return range.SpecialCells(XlCellType.xlCellTypeVisible);
            //XlCellType.xlCellTypeLastCell!!
        }

        //get a list of cells that contain a value (partial)
        //de me afinei generic List<Range> san epistrefomeno typo!!!
        public static ArrayList FindAll<T>(this Range range, T value)
        {
            ArrayList list = new ArrayList();

            Range cellFound = range.Find(value);
            if (cellFound != null)
            {
                list.Add(cellFound);
                string firstAddress = cellFound.Address;

                cellFound = range.FindNext(cellFound);
                while (cellFound != null && cellFound.Address != firstAddress)
                {
                    list.Add(cellFound);

                    cellFound = range.FindNext(cellFound);
                }
            }
            return list;
        }

        public static ArrayList FindAll<T>(this Worksheet sheet, T value)
        {
            return sheet.UsedRange.FindAll(value);
        }

        #endregion


    }
}
