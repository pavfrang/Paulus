using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using System.Threading.Tasks;
using System.Diagnostics;
using Paulus.Serial.UI;

namespace Paulus.Serial.InfusionPump.UI
{
    public partial class InfusionPumpControl : DeviceCommanderControl
    {
        public InfusionPumpControl()
        {
            InitializeComponent();
        }
        public InfusionPumpUI UI { get; private set; }

        protected override void initializeUI()
        {
            UI = new InfusionPumpUI(commander as InfusionPumpCommander);

            UI.InitializeUI(
                cboSyringes, cboLiquids,
                txtInitialVolume, cboInitialVolumeUnit,
                txtTargetVolume, cboTargetVolumeUnit,
                txtInfusionRate, cboInfusionRateUnits,
                lblRemainingVolume, lblRemainingVolumeUnit,
                lblDeliveredVolume, lblDeliveredVolumeUnit,
                lblTargetFlow, lblActualFlow,
                lblTargetConcentration, lblActualConcentration,
                cboGasMixers,
                btnUpdate, btnRun, btnStop,
                lblStatus,
                digitalGauge1,
                circularGauge1);

            UI.ExceptionThrown += UI_ExceptionThrown;


            txtInfusionRate.Validating += txt_Validating;
            txtTargetVolume.Validating += txt_Validating;
            txtInfusionRate.Validating += txt_Validating;

            //disable all controls (will be enabled after connection)
            //  UI.DisableControls();

        }

    

        private void UI_ExceptionThrown(object sender, Common.ExceptionEventArgs e)
        {
            // throw new NotImplementedException();
        }


        protected override async Task<bool> connect(bool connectFast)
        {
            bool connected = await base.connect(connectFast);
            if (!connected) return false;

            //upload loaded settings to syringe
            bool updated = await UI.Update();
            lblSoftwareVersion.Text = (commander.RuntimeSettings as InfusionPumpSettings).Version;

            if (!updated)
            {
                traceNotConnected();
                return false;
            }

            commander.TraceSource.TraceInformation(
                $"Successfully connected at {commander.PortName}.");

            return true; //successfully connected if arrived here
        }

        public override async Task<bool> Disconnect()
        {
            if (!Device.IsConnected) return true;

            await UI.PauseMeasurementTimer();
            UI.DisableControls();


            return Device.Disconnect();

        }

        private void txt_Validating(object sender, CancelEventArgs e)
        {
            TextEdit edit = (TextEdit)sender;
            float v;
            bool parsed = float.TryParse(edit.Text, out v);
            bool error = !(parsed && v > 0 && v <= 99999);
            if (error)
                edit.ErrorText = "Enter a value in the range (0,99999].";
        }
    }
}
