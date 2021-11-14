using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;
using System.ComponentModel;
using Paulus.Common;
using System.Drawing;
using System.IO;

namespace Paulus.Forms.Charting
{
    public static class ChartExtensions
    {
        //eg. Time [s] or Something[-]
        public static bool LabelContainsUnit(string label)
        { return label.EndsWith("]") && label.LastIndexOf('[') < label.LastIndexOf(']'); }

        public static string GetUnitFromLabel(string label)
        {
            int lastOpenBracketPosition = label.LastIndexOf('[');
            return lastOpenBracketPosition > 0 ? label.Substring2(lastOpenBracketPosition, '[', ']') : "";
        }

        public static void AddCommonChartHandlers(this Chart chart)
        {
            if (CurrentChartArea.ContainsKey(chart)) return;

            chart.GetToolTipText += new EventHandler<ToolTipEventArgs>(chart_GetToolTipText);
            chart.KeyDown += new KeyEventHandler(chart_KeyDown);
            chart.PreviewKeyDown += new PreviewKeyDownEventHandler(chart_PreviewKeyDown);
            chart.MouseMove += new MouseEventHandler(chart_MouseMove);
            chart.Disposed += new EventHandler(chart_Disposed);

            //find the immediate parent which can handle keydown events
            Control c = chart.getParentThatHandlesEvent("KeyDown");
            if (c != null)
                if (!ChartParentChildrens.ContainsKey(c))
                {
                    c.KeyDown += new KeyEventHandler(parent_KeyDown);
                    ChartParentChildrens.Add(c, new List<Chart>());
                    CurrentChart.Add(c, null);
                }
                else
                    ChartParentChildrens[c].Add(chart);

            CurrentChartArea.Add(chart, null);
            ChartParents.Add(chart, c);
        }

        //stores the current chartarea for each chart
        private static Dictionary<Chart, ChartArea> CurrentChartArea = new Dictionary<Chart, ChartArea>();
        private static Dictionary<Chart, Control> ChartParents = new Dictionary<Chart, Control>();
        private static Dictionary<Control, List<Chart>> ChartParentChildrens = new Dictionary<Control, List<Chart>>();
        private static Dictionary<Control, Chart> CurrentChart = new Dictionary<Control, Chart>();

        //this is used to set the focus to a specific chartarea
        static void chart_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.None) return;

            Chart ch = (Chart)sender;
            HitTestResult hit = ch.HitTest(e.X, e.Y);
            if (CurrentChart[ChartParents[ch]] != ch)
                CurrentChart[ChartParents[ch]] = ch;

            if(CurrentChartArea[ch]!=hit.ChartArea)
                CurrentChartArea[ch] = hit.ChartArea;
        }
        
        private static void parent_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                    Chart ch=CurrentChart[(Control)sender];
                    if(ch!=null && ch.Visible) chart_KeyDown(ch, e);
                    break;
            }
        }

        public static Control getParentThatHandlesEvent(this Control control,string eventName)
        {
            Control c = control.Parent;
            while (c!=null)
            {
                EventDescriptorCollection events = TypeDescriptor.GetEvents(c);
                EventDescriptor d=events.Find(eventName,true);
                if (d != null && d.IsBrowsable) break;
                else c = c.Parent;
            }
            return c;
        }

        private static void chart_Disposed(object sender, EventArgs e)
        {
            Chart ch = (Chart)sender;
            if (CurrentChartArea.ContainsKey(ch)) 
                ch.ReleaseCommonChartHandlers(); //CurrentChartArea.Remove(ch);
        }

        public static void ReleaseCommonChartHandlers(this Chart chart)
        {
            chart.GetToolTipText -= chart_GetToolTipText;
            chart.KeyDown -= chart_KeyDown;
            chart.PreviewKeyDown -= chart_PreviewKeyDown;
            CurrentChartArea.Remove(chart);
            Control chartParent = ChartParents[chart];
            if (chartParent != null)
            {
                ChartParentChildrens[chartParent].Remove(chart);
                 //if there are no parent charts associations then remove the parent too
                //and its handler
                if (ChartParentChildrens[chartParent].Count == 0)
                {
                    ChartParentChildrens.Remove(chartParent);
                    CurrentChart.Remove(chartParent);
                    chartParent.KeyDown -= parent_KeyDown;
                }
            }

            ChartParents.Remove(chart);
        }     
        
        private static CultureInfo en = CultureInfo.InvariantCulture;
        private static void chart_GetToolTipText(object sender, ToolTipEventArgs e)
        {
            Chart ch = (Chart)sender;
            if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                Series s = e.HitTestResult.Series;
                DataPoint p = s.Points[e.HitTestResult.PointIndex];
                //e.Text =string.Format(en,"{0}: x={1}, y={2}",s.Name, p.XValue,p.YValues[0]);
                //e.Text =string.Format(en,"{0}:({1},{2})",s.Name, p.XValue,p.YValues[0]);

                string xLabel = e.HitTestResult.ChartArea.AxisX.Title;
                string yLabel = e.HitTestResult.ChartArea.AxisY.Title;

                if (string.IsNullOrWhiteSpace(xLabel)) xLabel = "x";
                if (string.IsNullOrWhiteSpace(yLabel)) yLabel = "y";

                LabelUnit xlu = LabelUnit.Parse(xLabel), ylu = LabelUnit.Parse(yLabel);

                string xEq = !xlu.HasUnit ? string.Format(en, "{0}={1}", xLabel, p.XValue) :
                    string.Format(en, "{0}={1} {2}", xlu.Label, p.XValue, xlu.Unit),
                    yEq = !ylu.HasUnit ? string.Format(en, "{0}={1}", yLabel, p.YValues[0]) :
                    string.Format(en, "{0}={1} {2}", s.Name, p.YValues[0], ylu.Unit);

                e.Text = string.Format(en, "{0}, {1}", xEq, yEq);
            }
        }

        //not done should check which chartarea is selected
        public static void chart_KeyDown(object sender, KeyEventArgs e)
        {
            Chart ch = (Chart)sender;
            if (ch.ChartAreas.Count == 0) return;

            Func<ChartArea, bool> isZoomed = (entry) =>
            { return entry.AxisX.ScaleView.IsZoomed || entry.AxisY.ScaleView.IsZoomed; };
            //get the first one that is zoomed if no chartarea has the focus
            ChartArea a = CurrentChartArea[ch] ?? ch.ChartAreas.First(isZoomed);
            if (a == null) return;

            switch (e.KeyData)
            {
                case Keys.Right:
                    a.AxisX.ScaleView.Scroll(ScrollType.SmallIncrement);
                    break;
                case Keys.Left:
                    a.AxisX.ScaleView.Scroll(ScrollType.SmallDecrement);
                    break;
                case Keys.Up:
                    a.AxisY.ScaleView.Scroll(ScrollType.SmallIncrement);
                    break;
                case Keys.Down:
                    a.AxisY.ScaleView.Scroll(ScrollType.SmallDecrement);
                    break;
            }
            e.Handled = true;

        }

        private static void chart_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                    e.IsInputKey = true;
                    break;
            }
        }


        #region Common chart element operations
        public static void SetAxisDefaultProperties(this ChartArea c, int fontSize = 12, bool allStartFromZero = true, bool allowSelection = true)
        {
            //set grid properties
            c.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            c.AxisX.MajorGrid.LineColor = Color.LightGray;
            c.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            c.AxisY.MajorGrid.LineColor = Color.LightGray;
            c.AxisY2.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            c.AxisY2.MajorGrid.LineColor = Color.LightGray;

            //set font sizes
            c.AxisX.LabelStyle.Font = new Font(c.AxisX.LabelStyle.Font.FontFamily, fontSize);
            c.AxisY.LabelStyle.Font = new Font(c.AxisY.LabelStyle.Font.FontFamily, fontSize);
            c.AxisY2.LabelStyle.Font = new Font(c.AxisY2.LabelStyle.Font.FontFamily, fontSize);

            c.AxisX.TitleFont = new Font(c.AxisX.TitleFont.FontFamily, fontSize);
            c.AxisY.TitleFont = new Font(c.AxisY.TitleFont.FontFamily, fontSize);
            c.AxisY2.TitleFont = new Font(c.AxisY2.TitleFont.FontFamily, fontSize);

            c.AxisX.IsStartedFromZero = c.AxisY.IsStartedFromZero = c.AxisY2.IsStartedFromZero = allStartFromZero;

            c.CursorX.IsUserSelectionEnabled = c.CursorY.IsUserSelectionEnabled = allowSelection;

        }

        public static void SetSeriesProperties(this Series s,
            Color lineColor,

            Color markerColor,
            MarkerStyle markerStyle,
            SeriesChartType ct)
        {
            s.ChartType = ct;
            s.MarkerColor = markerColor;
            s.MarkerStyle = markerStyle;
            s.MarkerSize = 10;
            //s.MarkerStep = 5;
            //s.Palette = ChartColorPalette.EarthTones;
            s.Color = lineColor;
            //s.BorderColor = Color.Blue; doyleyei mono an exei kai area
            s.BorderWidth = 2;
            // s.IsValueShownAsLabel = true;
            //s.XValueType = ChartValueType.DateTime;
        }

        public static Series AddPointSeries(this Chart chart,
            ChartArea c, string name,
            Color markerColor, MarkerStyle markerStyle, Color markerBorderColor, int markerSize = 10)
        {
            Series series;
            //if (!string.IsNullOrWhiteSpace(name))
            series = chart.Series.Add(name);

            series.ChartType = SeriesChartType.Point;
            series.ChartArea = c.Name;

            series.MarkerColor = markerColor;
            series.MarkerStyle = markerStyle;
            series.MarkerBorderColor = markerBorderColor;
            series.MarkerSize = markerSize;
            //soot.XValueType = ChartValueType.Double;
            //soot.YValueType = ChartValueType.Double;

            return series;
        }

        public static void AddCentralLine(this Chart chart, double maximumX, double minimumX = 0)
        {
            Series central = chart.Series.Add("Central line");
            central.MarkerStyle = MarkerStyle.None;
            central.BorderDashStyle = ChartDashStyle.Dash;
            central.BorderWidth = 1;
            central.Color = Color.DimGray;
            central.ChartType = SeriesChartType.Line;
            central.Points.AddXY(minimumX, minimumX);
            central.Points.AddXY(maximumX, maximumX);
            central.IsVisibleInLegend = false;
        }

        public static void Add20PercentLines(this Chart chart, double maximumX, double minimumX = 0)
        {
            Series over20 = chart.Series.Add("Line over 20%");
            over20.MarkerStyle = MarkerStyle.None;
            over20.BorderDashStyle = ChartDashStyle.Dash;
            over20.BorderWidth = 1;
            over20.Color = Color.DimGray;
            over20.ChartType = SeriesChartType.Line;
            over20.Points.AddXY(minimumX, minimumX * 1.2);
            over20.Points.AddXY(maximumX, maximumX * 1.2);
            over20.IsVisibleInLegend = false;

            Series below20 = chart.Series.Add("Line below 20%");
            below20.MarkerStyle = MarkerStyle.None;
            below20.BorderDashStyle = ChartDashStyle.Dash;
            below20.BorderWidth = 1;
            below20.Color = Color.DimGray;
            below20.ChartType = SeriesChartType.Line;
            below20.Points.AddXY(minimumX, minimumX * 0.8);
            below20.Points.AddXY(maximumX, maximumX * 0.8);
            below20.IsVisibleInLegend = false;
        }

        public static void CopyImageToClipbard(this Chart chart)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                chart.SaveImage(ms, ChartImageFormat.Bmp);
                Bitmap bm = new Bitmap(ms);
                Clipboard.SetImage(bm);
            }
        }


        #endregion
    }
}
