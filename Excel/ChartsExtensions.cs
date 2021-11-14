using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace Paulus.Excel
{
    //Charts
    public static class ChartsExtensions
    {

        /// <summary>
        /// Returns a generic list of the charts.
        /// </summary>
        /// <param name="charts">The charts collection.</param>
        /// <returns>A generic list of the charts.</returns>
        public static List<Chart> ToList(this Charts charts)
        {
            List<Chart> list = new List<Chart>();

            foreach (Chart ch in charts)
                list.Add(ch);

            return list;
        }

        /// <summary>
        /// Returns an array of the charts.
        /// </summary>
        /// <param name="charts">The charts collection.</param>
        /// <returns>An array of the charts.</returns>
        public static Chart[] ToArray(this Charts charts)
        {
            return charts.ToList().ToArray();
        }

        /// <summary>
        /// Returns true if the chart exists.
        /// </summary>
        /// <param name="charts">The charts collection.</param>
        /// <param name="chartName">The name of the chart to be checked for existence.</param>
        /// <param name="ignoreCase">If true then the case is ignored.</param>
        /// <returns>true if the chart exists.</returns>
        public static bool Contains(this Charts charts, string chartName, bool ignoreCase = true)
        {
            foreach (Chart chart in charts)
            {
                bool found = ignoreCase ? chart.Name.ToLower() == chartName.ToLower() : chart.Name == chartName;
                if (found)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the chart exists. Also returns the chart if found.
        /// </summary>
        /// <param name="charts">The charts collection.</param>
        /// <param name="chartName">The name of the chart to be checked for existence.</param>
        /// <param name="chart">The chart to be returned.</param>
        /// <param name="ignoreCase">If true then the case is ignored.</param>
        /// <returns>true if the chart exists.</returns>
        public static bool TryGetChart(this Charts charts, string chartName, out Chart chart, bool ignoreCase = true)
        {
            foreach (Chart ch in charts)
            {
                bool found = ignoreCase ? ch.Name.ToLower() == chartName.ToLower() : ch.Name == chartName;
                if (found)
                {
                    chart = ch;
                    return true;
                }
            }
            chart = null;
            return false;
        }

        /// <summary>
        /// Forces the deletion of the chart. No confirmation box appears.
        /// </summary>
        /// <param name="chart">The chart to be deleted.</param>
        public static void ForceDelete(this Chart chart)
        {
            Application app = chart.Application;
            ExcelInfo info = new ExcelInfo();
            app.ChangeSettings(ref info, ChangeSettingsMode.ChangeSettingsForSpeedExceptForVisible);
            chart.Delete();
            app.RestoreSettings(ref info);
        }

        public static void ForceDelete(this ChartObject chartObject)
        {
            Application app = chartObject.Application;
            ExcelInfo info = new ExcelInfo();
            app.ChangeSettings(ref info, ChangeSettingsMode.ChangeSettingsForSpeedExceptForVisible);
            chartObject.Delete();
            app.RestoreSettings(ref info);
        }

        public static Chart AddChart(this Workbook wb, string chartName, string title, Worksheet sheetThatPrecedesChart = null,
            XlChartType chartType = XlChartType.xlXYScatterLinesNoMarkers)
        {
            if (wb.Charts.Contains(chartName))
            {
                Chart ch = wb.Charts[chartName];
                ch.ForceDelete();
            }

            //(wb.Worksheets[1] as Worksheet).Range["a1"].Select(); //to avoid adding series
            Chart newChart = sheetThatPrecedesChart != null ?
                wb.Charts.Add(After: sheetThatPrecedesChart) : wb.Charts.Add();

            newChart.Name = chartName;
            newChart.HasTitle = true;
            newChart.ChartTitle.Text = title;
            newChart.ChartType = chartType;
            newChart.RemoveAllSeries();
            return newChart;
        }

        /// <summary>
        /// Removes all series from the chart.
        /// </summary>
        /// <param name="chart">The chart to remove the series from.</param>
        public static void RemoveAllSeries(this Chart chart)
        {
            while (chart.SeriesCollection().Count > 0)
            {
                SeriesCollection col = chart.SeriesCollection();
                col.Item(1).Delete();
            }
        }

        /// <summary>
        /// Sets the axis titles for a chart. Optionally sets the font size in points.
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="axisX"></param>
        /// <param name="axisYPrimary"></param>
        /// <param name="axisYSecondary"></param>
        public static void SetAxisTitles(this Chart chart, string axisX, string axisYPrimary, string axisYSecondary = null, float fontSize = -1f)
        {
            Axis axX = chart.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary);
            axX.HasTitle = true;
            axX.AxisTitle.Text = axisX;

            Axis axY = chart.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary);
            axY.HasTitle = true;
            axY.AxisTitle.Text = axisYPrimary;

            Axis axY2 = chart.Axes(XlAxisType.xlValue, XlAxisGroup.xlSecondary);
            if (!string.IsNullOrWhiteSpace(axisYSecondary))
            {
                axY2.HasTitle = true;
                axY2.AxisTitle.Text = axisYSecondary;
            }

            if (fontSize > 0f)
            {
                axX.AxisTitle.Format.TextFrame2.TextRange.Font.Size =
                axY.AxisTitle.Format.TextFrame2.TextRange.Font.Size = fontSize;

                if (!string.IsNullOrWhiteSpace(axisYSecondary))
                    axY2.AxisTitle.Format.TextFrame2.TextRange.Font.Size = fontSize;
            }
        }

        /// <summary>
        /// Adds a new series to the chart.
        /// </summary>
        /// <param name="chart">The chart to add series to.</param>
        /// <param name="sheet">The worksheet that contains the X and Y values.</param>
        /// <param name="xColumn">The colunm of the X values.</param>
        /// <param name="yColumn">The column of the Y values.</param>
        /// <param name="firstRow">The first row of the X and Y values. If it is omitted then the first row is 2.</param>
        /// <param name="lastRow">The last row of the X and Y values. If it is omitted then the row of the last non empty value of the same contiguous area of the Y column is used.</param>
        /// <param name="seriesName">The name of the series. If omitted then the value of the cell before the startCellY is used. If is an empty cell then the value of the first cell in the same column is used.</param>
        /// <param name="useSecondaryAxis">true if the secondary axis should be used in the Y axis. If it is the first series in the chart, then it is ignored and the primary axis is used. </param>
        /// <returns>The new series.</returns>
        public static Series AddSeries(this Chart chart, Worksheet sheet, int xColumn, int yColumn, int firstRow = 2, int lastRow = -1, string seriesName = "", bool useSecondaryAxis = false)
        {
            Range startXCell = (Range)sheet.Cells[firstRow, xColumn],
                startYCell = (Range)sheet.Cells[firstRow, yColumn],
                endYCell = (Range)(lastRow < 1 ? startYCell.GetLastCellDown() : sheet.Cells[lastRow, yColumn]);
            return
            chart.AddSeries(startXCell, startYCell, endYCell, seriesName, useSecondaryAxis);
        }

        /// <summary>
        /// Adds a new series to the chart.
        /// </summary>
        /// <param name="chart">The chart to add series to.</param>
        /// <param name="startXCell">The start cell of the X values.</param>
        /// <param name="startYCell">The start cell of the Y values.</param>
        /// <param name="endYCell">The end cell of the Y values. This will define the number of values. If it is omitted then the last cell in the </param>
        /// <param name="seriesName">The name of the series. If omitted then the value of the cell before the startCellY is used. If is an empty cell then the value of the first cell in the same column is used.</param>
        /// <param name="useSecondaryAxis">true if the secondary axis should be used in the Y axis. If it is the first series in the chart, then it is ignored and the primary axis is used. </param>
        /// <returns>The new series.</returns>
        public static Series AddSeries(this Chart chart, Range startXCell, Range startYCell, Range endYCell = null,
            string seriesName = "", bool useSecondaryAxis = false)
        {
            Worksheet xSheet = startXCell.Worksheet;
            Worksheet ySheet = startYCell.Worksheet;
            if (endYCell == null) endYCell = startYCell.GetLastCellDown();
            Range endXCell = startXCell.Offset[endYCell.Row - startYCell.Row, 0];

            startXCell.Worksheet.Select();
            startXCell.Select(); //needed to avoid creation of the charts
            Series newSeries = (chart.SeriesCollection() as SeriesCollection).NewSeries();
            newSeries.XValues = xSheet.Range[startXCell, endXCell];
            newSeries.Values = ySheet.Range[startYCell, endYCell];

            //check the series name
            newSeries.Name = !string.IsNullOrWhiteSpace(seriesName) ? seriesName :
                startYCell.FindHeaderCell().Value;

            if (useSecondaryAxis) newSeries.AxisGroup = XlAxisGroup.xlSecondary;
            return newSeries;
        }

        public static void RemoveAllCharts(this Workbook workbook)
        {
            foreach (Chart ch in workbook.Charts)
                ch.ForceDelete();

            foreach (Worksheet sh in workbook.Sheets)
                foreach (ChartObject ch in sh.ChartObjects())
                    ch.ForceDelete();
        }
  
    }
}
