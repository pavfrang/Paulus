using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using DevExpress.XtraGrid.Views.Grid;
using Paulus.UI;
using System.Drawing;
using System.Diagnostics;
using System.Threading;

using UITimer = System.Windows.Forms.Timer;

namespace Paulus.Serial.UI
{
    public class MessagesUI : TraceListener
    {
        public MessagesUI(GridView gridView)
        {
            GridView = gridView;
            InitializeDataTable();
            InitializeGridView();
            SortByDescendingTime();

            traceTimer = new UITimer();
            traceTimer.Interval = 500;
            traceTimer.Tick += TraceTimer_Tick;
            traceTimer.Start();
        }



        #region TraceListener
        public override void Write(string message)
        {
            TraceEvent(null, "Application", TraceEventType.Information, (int)InformationState.Information, message);
        }

        public override void WriteLine(string message)
        {
            TraceEvent(null, "Application", TraceEventType.Information, (int)InformationState.Information, message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {

            TraceEvent(eventCache, source, eventType, id,
                args != null ? string.Format(format, args) : format);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            switch (eventType)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    AddMessage(InformationState.Error, source, message); break;
                case TraceEventType.Warning:
                    AddMessage(InformationState.Warning, source, message); break;
                //ignore only verbose messages
                case TraceEventType.Verbose:
                    return;
                default:
                    AddMessage(InformationState.Information, source, message); break;
            }
        }
        #endregion


        public DataTable Table { get; private set; }

        public GridView GridView { get; }

        /// <summary>
        /// Initializes the data table and assigns itself to the gridview.
        /// </summary>
        private void InitializeDataTable()
        {

            Table = new DataTable();
            Table.Columns.Add("Status", typeof(InformationState));
            Table.Columns.Add("Time", typeof(DateTime));
            Table.Columns.Add("Device", typeof(string));
            Table.Columns.Add("Description", typeof(string));

            if (GridView != null)
            {
                GridView.GridControl.DataSource = Table;
                SetColumnProperties();
            }

        }

        private void InitializeGridView()
        {
            // GridView.CenterColumnHeaders();

            GridView.InitializeStaticGridView(true, true);
            GridView.OptionsBehavior.Editable = false;
            GridView.OptionsSelection.EnableAppearanceFocusedCell = false;
            GridView.FocusRectStyle = DrawFocusRectStyle.RowFocus;
            GridView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.False;
            GridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.False;

            GridView.CustomDrawCell += GridView_CustomDrawCell;

            if (Table != null)
            {
                GridView.GridControl.DataSource = Table;
                SetColumnProperties();
            }

        }

        private void SetColumnProperties()
        {
            var columns = GridView.Columns;
            //columns["Status"].Caption = "";
            columns["Description"].OptionsFilter.AllowFilter = false;
            GridView.AutoFillColumn = columns["Description"];

            columns["Time"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            columns["Time"].DisplayFormat.FormatString = "HH:mm:ss"; //.fff
            columns["Status"].SetStatusGridColumn();

            columns["Time"].Width = 60;
            columns["Device"].Width = 100;


        }

        private void GridView_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            InformationState state = (InformationState)GridView.GetRowCellValue(e.RowHandle, GridView.Columns["Status"]);
            switch (state)
            {
                case InformationState.Error:
                    e.Appearance.ForeColor = Color.Red;
                    e.DefaultDraw();
                    e.Handled = true; break;
                case InformationState.Warning:
                    e.Appearance.ForeColor = Color.Brown;
                    e.DefaultDraw();
                    e.Handled = true; break;
                case InformationState.Information:
                    e.Appearance.ForeColor = Color.DarkGreen;
                    e.DefaultDraw();
                    e.Handled = true; break;
            }
        }

        public void SortByDescendingTime()
        {
            if (Table == null || GridView == null) return;

            GridView.ClearSorting();
            GridView.Columns["Time"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;
        }


        UITimer traceTimer;
        //the queue is needed in order to avoid cross-thread calls
        Queue<DataRow> addedMessages = new Queue<DataRow>();
        private void TraceTimer_Tick(object sender, EventArgs e)
        {
            bool hasAddedMessages = addedMessages.Count > 0;
            if (!hasAddedMessages) return;

            while (addedMessages.Count > 0)
                Table.Rows.Add(addedMessages.Dequeue());

            GridView.RefreshData();
            GridView.FocusedRowHandle = 0;
            // GridView.MakeRowVisible(0); //always make sure that the first row is visible (this corresponds to the latest event)
        }

        public void AddMessage(InformationState state, string deviceName, string description, DateTime time)
        {
            DataRow newRow = Table.NewRow();
            newRow["Status"] = state;
            newRow["Time"] = time;
            newRow["Device"] = deviceName;
            newRow["Description"] = description;

            addedMessages.Enqueue(newRow);

            //(the following is done in the TraceTimer_Tick handler)
            // GridView.MakeRowVisible()
            //Table.Rows.Add(newRow);
        }

        public void AddMessage(InformationState state, string deviceName, string description) =>
            AddMessage(state, deviceName, description, DateTime.Now);

    }


}
