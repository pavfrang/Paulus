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
    public class GridViewPortsUI : GridViewUI
    {
        public GridViewPortsUI(GridView gridView, MaintainPortsUI parent) :
            base(gridView, parent)
        {
            EnableEditColumns.Add("Cylinder");

            ColumnRepositories.Add("Cylinder", GasMixerRepositories.RepositoryCylinders2);

            ColumnNumericFormats.Add("K-Factor", "0.000");
            ColumnNumericFormats.Add("Volume [l]", "0.0");

            //set the table in the end
            DataTable = EditSettings.DataTables.UpdatePortsTableForDataGrid();

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
            //e.Column  e.Appearance.ForeColor
        }

        protected override async void GridView_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;

            //ColumnView view = sender as ColumnView;
            GridColumn column = (e as EditFormValidateEditorEventArgs)?.Column ?? view.FocusedColumn;
            if (column.FieldName != "Cylinder") return;

            int rowHandle = view.FocusedRowHandle;
            int portID = (int)view.GetFocusedRowCellValue(view.Columns["Port"]);
            Cylinder newCylinder = (Cylinder)e.Value;
            //this is equivalent with the one below
            //Cylinder oldCylinder = (Cylinder)view.GetFocusedRowCellValue(view.Columns["Cylinder"]);
            Cylinder oldCylinder = EditSettings.Ports[portID].Cylinder;

            //  e.Valid = false; return;

            unsavedRows.Add(rowHandle);
            if (DeviceCommander != null && DeviceCommander.IsConnected)
            {
                //view.RefreshData(); //set to yellow
                var command = await DeviceCommander.AssignPortCylinder(portID, newCylinder);
                if (command.Success)
                {
                    unsavedRows.Remove(rowHandle);
                    //change the current settings only if the response is successful!
                    EditSettings.Ports[portID].Cylinder = newCylinder;
                    gridView.RefreshData(); //set to normal
                    OnGridViewChanged();
                }
                else
                {
                    e.Valid = false;
                    e.ErrorText = command.Exception is GasMixerException ? (command.Exception as GasMixerException).LongDescription :
                            command.Exception.Message;

                    view.SetColumnError(column, e.ErrorText, ErrorType.Critical);
                }
            }
        }

        protected override void GridView_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e.Column != gridView.Columns["Cylinder"]) return;

            //get the selected cylinder and update other rows
            Cylinder newCylinder = e.Value as Cylinder;

            DataRow row = gridView.GetDataRow(e.RowHandle);
            int portID = (int)row["Port"];
            row["Code"] = newCylinder.CylinderCode;
            row["ID"] = newCylinder.ID;
            row["K-Factor"] = newCylinder.GasMixture.KFactor;
            row["Volume [l]"] = newCylinder.InitialSizeInLiters;

            //update the internal edit settings
            EditSettings.Ports[portID].Cylinder = newCylinder;
            OnPortChanged(new PortEventArgs(EditSettings.Ports[portID]));
        }
        public event EventHandler<PortEventArgs> PortChanged;
        private void OnPortChanged(PortEventArgs e) =>
            PortChanged?.Invoke(this, e);

        public async Task<bool> AssignCylindersAfterConnection()
        {
            for (int iRow = 0; iRow < dataTable.Rows.Count; iRow++)
            {
                DataRow row = dataTable.Rows[iRow];
                int portID = (int)row["Port"];
                var reply = await deviceCommander.AssignPortCylinder(portID, (Cylinder)row["Cylinder"]);
                if (reply.Success)
                {
                    unsavedRows.Remove(gridView.GetRowHandle(iRow));
                    gridView.RefreshData();
                }
                else
                {
                    OnExceptionThrown(reply.Exception);
                    return false;
                }
            }
            return true;
        }
    }




}
