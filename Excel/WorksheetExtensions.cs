using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace Paulus.Excel
{
    public static class WorksheetExtensions
    {

        #region Worksheets

        //return a typed worksheet to make the things
        public static Worksheet Item2(this Sheets worksheets, object Index)
        {
            return (Worksheet)worksheets[Index];
        }

        /// <summary>
        /// Returns true if the sheet exists.
        /// </summary>
        /// <param name="worksheets">The worksheets collection.</param>
        /// <param name="sheetName">The name of the worksheet to be checked for existence.</param>
        /// <returns>true if the worksheet exists.</returns>
        public static bool Contains(this Sheets worksheets, string sheetName)
        {
            StringComparison comparer = StringComparison.OrdinalIgnoreCase;
            foreach (Worksheet sheet in worksheets)
            {
                bool found = sheet.Name.Equals(sheetName, comparer);
                if (found) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the worksheets contains all of the sheet names. The search is case insensitive.
        /// </summary>
        /// <param name="worksheets">The worksheets collection.</param>
        /// <param name="sheetNames">The sheet names to be checked for existence.</param>
        /// <returns>true all sheet names are contained in the collection. </returns>
        public static bool ContainsAll(this Sheets worksheets, params string[] sheetNames)
        {
            foreach (string sheetName in sheetNames)
                if (!worksheets.Contains(sheetName)) return false;
            return true;
        }

        public static bool ContainsAny(this Sheets worksheets, out Worksheet firstOccurence, params string[] sheetNames)
        {
            foreach (string sheetName in sheetNames)
                if (worksheets.Contains(sheetName))
                {
                    worksheets.TryGetSheet(sheetName, out firstOccurence, true);
                    return true;
                }
            firstOccurence = null;
            return false;
        }

        /// <summary>
        /// Returns true if the sheet exists. Also returns the worksheet if found.
        /// </summary>
        /// <param name="worksheets">The worksheets collection.</param>
        /// <param name="sheetName">The name of the worksheet to be checked for existence.</param>
        /// <param name="workSheet">The worksheet to be returned.</param>
        /// <param name="ignoreCase">If true then the case is ignored.</param>
        /// <returns>true if the worksheet exists.</returns>
        public static bool TryGetSheet(this Sheets worksheets, string sheetName, out Worksheet workSheet, bool ignoreCase = true)
        {
            foreach (Worksheet sheet in worksheets)
            {
                bool found = ignoreCase ? sheet.Name.ToLower() == sheetName.ToLower() : sheet.Name == sheetName;
                if (found)
                {
                    workSheet = sheet;
                    return true;
                }
            }
            workSheet = null;
            return false;
        }

        /// <summary>
        /// Create a worksheet or returns one if it already exists.
        /// </summary>
        /// <param name="worksheets">The worksheets collection.</param>
        /// <param name="sheetName">The sheet name of the returned worksheet.</param>
        /// <returns>The worksheet which is created or already exists.</returns>
        public static Worksheet CreateOrGetWorksheet(this Sheets worksheets, string sheetName)
        {
            Worksheet sheet;
            if (!worksheets.TryGetSheet(sheetName, out sheet, true))
            {
                sheet = worksheets.Add();
                sheet.Name = sheetName;
            }
            return sheet;
        }

        /// <summary>
        /// Removes all sheets from the workbook except the ones that are declared in the names array. Optionally returns the first sheet declared in the names array.
        /// </summary>
        /// <param name="worksheets">The worksheets collection.</param>
        /// <param name="names">The worksheet names to remain in the workbook.</param>
        /// <returns>The first sheet that is declared in the names array.</returns>
        public static Worksheet RemoveAllExceptFor(this Sheets worksheets, params string[] names)
        {
            if (names == null) throw new ArgumentNullException("names cannot be null. Use at least one name.", "names");
            if (!worksheets.ContainsAll(names)) throw new ArgumentException("Not all names are contained in the Worksheets collection.", "names");

            //sheet indexing starts from 1 to count in Excel
            for (int iSheet = worksheets.Count; iSheet > 0; iSheet--)
                if (!names.Contains((string)worksheets[iSheet].Name)) worksheets[iSheet].Delete();

            return worksheets[names[0]];
        }

        /// <summary>
        /// Removes all sheets from the workbook except for the first sheet.
        /// </summary>
        /// <param name="worksheets">The worksheets collection.</param>
        /// <returns>The first worksheet of the workbook.</returns>
        public static Worksheet RemoveAllExceptForTheFirstSheet(this Sheets worksheets)
        {
            for (int iSheet = worksheets.Count; iSheet > 1; iSheet--)
                worksheets[iSheet].Delete();

            return worksheets[1]; //tested
        }

        /// <summary>
        /// Returns a generic list of the worksheets.
        /// </summary>
        /// <param name="worksheets">The worksheets collection.</param>
        /// <returns>A generic list of the worksheets.</returns>
        public static List<Worksheet> ToList(this Sheets worksheets)
        {
            List<Worksheet> list = new List<Worksheet>();
            foreach (Worksheet worksheet in worksheets)
                list.Add(worksheet);

            return list;
        }

        /// <summary>
        /// Returns an array of the worksheets.
        /// </summary>
        /// <param name="worksheets">The worksheets collection.</param>
        /// <returns>An array of the worksheets.</returns>
        public static Worksheet[] ToArray(this Sheets worksheets)
        {
            return worksheets.ToList().ToArray();
        }
        #endregion

        #region Worksheet
        /// <summary>
        /// Forces a copy of the worksheet inside the same workbook. No confirmation box appears, if the target already appears. If the location is not specified i.e. beforeWorksheet and afterWorksheet are null then the copy is located after the source worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet to be copied.</param>
        /// <param name="copySheetName">The name of the copied worksheet. If omitted then the " Copy" is appended to the original name.</param>
        /// <param name="beforeWorksheet">If specified then the copied worksheet is positioned before this.</param>
        /// <param name="afterWorksheet">If specified then the copied worksheet is positioned after this. If beforeWorksheet is specified too, then the afterWorksheet is ignored.</param>
        /// <returns>The copied worksheet.</returns>
        public static Worksheet ForceCopy(this Worksheet worksheet, string copySheetName = "", Worksheet beforeWorksheet = null, Worksheet afterWorksheet = null, ChangeSettingsMode changeSettingsMode = ChangeSettingsMode.ChangeSettingsForSpeedExceptForVisible)
        {
            if (string.IsNullOrWhiteSpace(copySheetName)) copySheetName = worksheet.Name + " Copy";

            Application app = worksheet.Application;
            Workbook wb = worksheet.Parent;

            //change settings to avoid confirmation dialog boxes and to speed up
            ExcelInfo info = new ExcelInfo();
            app.ChangeSettings(ref info, changeSettingsMode);

            //delete copied sheet if it already exists
            Worksheet copySheet;
            if (wb.Worksheets.TryGetSheet(copySheetName, out copySheet))
                copySheet.Delete();

            //now copy the desired sheet
            if (beforeWorksheet != null)
                worksheet.Copy(Before: beforeWorksheet);
            else if (afterWorksheet != null)
                worksheet.Copy(After: afterWorksheet);
            else
                worksheet.Copy(After: worksheet);

            copySheet = wb.ActiveSheet;
            copySheet.Name = copySheetName;

            //restore settings (if they have changed)
            app.RestoreSettings(ref info, false);

            return copySheet;
        }

        /// <summary>
        /// Clears all the contents of the worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet to cleared.</param>
        public static void Clear(this Worksheet worksheet)
        {
            worksheet.Rows.Clear();
        }

        /// <summary>
        /// Forces the deletion of the worksheet. No confirmation box appears.
        /// </summary>
        /// <param name="worksheet">The worksheet to be deleted.</param>
        public static void ForceDelete(this Worksheet worksheet)
        {
            Application app = worksheet.Application;
            ExcelInfo info = new ExcelInfo();
            app.ChangeSettings(ref info, ChangeSettingsMode.ChangeSettingsForSpeedExceptForVisible);
            worksheet.Delete();
            app.RestoreSettings(ref info);
        }

        public static void AutoFitColumns(this Worksheet worksheet)
        {
            worksheet.UsedRange.Columns.AutoFit();
        }

        //autofills each column based on the last cell that contains a formula
        public static void AutoFill(this Worksheet sh, int lastRow, int lastColumn, int firstColumn=1, int firstRow=1)
        {
            //auto fill based on the last cell of each column
            for (int iColumn = firstColumn; iColumn <= lastColumn; iColumn++)
            {
                Range startFormulaCell = sh.Cells[firstRow, iColumn] as Range;
                startFormulaCell.AutoFill(sh.Range[startFormulaCell, sh.Cells[lastRow, iColumn]]);
            }
        }

        #endregion



    }
}
