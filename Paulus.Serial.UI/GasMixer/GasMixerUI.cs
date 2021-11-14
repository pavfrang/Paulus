using System;
using System.Collections.Generic;
using System.Linq;

using Paulus.Serial.GasMixer;
using DevExpress.XtraGrid.Views.Grid;
using System.Data;
using DevExpress.XtraEditors;
using System.Threading.Tasks;
using DevExpress.XtraGauges.Win.Gauges.State;
using Paulus.UI;

namespace Paulus.Serial.UI.GasMixer
{

    public sealed class GasMixerUI : DeviceUIBase<GasMixerCommander, GasMixerSettings>
    {
        public GasMixerUI(GasMixerCommander commander) :
            base(commander)
        {
            //initialize repository cylinders
            GasMixerRepositories.LoadCylinders(EditSettings.CylinderLibrary);

            //disable enabling controls in all modes
            CascadeEnableControls = false;
        }

        //public static string GetDefaultLastConfigurationFilePath() =>
        //    Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "last_configuration.xml");

        public MaintainPortsUI MaintainPortsUI { get; private set; }

        public MaintainPortsUI InitializeMaintainPortsUI(GridView gridViewPorts, GridView gridViewMfcs)
        {
            MaintainPortsUI = new MaintainPortsUI(gridViewPorts, gridViewMfcs, this);
            MaintainPortsUI.PortsUI.PortChanged += GridViewPortsUI_PortChanged;
            MaintainPortsUI.MFCsUI.MfcPortChanged += MFCsUI_MfcPortChanged;
            return MaintainPortsUI;
        }

        private void GridViewPortsUI_PortChanged(object sender, PortEventArgs e)
        {
            //this occurs every time a cylinder changes
            int portID = e.Port.ID;
            Cylinder cylinder = e.Port.Cylinder;

            ConcentrationModeUI.GridViewUI.CheckAndChangeCylinderInformationIfPortIsChanged(portID);

            //automatically update the table (no need for sending a special command to the database)
            foreach (DataTable table in new DataTable[] {
                    EditSettings.DataTables.MFCsTable, EditSettings.DataTables.ConcentrationModeTable, EditSettings.DataTables.FlowModeTable })
            {
                if (table == null) continue;
                DataRow mfcRow = table.AsEnumerable().Where(r => (int)r["Port"] == portID).FirstOrDefault();
                if (mfcRow == null) continue;
                mfcRow["Cylinder"] = cylinder;
            }
        }

        private void MFCsUI_MfcPortChanged(object sender, MfcEventArgs e)
        {
            ConcentrationModeUI.GridViewUI.ChangeMfcPort(e.MFC.ID);
        }

        #region Concentration mode

        public ConcentrationModeUI ConcentrationModeUI { get; private set; }
        public ConcentrationModeUI InitializeConcentrationModeUI(
            GridView gridViewConcentration,
            TextEdit textTargetFlow, SimpleButton applyTargetFlow, LabelControl lblActualFlow,
            SimpleButton updateButton, SimpleButton stopButton,
            LabelControl labelFlow, StateIndicatorComponent flowGauge)
        {
            gridViewConcentration.GridControl.ExternalRepository = GasMixerRepositories.Repository;
            return ConcentrationModeUI = new ConcentrationModeUI(gridViewConcentration,
                textTargetFlow, applyTargetFlow, lblActualFlow, updateButton, stopButton, labelFlow, flowGauge, this);
        }
        #endregion

        #region Purge mode
        public PurgeModeUI PurgeModeUI { get; private set; }
        public PurgeModeUI InitializePurgeModeUI(GridView gridViewPurge,
            SimpleButton updateButton, SimpleButton stopButton,
            LabelControl labelFlow, StateIndicatorComponent flowGauge)
        {
            // gridViewPurge.GridControl.ExternalRepository = GasMixerRepositories.Repository;
            return PurgeModeUI = new PurgeModeUI(gridViewPurge, updateButton, stopButton, labelFlow, flowGauge, this);
        }
        #endregion


        #region Modes
        public Mode Mode
        {
            get
            {
                return
                    (DeviceCommander.RuntimeSettings as GasMixerSettings).Mode;
            }
        }

        public event EventHandler ModeLoading;
        public event EventHandler ModeLoaded;

        public async Task<bool> GotoPortMaintenanceMode(bool initializeGasMixer)
        {
            if (Mode == Mode.MaintainPorts && !initializeGasMixer) return true;

            ModeLoading?.Invoke(this, EventArgs.Empty);
            if (Mode == Mode.Concentration)
                await ConcentrationModeUI.PauseMeasurementTimer();
            else if (Mode == Mode.Purge)
                await PurgeModeUI.PauseMeasurementTimer();

            //ensure that concentration mode controls (buttons) are disabled
            ConcentrationModeUI.DisableControls();
            PurgeModeUI.DisableControls();

            bool success = await MaintainPortsUI.GotoPortMaintenanceMode(initializeGasMixer);

            (DeviceCommander.RuntimeSettings as GasMixerSettings).Mode = Mode.MaintainPorts;
            (DeviceCommander.EditSettings as GasMixerSettings).Mode = Mode.MaintainPorts;
            ModeLoaded?.Invoke(this, EventArgs.Empty);
            return success;
        }

        public async Task<bool> GoToConcentrationMode()
        {
            if (Mode == Mode.Concentration) return true;
            if (Mode == Mode.Purge)
                await PurgeModeUI.PauseMeasurementTimer();
            if (Mode == Mode.Purge) await deviceCommander.Home(); //we send a HOME first to avoid the 20/20 port error (?)

            ModeLoading?.Invoke(this, EventArgs.Empty);
            bool success = await ConcentrationModeUI.GotoConcentrationMode();

            (DeviceCommander.RuntimeSettings as GasMixerSettings).Mode = Mode.Concentration;
            (DeviceCommander.EditSettings as GasMixerSettings).Mode = Mode.Concentration;
            PurgeModeUI.DisableControls();

            ModeLoaded?.Invoke(this, EventArgs.Empty);
            return success;
        }


        public async Task<bool> GotoPurgeMode()
        {
            if (Mode == Mode.Purge) return true;
            if (Mode == Mode.Concentration)
                await ConcentrationModeUI.PauseMeasurementTimer();
            ConcentrationModeUI.DisableControls();

            ModeLoading?.Invoke(this, EventArgs.Empty);
            bool success = await PurgeModeUI.GotoPurgeMode();

            (DeviceCommander.RuntimeSettings as GasMixerSettings).Mode = Mode.Purge;
            (DeviceCommander.EditSettings as GasMixerSettings).Mode = Mode.Purge;
            ModeLoaded?.Invoke(this, EventArgs.Empty);
            return success;

        }





        #endregion

        #region GridView Flow
        public GridView GridViewFlow { get; private set; }
        private void UpdateGridViewFlowColumnsAfterLoading()
        {
            GridViewFlow.EnableEditForSpecificColumns("Target Flow", "Unit");

            var columns = GridViewFlow.Columns;
            columns["Unit"].ColumnEdit = GasMixerRepositories.RepositoryFlowUnits;

            columns["Target Flow"].SetHeaderWrapText("Target<br>Flow [l/min]");
            columns["Actual Flow"].SetHeaderWrapText("Actual<br>Flow [l/min]");
            //columns["Cylinder Volume [l]"].SetHeaderWrapText("Cylinder<br>Volume [l/min]");

            GridViewFlow.BestFitColumnsWithMargin(10);

            columns["Unit"].Width = columns["Unit"].Width + 20;

        }

        public void AssignFlowsAfterConnection() { }
        #endregion

        #region GridView Purge
        public GridView GridViewPurge { get; private set; }
        private void UpdateGridViewPurgeColumnsAfterLoading()
        {
            GridViewPurge.EnableEditForSpecificColumns("Target Flow", "Unit", "On/Off");

            var columns = GridViewPurge.Columns;
            columns["Unit"].ColumnEdit = GasMixerRepositories.RepositoryFlowUnits;

            columns["Maximum Flow"].SetHeaderWrapText("Maximum<br>Flow");
            columns["Target Flow"].SetHeaderWrapText("Target<br>Flow");
            columns["Actual Flow"].SetHeaderWrapText("Actual<br>Flow");

            GridViewPurge.BestFitColumnsWithMargin(10);
            columns["Unit"].Width = columns["Unit"].Width + 20;
        }

        public void AssignPurgeFlowsAfterConnection() { }

        #endregion

        #region Flow mode data table
        public DataTable FlowModeTable { get; private set; }

        //when a cylinder row is saved then it is saved to the gas mixer device
        HashSet<int> unsavedFlowRows = new HashSet<int>();


        public void LoadFlowModeFromSettings(GasMixerSettings settings)
        {
            FlowModeTable = settings.DataTables.UpdateFlowModeTableForDataGrid();

            //invalidate all rows until the moment when the values are saved to the gas mixer device
            for (int iRow = 0; iRow < FlowModeTable.Rows.Count; iRow++)
                unsavedFlowRows.Add(GridViewFlow.GetRowHandle(iRow));

            GridViewFlow.LoadTableAndPrepareColumns(FlowModeTable);
        }

        #endregion

        #region Flow mode data table
        public DataTable PurgeModeTable { get; private set; }

        //when a cylinder row is saved then it is saved to the gas mixer device
        HashSet<int> unsavedPurgeRows = new HashSet<int>();


        public void LoadPurgeModeFromSettings(GasMixerSettings settings)
        {
            PurgeModeTable = settings.DataTables.UpdatePurgeModeTableForDataGrid();

            //invalidate all rows until the moment when the values are saved to the gas mixer device
            for (int iRow = 0; iRow < PurgeModeTable.Rows.Count; iRow++)
                unsavedPurgeRows.Add(GridViewPurge.GetRowHandle(iRow));

            GridViewPurge.LoadTableAndPrepareColumns(PurgeModeTable);
        }

        #endregion


    }
}
