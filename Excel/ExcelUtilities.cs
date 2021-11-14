using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Office.Core;
using Xl=Microsoft.Office.Interop.Excel;
//using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices; //Marshall

namespace Paulus.Excel
{
    public static class ExcelUtilities
    {

        public static int FindColumn(object what, Xl.Range firstCell, Xl.Worksheet sheet, bool isPart = false, bool matchCase = false)
        {
            Queue toDispose = new Queue();

            Xl.Range row = sheet.Rows[firstCell.Row]; toDispose.Enqueue(row);
            Xl.Range foundRange = row.Find(what, firstCell, Xl.XlFindLookIn.xlValues, isPart ? Xl.XlLookAt.xlPart : Xl.XlLookAt.xlWhole,
                Xl.XlSearchOrder.xlByColumns, Xl.XlSearchDirection.xlNext,
                matchCase); toDispose.Enqueue(foundRange);

            int column = foundRange != null ? foundRange.Column : -1;
            DisposeComObjects(toDispose);
            return column;
        }

        public static  List<double> ReadColumnData(int column, int firstRow, Xl.Worksheet sheet)
        {
            Queue toDispose = new Queue();

            Xl.Range firstDataCell = sheet.Cells[firstRow, column]; toDispose.Enqueue(firstDataCell);
            Xl.Range nextDataCell = sheet.Cells[firstRow + 1, column]; toDispose.Enqueue(nextDataCell);
            Xl.Range lastDataCell = nextDataCell != null ? firstDataCell.End[Xl.XlDirection.xlDown] : nextDataCell; toDispose.Enqueue(lastDataCell);

            List<double> data = new List<double>();
            for (int row = firstRow; row <= lastDataCell.Row; row++)
            {
                Xl.Range cell = sheet.Cells[row, column]; toDispose.Enqueue(cell);

                if (cell != null && !CellContainsError(cell))
                    data.Add(cell.Value);
            }

            DisposeComObjects(toDispose);

            return data;
        }

        public static  void DisposeComObjects(Queue list)
        {
            //collect all objects to be disposed
            GC.Collect(); GC.WaitForPendingFinalizers();

            //the duplicate is needed only if VSTO are used
            GC.Collect(); GC.WaitForPendingFinalizers();

            while (list.Count > 0)
            {
                object item = list.Dequeue();
                if (item != null)
                    Marshal.FinalReleaseComObject(item);
            }
        }

        public static bool CellContainsError(Xl.Range cell)
        {
            foreach (Xl.XlErrorChecks ch in Enum.GetValues(typeof(Xl.XlErrorChecks)))
                if (cell.Errors[ch].Value) return true;
            return false;
        }
    }
}
