using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Paulus.Serial.GasMixer;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace Paulus.Serial.UI.GasMixer
{
    public class GridViewMfcsUI : GridViewUI
    {
        public GridViewMfcsUI(GridView gridView, MaintainPortsUI parent) :
            base(gridView, parent)
        {
            EnableEditColumns.Add("Port");

            ColumnRepositories.Add("Port", GasMixerRepositories.RepositoryPorts);

            ColumnNumericFormats.Add("Maximum Flow [l/min]", "0.0");

            HeaderWrapTexts.Add("Maximum Flow [l/min]", $"Max Flow<br>[l/min]");

            //set the table in the end
            DataTable = EditSettings.DataTables.UpdateMfcsTableForDataGrid();

            //up until 4 MFCs with double port
            Color[] colors = new Color[] { Color.Blue, Color.Green, Color.Red, Color.Gold };
            int iColor = 0;
            mfcColors = new Dictionary<MFC, Color>();
            foreach (var entry in EditSettings.MFCs.Where(m => m.Value.Ports.Count > 1))
                mfcColors.Add(entry.Value, colors[iColor++]);

        }

        private Dictionary<MFC, Color> mfcColors;

        protected override void GridView_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            base.GridView_CustomDrawCell(sender, e);

            if (e.Column == gridView.Columns["Port"])
            {
                var s = Parent.EditSettings; if (s == null) return;
                Port thisPort = Parent.EditSettings.Ports[(int)e.CellValue];
                MFC parentMfc = Parent.EditSettings.MFCs.Where(entry => entry.Value.Ports.Contains(thisPort)).Select(entry => entry.Value).FirstOrDefault();
                if (parentMfc == null) return;
                if (mfcColors.ContainsKey(parentMfc))
                    e.Appearance.ForeColor = mfcColors[parentMfc];
            }
            else if (e.Column == gridView.Columns["MFC"])
            {
                MFC mfc = Parent.EditSettings.MFCs[(int)e.CellValue];
                if (mfcColors.ContainsKey(mfc))
                    e.Appearance.ForeColor = mfcColors[mfc];

            }
        }


        protected override void initialize()
        {
            base.initialize();

            gridView.FocusedRowChanged += GridView_FocusedRowChanged;
        }

        private void GridView_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            //populate the repository based on the focused row
            if (gridView.IsValidRowHandle(e.FocusedRowHandle))
            {
                DataRow row = gridView.GetDataRow(e.FocusedRowHandle);
                int mfc = (int)row["MFC"];
                GasMixerRepositories.LoadPorts(EditSettings.MFCs[mfc].Ports);
            }
        }

        protected override async void GridView_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e.Column != gridView.Columns["Port"]) return;

            DataRow row = gridView.GetDataRow(e.RowHandle);
            int newPortID = (int)e.Value;
            int mfcID = (int)row["MFC"];

            //get the selected cylinder from the ports table and update other rows
            Port newPort = EditSettings.Ports[newPortID];
            Cylinder newPortCylinder = newPort.Cylinder;
            MFC mfc = EditSettings.MFCs[mfcID];
            row["Cylinder"] = newPortCylinder;

            unsavedRows.Add(e.RowHandle);
            //if (DeviceCommander != null && DeviceCommander.IsConnected)
            if (DeviceCommander?.IsConnected ??  false) //(DeviceCommander!= null && DeviceCommander.IsConnected)
            {
                gridView.RefreshData(); //set to yellow
                var command = await DeviceCommander.AssignPortToMfc(newPortID, mfcID);
                if (command.Success)
                {
                    unsavedRows.Remove(e.RowHandle);
                    gridView.RefreshData(); //set to normal

                    mfc.CurrentPort = newPort;
                    OnMfcPortChanged(mfc);
                    OnGridViewChanged();
                }
            }
            else
            {//just raise the event
                mfc.CurrentPort = newPort;
                OnMfcPortChanged(mfc);
                OnGridViewChanged();
            }

        }

        public event EventHandler<MfcEventArgs> MfcPortChanged;
        private void OnMfcPortChanged(MfcEventArgs e) =>
            MfcPortChanged?.Invoke(this, e);
        private void OnMfcPortChanged(MFC mfc) =>
            OnMfcPortChanged(new MfcEventArgs(mfc));

        protected override void GridView_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
        {
            // throw new NotImplementedException();
        }

        public async Task<bool> AssignMfcPortsAfterConnection()
        {
            for (int iRow = 0; iRow < dataTable.Rows.Count; iRow++)//foreach (DataRow row in (GridViewPorts.GridControl.DataSource as DataTable).AsEnumerable())
            {
                DataRow row = dataTable.Rows[iRow];
                int mfcID = (int)row["MFC"];
                int portID = (int)row["Port"];
                var reply = await DeviceCommander.AssignPortToMfc(portID, mfcID);
                if (!reply.Success)
                {
                    OnExceptionThrown(reply.Exception);
                    return false;
                }
                else
                {
                    unsavedRows.Remove(gridView.GetRowHandle(iRow));
                    gridView.RefreshData();
                }
            }
            return true;
        }

    }

    [Serializable]
    public class MfcEventArgs : EventArgs
    {
        public MfcEventArgs(MFC mfc)
        {
            this.mfc = mfc;
        }

        private MFC mfc;
        public MFC MFC { get { return mfc; } }
    }
}
