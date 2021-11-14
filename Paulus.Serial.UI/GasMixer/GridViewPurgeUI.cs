using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.XtraGrid.Views.Base;
using Paulus.Serial.GasMixer;
using System.Data;
using DevExpress.XtraEditors.Repository;
using Paulus.UI;

namespace Paulus.Serial.UI.GasMixer
{
    public class GridViewPurgeUI : GridViewUI
    {
        public GridViewPurgeUI(GridView gridView, PurgeModeUI parentModeUI) :
            base(gridView, parentModeUI)
        {
            EnableEditColumns.AddRange(new string[] { "On/Off", "Target Flow", "Unit" });

            onOffRepository = new RepositoryItemCheckEdit();
            onOffRepository.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Standard;
            onOffRepository.EditValueChanged += OnOffRepository_EditValueChanged;
            ColumnRepositories.Add("On/Off", onOffRepository);

            ColumnNumericFormats.Add("Maximum Flow", "0.0");
            ColumnNumericFormats.Add("Target Flow", "0.###");

            HeaderWrapTexts.Add("Target Flow", "Target<br>Flow [l/min]");
            //HeaderWrapTexts.Add("Actual Flow", "Actual<br>Flow [l/min]");
            HeaderWrapTexts.Add("Maximum Flow", "Max Flow<br>[l/min]");

            //CellsMargin = 10;

            //set the table in the end
            DataTable = EditSettings.DataTables.UpdatePurgeModeTableForDataGrid();
            var columns = gridView.Columns;
            columns["Unit"].Visible = false;

            gridView.EnableClickAndSelectCell();


            //CascadeException = true;
        }

        private void OnOffRepository_EditValueChanged(object sender, EventArgs e)
        {
            gridView.PostEditor();
        }

        RepositoryItemCheckEdit onOffRepository;


        protected override async void GridView_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            gridView.CellValueChanged -= GridView_CellValueChanged;

            //temporarily disable the editing
            e.Column.OptionsColumn.AllowEdit = false;

            DataRow row = gridView.GetDataRow(e.RowHandle);
            int mfcID = (int)row["MFC"];
            MFC mfc = EditSettings.MFCs[mfcID];

            if (e.Column.FieldName == "On/Off")
            {
                unsavedRows.Add(mfcID - 1);
                bool isOn = (bool)e.Value;

                if (DeviceCommander?.IsConnected ?? false)
                {
                    if ((DeviceCommander.RuntimeSettings as GasMixerSettings).Mode == Mode.Purge)
                        await (Parent as PurgeModeUI).PauseMeasurementTimer();

                    if (isOn)
                    {
                        var response = await deviceCommander.SetPurgeOn(mfcID);
                        if (response.Success)
                        {
                            unsavedRows.Remove(mfcID - 1);
                            mfc.IsPurgeOn = true;
                            OnGridViewChanged();

                        }
                    }
                    else
                    {
                        var response = await deviceCommander.SetPurgeOff(mfcID);
                        if (response.Success)
                        {
                            unsavedRows.Remove(mfcID - 1);
                            mfc.IsPurgeOn = false;
                            OnGridViewChanged();

                        }
                    }

                    if ((DeviceCommander.RuntimeSettings as GasMixerSettings).Mode == Mode.Purge)
                        (Parent as PurgeModeUI).StartMeasurementTimer();
                }
                else //just update the internal settings
                {
                    mfc.IsPurgeOn = isOn;
                    OnGridViewChanged();

                }

            }
            else if (e.Column.FieldName == "Target Flow")
            {
                unsavedRows.Add(mfcID - 1);
                float previousFlowInCcm = EditSettings.MFCs[mfcID].TargetPurgeFlowInCcm;

                float targetFlowInLpm = (float)e.Value;

                if (deviceCommander?.IsConnected ?? false)
                {
                    if ((DeviceCommander.RuntimeSettings as GasMixerSettings).Mode == Mode.Purge)
                        await (Parent as PurgeModeUI).PauseMeasurementTimer();

                    var response = await deviceCommander.AssignPurgeTargetFlow(mfcID, UnitConversions.ToCcm(targetFlowInLpm));
                    if (response.Success)
                    {
                        unsavedRows.Remove(mfcID - 1);
                        mfc.TargetPurgeFlowInCcm = UnitConversions.ToCcm(targetFlowInLpm);
                        gridView.ClearColumnErrors();
                        OnGridViewChanged();
                    }
                    else //error! revert now
                    {
                        dataTable.Rows[mfcID - 1]["Target Flow"] = UnitConversions.ToLpm(previousFlowInCcm);
                        gridView.SetColumnError(gridView.Columns["Target Flow"], response.Exception.Message);
                        OnExceptionThrown(response.Exception);
                    }

                    if ((DeviceCommander.RuntimeSettings as GasMixerSettings).Mode == Mode.Purge)
                        (Parent as PurgeModeUI).StartMeasurementTimer();
                }
                else //we are still offline
                {
                    mfc.TargetPurgeFlowInCcm = UnitConversions.ToCcm(targetFlowInLpm);
                    OnGridViewChanged();
                }
            }


            //reset the handler
            gridView.CellValueChanged += GridView_CellValueChanged;
            gridView.RefreshData();
            e.Column.OptionsColumn.AllowEdit = true;
        }

        public async Task<bool> AssignTargetConcentrationsAfterConnection()
        {
            //first we ned to call purge on or off for each MFC
            for (int iRow = 0; iRow < dataTable.Rows.Count; iRow++)
            {
                if (!unsavedRows.Contains(iRow)) continue;

                DataRow row = dataTable.Rows[iRow];
                int mfcID = (int)row["MFC"];

                bool isOn = (bool)row["On/Off"];

                var reply = isOn ? await deviceCommander.SetPurgeOn(mfcID) : await deviceCommander.SetPurgeOff(mfcID);
                if (!reply.Success)
                {
                    OnExceptionThrown(reply.Exception);
                    return false;
                }

                float targetFlowInCcm = UnitConversions.ToCcm((float)row["Target Flow"]);
                reply = await deviceCommander.AssignPurgeTargetFlow(mfcID, targetFlowInCcm);
                if (!reply.Success)
                {
                    OnExceptionThrown(reply.Exception);
                    return false;
                }

                unsavedRows.Remove(gridView.GetRowHandle(iRow));
                gridView.RefreshData();
            }
            return true;
        }
    }
}
