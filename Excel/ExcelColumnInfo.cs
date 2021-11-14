using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.Xml.Serialization;
using System.Globalization;

namespace Paulus.Excel
{
    public class ExcelWorkbookColumnConfiguration
    {
        public string WorkbookName;
        
        [XmlArray("SheetConfigurations")]
        [XmlArrayItem("SheetConfiguration")]
        public List<ExcelSheetColumnConfiguration> SheetColumnConfigurations; //for each sheet fill the columns

        public static ExcelWorkbookColumnConfiguration Create(Workbook wb, params ExcelSheetColumnConfiguration[] sheetColumnConfigurations)
        {
            ExcelWorkbookColumnConfiguration config = new ExcelWorkbookColumnConfiguration();
            config.WorkbookName = wb.Name;

            config.SheetColumnConfigurations = sheetColumnConfigurations.ToList();

            return config;
        }
    }

    public class ExcelSheetColumnConfiguration
    {
        [XmlAttribute("sheet")]
        public string SheetName;

        public int HeaderRow;
        public int HeaderRowsCount;
        public int FirstValuesRow;
        public int FirstColumn;
        public int LastColumn;

        [XmlArray("ColumnInfos")]
        [XmlArrayItem("ColumnInfo", typeof(ExcelColumnInfo))]
        public List<ExcelColumnInfo> ColumnInfos;

        public static ExcelSheetColumnConfiguration Create(
            Worksheet sheet,
            int firstColumn, int lastColumn,
            int headerRow, int headerRowsCount, int firstValuesRow)
        {
            ExcelSheetColumnConfiguration config = new ExcelSheetColumnConfiguration();
            config.HeaderRow = headerRow;
            config.HeaderRowsCount = headerRowsCount;
            if (firstValuesRow < headerRow + headerRowsCount) throw new ArgumentOutOfRangeException("firstValuesRow", "firstValuesRow must be at greater than or equal to headerRow+headerRowsCount.");

            config.FirstValuesRow = firstValuesRow;

            config.FirstColumn = firstColumn;
            if (lastColumn < firstColumn) throw new ArgumentOutOfRangeException("lastColumn", "lastColumn must be greater than or equal to firstColumn.");
            config.LastColumn = lastColumn;

            config.ColumnInfos = new List<ExcelColumnInfo>();
            for (int iColumn = 1; iColumn <= lastColumn; iColumn++)
                config.ColumnInfos.Add(ExcelColumnInfo.Create(sheet, iColumn, headerRow, headerRowsCount, firstValuesRow));

            return config;
        }
    }

    public class ExcelColumnInfoWithStats : ExcelColumnInfo
    {
        #region Values section
        public int LastValuesRow;

        public int ValuesCount;
        public int EmptyValuesCount;
        public int NonEmptyValuesCount;

        [XmlIgnore]
        public bool ContainsValues { get { return EmptyValuesCount == ValuesCount; } }

        [XmlIgnore]
        public Range ValuesRange;

        #endregion

        //the safest solution is for the user to provide the exact data to avoid stuff
        public static ExcelColumnInfoWithStats Create(Worksheet sheet, int columnIndex,
            int headerRow, int headerRowsCount,
            int firstValuesRow, int lastValuesRow)
        {
            ExcelColumnInfoWithStats info = (ExcelColumnInfoWithStats)
                ExcelColumnInfo.Create(sheet, columnIndex, headerRow, headerRowsCount, firstValuesRow);

            if (lastValuesRow < firstValuesRow) throw new ArgumentOutOfRangeException("lastValuesRow", "lastValuesRow must be greater than or equal to firstValuesRow.");
            info.LastValuesRow = lastValuesRow;

            info.ValuesRange = sheet.Range[sheet.Cells[firstValuesRow, columnIndex], sheet.Cells[lastValuesRow, columnIndex]];
            info.ValuesCount = info.LastValuesRow - info.FirstValuesRow + 1;
            //info.ValuesCount = valuesRange.Count;

            info.EmptyValuesCount = info.ValuesRange.CountEmpty(); //(int)sheet.Application.WorksheetFunction.CountBlank(valuesRange);
            info.NonEmptyValuesCount = info.ValuesRange.CountNonEmpty();

            return info;
        }
    }

    [XmlInclude(typeof(ExcelColumnInfoWithStats))]
    public class ExcelColumnInfo
    {
        [XmlAttribute("sheet")]
        public string ParentSheetName;

        [XmlAttribute]
        public int Index; //1

        [XmlAttribute]
        public string Name; //a

        //the header is always the first cell in a desired range

        public string Header; //NO2_out

        public int HeaderRowsCount;

        [XmlArray]
        [XmlArrayItem("HeaderExtraRow")]
        public List<string> HeaderExtraRows;

        [XmlIgnore]
        public bool HasHeader { get { return !string.IsNullOrWhiteSpace(Header); } }

        public int FirstValuesRow;

        public override string ToString()
        {
            return string.Format(@"{0}/{1} @ {2}({3})", ParentSheetName, Header, Name, Index);
        }

        public static ExcelColumnInfo Create(Worksheet sheet, int columnIndex, int headerRow, int headerRowsCount, int firstValuesRow)
        {
            ExcelColumnInfo info = new ExcelColumnInfo();
            info.ParentSheetName = sheet.Name;
            info.Index = columnIndex;
            info.Name = columnIndex.ToExcelColumnLetters();

            CultureInfo en = CultureInfo.InvariantCulture;

            Range headerCell = sheet.Cells[headerRow, columnIndex];
            info.Header = headerCell.ToString2(en);


            info.HeaderRowsCount = headerRowsCount;
            info.HeaderExtraRows = new List<string>(); //always create the list
            if (headerRowsCount > 1)
            {
                for (int iRow = headerCell.Row + 1; iRow < headerCell.Row + headerRowsCount; iRow++)
                    info.HeaderExtraRows.Add((sheet.Cells[iRow, columnIndex] as Range).ToString2(en));
            }

            if (firstValuesRow < headerRow + headerRowsCount) throw new ArgumentOutOfRangeException("firstValuesRow", "firstValuesRow must be at greater than or equal to headerRow+headerRowsCount.");
            info.FirstValuesRow = firstValuesRow;

            return info;
        }

        //#region ExcelColumnInfo Equality
        // public string[] EquivalentHeaders;
        ////allow second header at the second line
        //public override bool Equals(object obj)
        //{
        //    if (obj is ExcelColumnInfo)
        //    {
        //        ExcelColumnInfo typedObj = (ExcelColumnInfo)obj;
        //        return this.Header == typedObj.Header ||
        //            this.EquivalentHeaders != null && this.EquivalentHeaders.Contains(typedObj.Header) ||
        //            typedObj.EquivalentHeaders != null && typedObj.EquivalentHeaders.Contains(this.Header) ||
        //            this.EquivalentHeaders != null && typedObj.EquivalentHeaders != null &&
        //                ArraysContainCommonElement<string>(this.EquivalentHeaders, typedObj.EquivalentHeaders);
        //    }
        //    else return false;
        //}

        //private static bool ArraysContainCommonElement<T>(T[] arr1, T[] arr2) where T : IEquatable<T>
        //{
        //    for (int i = 0; i < arr1.Length; i++)
        //        for (int j = 0; j < arr2.Length; j++)
        //            if (arr1[i].Equals(arr2[j])) return true;
        //    return false;
        //}

        //public static bool operator ==(ExcelColumnInfo left, ExcelColumnInfo right)
        //{
        //    return left.Equals(right);
        //}

        //public static bool operator !=(ExcelColumnInfo left, ExcelColumnInfo right)
        //{
        //    return !left.Equals(right);
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
        //#endregion

    }
}
