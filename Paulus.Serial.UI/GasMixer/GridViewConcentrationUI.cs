using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Base;
using System.Data;
using System.Threading.Tasks;
using static Paulus.Serial.GasMixer.UnitConversions;
using Paulus.Common;
using System.Drawing;
using DevExpress.XtraGrid.Columns;
using Paulus.UI;
using Paulus.Serial.GasMixer;
using System.Windows.Forms;

namespace Paulus.Serial.UI.GasMixer
{
    static class GridViewConcentrationExtensions
    {
        public static ConcentrationUnit GetUnit(this DataRow row) =>
            ((string)row["unit"]) == "%" ? ConcentrationUnit.PerCent : ConcentrationUnit.PPM;
    }

    public class GridViewConcentrationUI : GridViewUI
    {
        public GridViewConcentrationUI(GridView gridView, ConcentrationModeUI parentModeUI) :
            base(gridView, parentModeUI)
        {
            EnableEditColumns.AddRange(new string[] { "Target Concentration", "Balance", "Unit" });

            ColumnRepositories.Add("Unit", GasMixerRepositories.RepositoryConcentrationUnits);
            ColumnRepositories.Add("Balance", GasMixerRepositories.RepositoryCheckOnly);

            ColumnNumericFormats.Add("Actual Concentration", "0.0##");
            ColumnNumericFormats.Add("Minimum Concentration", "0.0##");
            ColumnNumericFormats.Add("Maximum Concentration", "0.0##");
            ColumnNumericFormats.Add("Target Flow", "0.0##");
            ColumnNumericFormats.Add("Actual Flow", "0.0##");
            ColumnNumericFormats.Add("Maximum Flow", "0.0##");
            ColumnNumericFormats.Add("Minimum Flow", "0.0##");
            ColumnNumericFormats.Add("Absolute Maximum Flow", "0.0##");

            HeaderWrapTexts.Add("Target Concentration", "Target<br>Conc.");
            HeaderWrapTexts.Add("Actual Concentration", "Actual<br>Conc.");
            HeaderWrapTexts.Add("Minimum Concentration", "Minimum<br>Conc. (10%)");
            HeaderWrapTexts.Add("Maximum Concentration", "Maximum<br>Conc. (90%)");

            HeaderWrapTexts.Add("Target Flow", "Target Flow<br>[l/min]");
            HeaderWrapTexts.Add("Actual Flow", "Actual Flow<br>[l/min]");
            HeaderWrapTexts.Add("Minimum Flow", "Min Flow<br>(10%) [l/min]");
            HeaderWrapTexts.Add("Maximum Flow", "Max Flow<br>(90%) [l/min]");
            HeaderWrapTexts.Add("Absolute Maximum Flow", "Max Flow<br>(100%) [l/min]");

            //CellsMargin = 10;

            //set the table in the end
            DataTable = EditSettings.DataTables.UpdateConcentrationModeTableForDataGrid();

            gridView.EnableClickAndSelectCell();

            //CascadeException = true;

            gridView.GridControl.EditorKeyDown += GridControl_EditorKeyDown;
        }

        bool enterWasPressed = false;
        private void GridControl_EditorKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
                enterWasPressed = true;
        }

        protected override void GridView_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
        {
            //offline validation

            //check for valid concentration values before sending to the device
            GridColumn column = e.GetGridColumn(gridView);
            if (column == gridView.Columns["Target Concentration"])
            {
                DataRow row = gridView.GetFocusedDataRow();
                //continue validation only if 
                float concentration;
                if (!float.TryParse((string)e.Value, out concentration)) return;
                switch (row.GetUnit())
                {
                    case ConcentrationUnit.PerCent:
                        e.Valid = concentration >= 0 && concentration <= 100;
                        if (!e.Valid) e.ErrorText = "Concentration [%] must be in the range [0,100]."; break;
                    case ConcentrationUnit.PPM:
                        e.Valid = concentration >= 0 && concentration <= 1e6;
                        if (!e.Valid) e.ErrorText = "Concentration [ppm] must be in the range [0,1000000]."; break;
                }
            }
        }

        //fires only after manual cell updates

        protected override async void GridView_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            gridView.CellValueChanged -= GridView_CellValueChanged;

            //temporarily disable the editing
            e.Column.OptionsColumn.AllowEdit = false;

            DataRow row = gridView.GetDataRow(e.RowHandle);
            int mfcID = (int)row["MFC"];
            MFC mfc = EditSettings.MFCs[mfcID];
            //balance, unit and target concentration are the only valid inputs

            //changing the unit will not result into an error in any case
            if (e.Column == gridView.Columns["Unit"])
            {
                #region Unit

                if (!mfc.CurrentPort.Cylinder.IsEmptyCylinder())
                {
                    //just the change the target and actual concentration based on the current settings values
                    string unit = (string)e.Value;
                    row["Target Concentration"] =
                        unit == "%" ? ToPerCent(mfc.TargetConcentrationInPpm) : mfc.TargetConcentrationInPpm;

                    row["Actual Concentration"] =
                        unit == "%" ? ToPerCent(mfc.ActualConcentrationInPpm) : mfc.ActualConcentrationInPpm;

                    float sizeRatio = mfc.SizeInCcm / EditSettings.TotalTargetFlowInCcm;

                    float maximumConcentration = (sizeRatio > 1 ? 1 : sizeRatio) * mfc.CurrentPort.ConcentrationInPpm * mfc.CurrentPort.KFactor;
                    row["Minimum Concentration"] = unit == "%" ? ToPerCent(0.1f * maximumConcentration) : 0.1f * maximumConcentration;
                    row["Maximum Concentration"] = unit == "%" ? ToPerCent(maximumConcentration * 0.9f) : maximumConcentration * 0.9f;
                }
                #endregion
            }

            else if (e.Column.FieldName == "Balance")
            {
                #region Balance

                //get previous balance  (should get it from the settings!)
                int previousBalance = EditSettings.BalanceMfc.ID;
                int candidateNewBalance = (int)row["MFC"];

                unsavedRows.Add(previousBalance - 1); //the target concentration must be retrieved
                unsavedRows.Add(candidateNewBalance - 1);

                if (DeviceCommander?.IsConnected ?? false)
                {
                    if ((DeviceCommander.RuntimeSettings as GasMixerSettings).Mode == Mode.Concentration)
                        await (Parent as ConcentrationModeUI).PauseMeasurementTimer();


                    var response = await DeviceCommander.AssignBalance(candidateNewBalance);
                    bool success = response.Success;
                    if (success)
                    {
                        //clear the flag
                        unsavedRows.Remove(candidateNewBalance - 1);
                        //set previous row to false
                        dataTable.Rows[previousBalance - 1]["Balance"] = false;
                        //set current row to true (not needed because the editor has changed the value)
                        //dataTable.Rows[candidateNewBalance - 1]["Balance"] = true;
                        //update the settings balance!
                        EditSettings.BalanceMfc = EditSettings.MFCs[candidateNewBalance];
                        gridView.ClearColumnErrors();

                        var response2 = await DeviceCommander.ReadTargetConcentration(previousBalance);
                        if (success = response2.Success)
                        {
                            string sUnit = (string)gridView.GetRowCellValue(previousBalance - 1, "Unit");
                            unsavedRows.Remove(previousBalance - 1);
                            float concentrationInPpm =
                                EditSettings.MFCs[previousBalance].TargetConcentrationInPpm =  //store to the settings too!
                                (DeviceCommander.RuntimeSettings as GasMixerSettings).MFCs[previousBalance].TargetConcentrationInPpm;
                            dataTable.Rows[previousBalance - 1]["Target Concentration"] = sUnit == "ppm" ? concentrationInPpm : ToPerCent(concentrationInPpm);

                            OnGridViewChanged();
                        }
                    }

                    if (!success)
                    {
                        //go back to the previous balance
                        dataTable.Rows[previousBalance - 1]["Balance"] = true;
                        dataTable.Rows[candidateNewBalance - 1]["Balance"] = false;
                        //EditSettings.BalanceMfc = EditSettings.MFCs[previousBalance];
                        gridView.SetColumnError(e.Column, response.Exception.Message);

                        OnExceptionThrown(response.Exception);
                    }

                    if ((DeviceCommander.RuntimeSettings as GasMixerSettings).Mode == Mode.Concentration)
                        (Parent as ConcentrationModeUI).StartMeasurementTimer();
                }
                else //reset and get previous value
                {
                    //update the settings balance!
                    dataTable.Rows[previousBalance - 1]["Balance"] = false;
                    EditSettings.BalanceMfc = EditSettings.MFCs[candidateNewBalance];
                    gridView.ClearColumnErrors();
                    OnGridViewChanged();
                }


                #endregion
            }
            else if (e.Column == gridView.Columns["Target Concentration"])
            {
                #region Target concentration
                //get previous balance  (should get it from the settings!)
                float previousConcentration = EditSettings.MFCs[mfcID].TargetConcentrationInPpm;
                string sUnit = (string)row["unit"];
                float newConcentration = GetPpm((float)row["Target Concentration"], sUnit);

                unsavedRows.Add(mfcID - 1);
                if (DeviceCommander?.IsConnected ?? false)
                {
                    if ((DeviceCommander.RuntimeSettings as GasMixerSettings).Mode == Mode.Concentration)
                        await (Parent as ConcentrationModeUI).PauseMeasurementTimer();

                    MFC mfcRuntime = RuntimeSettings.MFCs[mfcID];

                    var response = await DeviceCommander.AssignTargetConcentration(mfcID, newConcentration);
                    if (response.Success)
                    {

                        unsavedRows.Remove(mfcID - 1);
                        //update the settings target concentration
                        mfc.TargetConcentrationInPpm = mfcRuntime.TargetConcentrationInPpm = newConcentration;

                        //also update the target flow command
                        mfc.TargetFlowInCcm = mfcRuntime.TargetFlowInCcm = EditSettings.TotalTargetFlowInCcm * mfc.TargetConcentrationInPpm / mfc.CurrentPort.ConcentrationInPpm / mfc.CurrentPort.KFactor;
                        row["Target Flow"] = ToLpm(mfc.TargetFlowInCcm);

                        gridView.ClearColumnErrors();

                        OnGridViewChanged();
                    }
                    else  //error! (revert value)
                    {
                        dataTable.Rows[mfcID - 1]["Target Concentration"] =
                            sUnit == "%" ? ToPerCent(previousConcentration) : previousConcentration;

                        //also update the target flow command
                        mfc.TargetFlowInCcm = mfcRuntime.TargetFlowInCcm = EditSettings.TotalTargetFlowInCcm * previousConcentration / mfc.CurrentPort.ConcentrationInPpm / mfc.CurrentPort.KFactor;
                        dataTable.Rows[mfcID - 1]["Target Flow"] = UnitConversions.ToLpm(mfc.TargetFlowInCcm);

                        gridView.SetColumnError(gridView.Columns["Target Concentration"], response.Exception.Message);
                        OnExceptionThrown(response.Exception);
                    }

                    if ((DeviceCommander.RuntimeSettings as GasMixerSettings).Mode == Mode.Concentration)
                        //resume the measurement timer
                        (Parent as ConcentrationModeUI).StartMeasurementTimer();
                }
                else// if agent is null then just update the value
                {    //update the settings target concentration
                    mfc.TargetConcentrationInPpm = newConcentration;
                    OnGridViewChanged();
                }

                #endregion
            }

            //reset the handler
            gridView.CellValueChanged += GridView_CellValueChanged;
            gridView.CustomColumnDisplayText += GridView_CustomColumnDisplayText;

            gridView.RefreshData();
            e.Column.OptionsColumn.AllowEdit = true;

            if (enterWasPressed)
            {
                enterWasPressed = false;
                if (gridView.IsValidRowHandle(gridView.FocusedRowHandle + 1))
                    gridView.FocusedRowHandle += 1;
            }
        }

        private void GridView_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column == gridView.Columns["Status"])
            {
                var cellValue = (Warning)e.Value;
                e.DisplayText = cellValue.GetDescription();
            }

        }

        protected override void GridView_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            base.GridView_CustomDrawCell(sender, e);
            object concentrationValue = gridView.GetRowCellValue(e.RowHandle, "Actual Concentration");
            if (concentrationValue == DBNull.Value) return;

            bool hasFlow = (float)concentrationValue != 0.0f;

            if (e.Column.FieldName == "Status")
            {
                var cellValue = (Warning)e.CellValue;
                if (cellValue != Warning.NoWarning)
                {
                    e.Appearance.ForeColor = Color.Red;
                    e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Bold);
                }
            }
            else if (hasFlow) //if(e.Column.FieldName=="Actual Concentration")
            {
                e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Bold);

            }

        }

        public async Task<bool> AssignTargetConcentrationsAndBalanceAfterConnection()//AssignTargetValuesToGoToConcentration()
        {
            //first set the balance
            int balanceID = dataTable.AsEnumerable().Where(row => (bool)row["Balance"]).Select(row => (int)row["MFC"]).First();
            var reply = await DeviceCommander.AssignBalance(balanceID); //ok
            if (!reply.Success)
            {
                OnExceptionThrown(reply.Exception);
                return false;
            }
            else
            {
                //unsavedRows.Remove(gridView.GetRowHandle(balanceID - 1));
                gridView.RefreshData();
            }


            for (int iRow = 0; iRow < dataTable.Rows.Count; iRow++)
            //foreach (DataRow row in (GridViewPorts.GridControl.DataSource as DataTable).AsEnumerable())
            {
                if (!unsavedRows.Contains(iRow)) continue;

                DataRow row = dataTable.Rows[iRow];
                int mfcID = (int)row["MFC"];
                //if (mfcID == balanceID) continue; //also assign the previous target
                float concentrationInPpm = !EditSettings.MFCs[mfcID].CurrentPort.Cylinder.IsEmptyCylinder() ?
                    GetPpm((float)row["Target Concentration"], (string)row["Unit"]) : 0.0f;
                reply = await DeviceCommander.AssignTargetConcentration(mfcID, concentrationInPpm);
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

        public void CheckAndChangeCylinderInformationIfPortIsChanged(int portID)
        {
            DataRow mfcRow = dataTable.AsEnumerable().Where(r => (int)r["Port"] == portID).FirstOrDefault();
            if (mfcRow == null) return;

            updateCylinderInformation(portID, mfcRow);
        }

        public void ChangeMfcPort(int mfcID)
        {
            DataRow mfcRow = dataTable.AsEnumerable().Where(r => (int)r["MFC"] == mfcID).FirstOrDefault();
            mfcRow["Port"] = EditSettings.MFCs[mfcID].CurrentPort.ID;

            updateCylinderInformation(EditSettings.MFCs[mfcID].CurrentPort.ID, mfcRow);
        }

        private void updateCylinderInformation(int portID, DataRow mfcRow)
        {
            Cylinder cylinder = EditSettings.Ports[portID].Cylinder;
            int mfcID = (int)mfcRow["MFC"];
            MFC mfc = EditSettings.MFCs[mfcID];
            Port currentPort = mfc.CurrentPort;
            unsavedRows.Add(mfcID - 1);

            //also reset current target concentration and unit based on the cylinder
            mfcRow["Cylinder"] = cylinder;
            mfcRow["Target Concentration"] = 0.0;
            mfcRow["Unit"] = cylinder.GetDefaultConcentrationUnit().GetDisplayName();

            float sizeRatio = mfc.SizeInCcm / EditSettings.TotalTargetFlowInCcm;

            float maximumConcentration = (sizeRatio > 1 ? 1 : sizeRatio) * currentPort.ConcentrationInPpm * currentPort.KFactor;
            string unit = (string)mfcRow["Unit"];

            if (!mfc.CurrentPort.Cylinder.IsEmptyCylinder())
            {
                if (unit == "ppm")
                {
                    mfcRow["Target Concentration"] = mfc.TargetConcentrationInPpm;
                    mfcRow["Maximum Concentration"] = maximumConcentration * 0.9f;
                    mfcRow["Minimum Concentration"] = maximumConcentration * 0.1f;
                }
                else //if (unit == ConcentrationUnit.PerCent) //per cent
                {
                    mfcRow["Target Concentration"] = ToPerCent(mfc.TargetConcentrationInPpm);
                    mfcRow["Maximum Concentration"] = ToPerCent(maximumConcentration * 0.9f);
                    mfcRow["Minimum Concentration"] = ToPerCent(maximumConcentration * 0.1f);
                }
            }
            else mfcRow["Maximum Concentration"] = mfcRow["Minimum Concentration"] = DBNull.Value;

            if (!mfc.CurrentPort.Cylinder.IsEmptyCylinder())
            {
                mfcRow["Target Flow"] = EditSettings.TotalTargetFlowInCcm / 1000.0f * mfc.TargetConcentrationInPpm / currentPort.ConcentrationInPpm / currentPort.KFactor;
                mfcRow["Actual Flow"] = EditSettings.TotalActualFlowInCcm / 1000.0f * mfc.ActualConcentrationInPpm / currentPort.ConcentrationInPpm / currentPort.KFactor;
            }
            else
                mfcRow["Target Flow"] = mfcRow["Actual Flow"] = DBNull.Value;

            float maximumFlow = (sizeRatio > 1f ? EditSettings.TotalTargetFlowInCcm : mfc.SizeInCcm) / 1000.0f / mfc.CurrentPort.KFactor;
            mfcRow["Minimum Flow"] = maximumFlow * 0.1f;
            mfcRow["Maximum Flow"] = maximumFlow * 0.9f;
            mfcRow["Absolute Maximum Flow"] = maximumFlow; //in lpm

            gridView.RefreshData();
        }

    }
}
