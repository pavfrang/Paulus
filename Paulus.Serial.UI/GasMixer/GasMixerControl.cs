using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using Paulus.Common;
using System.Diagnostics;
using DevExpress.XtraBars.Navigation;
using Paulus.Serial.UI;
using Paulus.Serial.GasMixer;

namespace Paulus.Serial.UI.GasMixer
{
    public partial class GasMixerControl : DeviceCommanderControl
    {
        public GasMixerControl()
        {
            InitializeComponent();

            //tabPaneGasMixer.SelectedPageIndex = 0;
        }

        //protected SerialPort serialPort;
        //public SerialPort SerialPort
        //{
        //    get { return serialPort; }
        //    set

        //    { }
        //}

        public GasMixerUI UI { get; private set; }

        protected override void initializeUI()
        {
            UI = new GasMixerUI(commander as GasMixerCommander);
            UI.InitializeMaintainPortsUI(gridViewPorts, gridViewMfcs);
            UI.InitializeConcentrationModeUI(gridViewConcentration, txtTargetTotalFlow, btnApplyTargetTotalFlow,
                lblActualTotalFlow, btnConcentrationUpdate, btnConcentrationStop, lblConcentrationFlowStatus, gaugeFlow);
            UI.InitializePurgeModeUI(gridViewPurge, btnPurgeUpdate, btnPurgeStop, lblPurgeFlowStatus, gaugePurgeFlow);
            UI.ExceptionThrown += UI_ExceptionThrown;

            //disable all controls (will be enabled after connection)
            UI.DisableControls();
        }

        private void tabPane_SelectedPageChanging(object sender, SelectedPageChangingEventArgs e)
        {
            e.Cancel = IsModeLoading;
        }

        private void UI_ExceptionThrown(object sender, ExceptionEventArgs e)
        {
            //
        }

        private async void tabPane_SelectedPageIndexChanged(object sender, EventArgs e)
        {
            if (Device == null || !Device.IsConnected) return;

            //prevent changing modes when commands are being sent
            IsModeLoading = true;
            switch (tabPaneGasMixer.SelectedPageIndex)
            {
                case 0://maintain ports
                    await UI.GotoPortMaintenanceMode(false); break;
                case 1://concentration mode
                    await UI.GoToConcentrationMode(); break;
                case 3: //purge mode
                    await UI.GotoPurgeMode(); break;
            }
            IsModeLoading = false;
        }

        public void GoToConcentrationMode()
        {
            tabPaneGasMixer.SelectedPage = tabNavigationPageConcentration;
        }

        public bool IsModeLoading { get; private set; }

        protected override async Task<bool> connect(bool connectFast)
        {
            DisableHandlers();

            if (!connectFast)
            {
                tabPaneGasMixer.SelectedPage = tabNavigationPageMaintainPorts;
                IsModeLoading = true;

                gridViewConcentration.OptionsBehavior.Editable = false;

                bool connected = await base.connect(connectFast);
                if (!connected)
                {
                    gridViewConcentration.OptionsBehavior.Editable = true;
                    return IsModeLoading = false;
                }

                bool success = await UI.GotoPortMaintenanceMode(true);
                if (!success)
                {
                    IsModeLoading = false;
                    traceNotConnected();
                }
                gridViewConcentration.OptionsBehavior.Editable = true;
                IsModeLoading = false;
            }
            else
            {
                IsModeLoading = true;

                //copy edit settings to runtime settings
                (Device as GasMixerCommander).CopyEditToRuntimeSettings();

                ////the UI is re-initialized when the device is assigned!
                //initializeUI();


                gridViewConcentration.OptionsBehavior.Editable = false;
                bool connected = await base.connect(true);
                if (!connected)
                {
                    gridViewConcentration.OptionsBehavior.Editable = true;
                    return IsModeLoading = false;
                }
                gridViewConcentration.OptionsBehavior.Editable = true;

                //in all cases the maintain ports rows are cleared
                UI.MaintainPortsUI.MFCsUI.ClearUnsavedRows();
                UI.MaintainPortsUI.PortsUI.ClearUnsavedRows();

                //we assume that the runtime settings have been loaded externally
                switch ((Device.RuntimeSettings as GasMixerSettings).Mode)
                {
                    //case Mode.Disconnected:
                    case Mode.MaintainPorts:
                        tabPaneGasMixer.SelectedPage = tabNavigationPageMaintainPorts;
                        //(Device.RuntimeSettings as GasMixerSettings).Mode = Mode.MaintainPorts;
                        UI.MaintainPortsUI.EnableControls();
                        break;
                    case Mode.Flow:
                    case Mode.Concentration:
                        tabPaneGasMixer.SelectedPage = tabNavigationPageConcentration;
                        //(Device.RuntimeSettings as GasMixerSettings).Mode = Mode.Concentration;
                        UI.ConcentrationModeUI.GridViewUI.ClearUnsavedRows();
                        txtTargetTotalFlow.BackColor = Color.White;
                        UI.ConcentrationModeUI.EnableControls();
                        UI.ConcentrationModeUI.StartMeasurementTimer();
                        break;
                    case Mode.Purge:
                        tabPaneGasMixer.SelectedPage = tabNavigationPagePurge;
                        // (Device.RuntimeSettings as GasMixerSettings).Mode = Mode.Purge;
                        UI.PurgeModeUI.GridViewUI.ClearUnsavedRows();
                        UI.PurgeModeUI.EnableControls();
                        UI.PurgeModeUI.StartMeasurementTimer();
                        break;
                }

                IsModeLoading = false;

            }
            EnableHandlers();

            return true; //successful connection if arrived here
        }

        public override async Task<bool> Disconnect()
        {
            if (!Device.IsConnected) return true;

            if (UI.Mode == Mode.Concentration)
            {
                await UI.ConcentrationModeUI.PauseMeasurementTimer();

            }
            else if (UI.Mode == Mode.Purge)
            {
                await UI.PurgeModeUI.PauseMeasurementTimer();
            }

            //await UI.GotoPortMaintenanceMode(false);
            UI.DisableControls();

            bool isDisconnected = Device.Disconnect();

            UI.EnableControls();

            //reset the colors for all controls
            UI.MaintainPortsUI.MFCsUI.ResetUnsavedRows();
            UI.MaintainPortsUI.PortsUI.ResetUnsavedRows();
            UI.ConcentrationModeUI.GridViewUI.ResetUnsavedRows();
            UI.PurgeModeUI.GridViewUI.ResetUnsavedRows();
            txtTargetTotalFlow.BackColor = Color.LightYellow;

            return isDisconnected;
        }

        private async void uploadToGasMixerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Device.IsConnected)
                await UI.GotoPortMaintenanceMode(true);
        }



        private void DeviceChangedHandler(object sender, EventArgs e) =>
            OnDeviceChanged();


        public void EnableHandlers()
        {
            tabPaneGasMixer.SelectedPageIndexChanged += tabPane_SelectedPageIndexChanged;
            tabPaneGasMixer.SelectedPageChanging += tabPane_SelectedPageChanging;

            txtTargetTotalFlow.EditValueChanged += DeviceChangedHandler;
            UI.MaintainPortsUI.MFCsUI.GridViewChanged += DeviceChangedHandler;
            UI.MaintainPortsUI.PortsUI.GridViewChanged += DeviceChangedHandler;
            UI.ConcentrationModeUI.GridViewUI.GridViewChanged += DeviceChangedHandler;
            UI.PurgeModeUI.GridViewUI.GridViewChanged += DeviceChangedHandler;
        }
        public void DisableHandlers()
        {
            //disable tabpane handlers
            tabPaneGasMixer.SelectedPageIndexChanged -= tabPane_SelectedPageIndexChanged;
            tabPaneGasMixer.SelectedPageChanging -= tabPane_SelectedPageChanging;

            txtTargetTotalFlow.EditValueChanged -= DeviceChangedHandler;
            UI.MaintainPortsUI.MFCsUI.GridViewChanged -= DeviceChangedHandler;
            UI.MaintainPortsUI.PortsUI.GridViewChanged -= DeviceChangedHandler;
            UI.ConcentrationModeUI.GridViewUI.GridViewChanged -= DeviceChangedHandler;
            UI.PurgeModeUI.GridViewUI.GridViewChanged -= DeviceChangedHandler;
        }
    }
}
