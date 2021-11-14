using DevExpress.XtraEditors;
using DevExpress.XtraGauges.Core.Drawing;
using DevExpress.XtraGauges.Core.Model;
using DevExpress.XtraGauges.Win;
using DevExpress.XtraGauges.Win.Gauges.Circular;
using DevExpress.XtraGauges.Win.Gauges.Digital;
using Paulus.Common;
using Paulus.Serial.GasMixer;
using Paulus.Serial.InfusionPump;
using Paulus.Serial.UI;
using Paulus.Serial.UI.InfusionPump;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paulus.Serial.InfusionPump.UI
{
    public sealed class InfusionPumpUI : DeviceUIBase<InfusionPumpCommander, InfusionPumpSettings>
    {
        public InfusionPumpUI(InfusionPumpCommander commander) :
            base(commander) //this will update the EditSettings internally
        { }

        public void InitializeUI(LookUpEdit cboSyringes, LookUpEdit cboLiquids,
            TextEdit txtInitialVolume, ComboBoxEdit cboInitialVolumeUnits,
            TextEdit txtTargetVolume, ComboBoxEdit cboTargetVolumeUnits,
            TextEdit txtInfusionRate, ComboBoxEdit cboInfusionRateUnits,
            LabelControl lblRemainingVolume, LabelControl lblRemainingVolumeUnit,
            LabelControl lblDeliveredVolume, LabelControl lblDeliveredVolumeUnit,

            LabelControl lblTargetFlow, LabelControl lblActualFlow,
            LabelControl lblTargetConcentration, LabelControl lblActualConcentration,
            CheckedComboBoxEdit cboGasMixers,

            SimpleButton btnUpdate, SimpleButton btnStart, SimpleButton btnStop,
            LabelControl lblStatus,
            //LabelControl lblError,
            DigitalGauge digitalControl,
            CircularGauge gaugeControl
            )
        {
            ComboSyringes = cboSyringes;
            ComboLiquids = cboLiquids;

            TextInitialVolume = txtInitialVolume;
            ComboInitialVolumeUnit = cboInitialVolumeUnits;

            TextTargetVolume = txtTargetVolume;
            ComboTargetVolumeUnit = cboTargetVolumeUnits;

            TextInfusionRate = txtInfusionRate;
            ComboInfusionRateUnits = cboInfusionRateUnits;

            LabelRemainingVolume = lblRemainingVolume;
            LabelRemainingVolumeUnit = lblRemainingVolumeUnit;

            LabelDeliveredVolume = lblDeliveredVolume;
            LabelDeliveredVolumeUnit = lblDeliveredVolumeUnit;
            // ComboDeliveredVolumeUnits = lblDeliveredVolumeUnit;

            LabelTargetFlow = lblTargetFlow;
            LabelActualFlow = lblActualFlow;
            LabelTargetConcentration = lblTargetConcentration;
            LabelActualConcentration = lblActualConcentration;

            ComboBoxGasMixers = cboGasMixers;

            ButtonUpdate = btnUpdate;
            ButtonStart = btnStart;
            ButtonStop = btnStop;

            LabelStatus = lblStatus;
            //LabelError = lblError;
            GaugeControl = gaugeControl;
            DigitalControl = digitalControl;
            if (digitalControl != null)
                DigitalControl.Text = "00:00:00";

            addHandlers();

            loadValuesFromSettings();

            DisableControls();

            initializeMeasurementTimer();

        }


        private void addHandlers()
        {
            InfusionPumpControls.AttachSyringesControls(ComboSyringes, EditSettings);
            InfusionPumpControls.AttachInitialVolumeControls(TextInitialVolume, ComboInitialVolumeUnit, EditSettings);
            InfusionPumpControls.AttachTargetVolumeControls(TextTargetVolume, ComboTargetVolumeUnit, EditSettings);
            InfusionPumpControls.AttachInfusionRateControls(TextInfusionRate, ComboInfusionRateUnits, EditSettings);

            ComboLiquids.EditValueChanged += (o, e) =>
               RuntimeSettings.Liquid = EditSettings.Liquid = (SyringeLiquid)ComboLiquids.EditValue;
            // TextInitialVolume.EditValueChanged += setToUnsavedStateHandler;

            ButtonUpdate.Click += async (o, e) =>
            {
                try
                {
                    await Update();
                }
                catch { }


            };
            ButtonStart.Click += async (o, e) =>
            {
                try
                {
                    await deviceCommander.Start();
                    //LabelStatus.Text = "Infusing";
                }
                catch { }
                StartMeasurementTimer();
            };


            ButtonStop.Click += async (o, e) =>
            {
                try
                {
                    await deviceCommander.Stop();

                    //LabelStatus.Text = "Stopped";
                }
                catch { }
            };

        }

        private void setToUnsavedStateHandler(object sender, EventArgs e) =>
            (sender as Control).BackColor = Color.LightYellow;

        private void loadValuesFromSettings()
        {
            InfusionPumpControls.FillLiquids(ComboLiquids, EditSettings);
            //TextInitialVolume.EditValue = EditSettings.InitialVolume;
            //ComboInitialVolumeUnit.EditValue = EditSettings.InitialVolumeUnit != "μl" ?
            //    EditSettings.InitialVolumeUnit : "ul";

            //LabelRemainingVolumeUnits.EditValue = EditSettings.RemainingVolumeUnit != "μl" ?
            //    EditSettings.RemainingVolumeUnit : "ul";

            //ComboDeliveredVolumeUnits.EditValue = EditSettings.DeliveredVolumeUnit != "μl" ?
            //    EditSettings.DeliveredVolumeUnit : "ul";

            LabelRemainingVolume.Text = EditSettings.RemainingVolume.ToString("###0.0##");
            LabelDeliveredVolume.Text = EditSettings.DeliveredVolume.ToString("###0.0##");
            LabelActualConcentration.Text = LabelActualFlow.Text =
                LabelTargetConcentration.Text = LabelActualConcentration.Text = "0.0";

            ComboBoxGasMixers.Properties.Items.Clear();
            //add all the gas mixers from the parent device manager
            List<GasMixerCommander> allGasMixers =
                this.deviceCommander.DeviceManager.DeviceCommanders.Values.OfType<GasMixerCommander>().ToList();
            foreach (var commander in allGasMixers)
                ComboBoxGasMixers.Properties.Items.Add(commander, true);
            ComboBoxGasMixers.CustomDisplayText += ComboBoxGasMixers_CustomDisplayText;

            ComboBoxGasMixers.Refresh(); //redraw now

            updateGaugeControl();
        }

        private void ComboBoxGasMixers_CustomDisplayText(object sender, DevExpress.XtraEditors.Controls.CustomDisplayTextEventArgs e)
        {
            if (string.IsNullOrWhiteSpace((string)e.Value)) e.DisplayText = "<None>";
            else if (ComboBoxGasMixers.Properties.Items.GetCheckedValues().Count == 1)//if (e. is GasMixerCommander)
                e.DisplayText = ((string)e.Value).RemoveTextInParentheses();
            else
                e.DisplayText = "Combined";
        }

        #region Controls
        public LookUpEdit ComboSyringes { get; private set; }

        public LookUpEdit ComboLiquids { get; private set; }

        public TextEdit TextInitialVolume { get; private set; }

        public ComboBoxEdit ComboInitialVolumeUnit { get; private set; }

        public TextEdit TextTargetVolume { get; private set; }

        public ComboBoxEdit ComboTargetVolumeUnit { get; private set; }

        public TextEdit TextInfusionRate { get; private set; }
        public ComboBoxEdit ComboInfusionRateUnits { get; private set; }

        public LabelControl LabelRemainingVolume { get; private set; }
        public LabelControl LabelRemainingVolumeUnit { get; private set; }

        public LabelControl LabelDeliveredVolume { get; private set; }

        //public ComboBoxEdit ComboRemainingVolumeUnits { get; private set; }
        public LabelControl LabelDeliveredVolumeUnit { get; private set; }

        // public ComboBoxEdit ComboDeliveredVolumeUnits { get; private set; }

        public CheckedComboBoxEdit ComboBoxGasMixers { get; private set; }

        public LabelControl LabelTargetFlow { get; private set; }

        public LabelControl LabelActualFlow { get; private set; }

        public LabelControl LabelTargetConcentration { get; private set; }
        public LabelControl LabelActualConcentration { get; private set; }

        public SimpleButton ButtonUpdate { get; private set; }
        public SimpleButton ButtonStart { get; private set; }
        public SimpleButton ButtonStop { get; private set; }

        public LabelControl LabelStatus { get; private set; }
        public LabelControl LabelError { get; private set; }

        public CircularGauge GaugeControl { get; private set; }
        public DigitalGauge DigitalControl { get; private set; }
        #endregion


        //enabling and disabling controls if the device is disconnected, or during updates
        public override void DisableControls()
        {
            ButtonUpdate.Enabled = ButtonStart.Enabled = ButtonStop.Enabled = false;
        }

        public override void EnableControls()
        {
            ButtonUpdate.Enabled = ButtonStart.Enabled = ButtonStop.Enabled = true;
        }

        public async Task<bool> Update()
        {
            DisableControls();

            RuntimeSettings.InitialVolume = EditSettings.InitialVolume;
            RuntimeSettings.InitialVolumeUnit = EditSettings.InitialVolumeUnit;
            TextInitialVolume.BackColor = Color.White;

            bool wasRunning = MeasurementTimer?.Enabled ?? false;
            if (wasRunning) await PauseMeasurementTimer();

            //set syringe
            if (EditSettings.Syringe != RuntimeSettings.Syringe)
            {
                var reply = await deviceCommander.SetSyringe(EditSettings.Syringe);

                if (reply.Success)
                    ComboSyringes.BackColor = Color.White;
                else
                {
                    OnExceptionThrown(reply.Exception);
                    EnableControls();
                    if (wasRunning)
                        StartMeasurementTimer();

                    //controls should be re-enabled!
                    return false;
                }
            }

            //set target volume
            if (EditSettings.GetTargetVolumeInMicroLiters() != RuntimeSettings.GetTargetVolumeInMicroLiters())
            {
                if (EditSettings.TargetVolumeUnit == "μl") EditSettings.TargetVolumeUnit = "ul";

                var reply = await DeviceCommander.SetTargetVolume(EditSettings.TargetVolume, EditSettings.TargetVolumeUnit);
                if (reply.Success)
                {
                    TextTargetVolume.BackColor = Color.White;
                    updateGaugeControl();
                }
                else
                {
                    OnExceptionThrown(reply.Exception);
                    EnableControls();
                    if (wasRunning)
                        StartMeasurementTimer();
                    //controls should be re-enabled!
                    return false;
                }
            }

            //set infusion rate
            if (EditSettings.GetInfusionRateInMicrolitersPerMinute() != RuntimeSettings.GetInfusionRateInMicrolitersPerMinute())
            {
                var reply = await DeviceCommander.SetInfusionRate(EditSettings.InfusionRate, EditSettings.GetFriendlyInfusionRateUnit());
                if (reply.Success)
                    TextInfusionRate.BackColor = Color.White;
                else
                {
                    OnExceptionThrown(reply.Exception);
                    EnableControls();
                    if (wasRunning)
                        StartMeasurementTimer();
                    //controls should be re-enabled!
                    return false;
                }

            }

            //if(EditSettings)
            EnableControls();

            if (wasRunning)
                StartMeasurementTimer();

            return true;
        }

        private void updateGaugeControl()
        {
            ArcScaleComponent scale = GaugeControl.Scales[0];
            scale.MinValue = 0;
            scale.MaxValue = EditSettings.InitialVolume;

            scale.Ranges.Clear();

            ArcScaleRange range1 = new ArcScaleRange();
            range1.StartValue = 0.2f * EditSettings.InitialVolume;
            range1.EndValue = EditSettings.InitialVolume;
            range1.AppearanceRange.ContentBrush = new DevExpress.XtraGauges.Core.Drawing.SolidBrushObject("Color:#9EC968");
            range1.EndThickness = 14F;
            //   range1.Name = "Range0";
            range1.ShapeOffset = 0F;
            range1.StartThickness = 14F;
            scale.Ranges.Add(range1);

            ArcScaleRange range2 = new ArcScaleRange();
            range2.StartValue = 0.1f * EditSettings.InitialVolume;
            range2.EndValue = 0.2f * EditSettings.InitialVolume;
            range2.AppearanceRange.ContentBrush = new DevExpress.XtraGauges.Core.Drawing.SolidBrushObject("Color:#FED96D");
            range2.EndThickness = 14F;
            range2.ShapeOffset = 0F;
            range2.StartThickness = 14F;
            scale.Ranges.Add(range2);


            ArcScaleRange range3 = new ArcScaleRange();
            range3.StartValue = 0;
            range3.EndValue = 0.1f * EditSettings.InitialVolume;
            range3.AppearanceRange.ContentBrush = new DevExpress.XtraGauges.Core.Drawing.SolidBrushObject("Color:#EF8C75");
            range3.EndThickness = 14F;
            range3.ShapeOffset = 0F;
            range3.StartThickness = 14F;
            scale.Ranges.Add(range3);
            scale.Value = EditSettings.InitialVolume - RuntimeSettings.DeliveredVolume;

            if (EditSettings.InitialVolume <= 10)
                scale.MajorTickmark.FormatString = "{0:0.0}";
            else
                scale.MajorTickmark.FormatString = "{0:0}";

        }

        #region Timer

        public InfusionPumpCommandTimer MeasurementTimer { get; private set; }

        private void initializeMeasurementTimer()
        {
            MeasurementTimer = new InfusionPumpCommandTimer(deviceCommander);
            MeasurementTimer.CurrentCommandCompleted += MeasurementTimer_CurrentCommandCompleted;
        }

        private async void MeasurementTimer_CurrentCommandCompleted(object sender, EventArgs e)
        {
            //Task<SimpleSerialCommandWithResponse<Tuple<float?, string, Status?>>> lastReadDeliveredVolume =
            //   (Task<SimpleSerialCommandWithResponse<Tuple<float?, string, Status?>>>)MeasurementTimer.CurrentCommandTask;

            var scale = GaugeControl.Scales[0];
            scale.Value = RuntimeSettings.RemainingVolumeFromInitial = RuntimeSettings.InitialVolume - RuntimeSettings.DeliveredVolume;

            LabelStatus.Text = RuntimeSettings.Status.Value.GetDescription();

            LabelDeliveredVolume.Text = RuntimeSettings.DeliveredVolume.ToString("0.0#");
            LabelDeliveredVolumeUnit.Text = LabelRemainingVolumeUnit.Text =
                RuntimeSettings.RemainingVolumeUnit = RuntimeSettings.DeliveredVolumeUnit == "ul" ? "μl" : "ml";

            LabelRemainingVolume.Text = RuntimeSettings.RemainingVolume.ToString("0.0#");

            //check the time threshold and assign the color!

            //remaining time
            float remainingul = RuntimeSettings.GetInitialVolumeInMicroLiters() - RuntimeSettings.GetDeliveredVolumeInMicroLiters();

            if(remainingul<0)
            {
                remainingul = 0;
            }

            //RuntimeSettings.GetTargetVolumeInMicroLiters() - RuntimeSettings.GetDeliveredVolumeInMicroLiters();

            float speedInulPerMin = RuntimeSettings.GetInfusionRateInMicrolitersPerMinute();
            if (speedInulPerMin == 0) return;

            float remainingMinutes = remainingul / speedInulPerMin;
            TimeSpan span = new TimeSpan((long)(TimeSpan.TicksPerMinute * remainingMinutes));
            if (DigitalControl != null)
            {
                DigitalControl.Text = $"{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";
                if (remainingMinutes < 5.0f) DigitalControl.AppearanceOn.ContentBrush = new SolidBrushObject(Color.Red);
                else DigitalControl.AppearanceOn.ContentBrush = new SolidBrushObject("Color:#7184BA");
            }

            var gasMixers = ComboBoxGasMixers.Properties.Items.GetCheckedValues().Cast<GasMixerCommander>();
            if (gasMixers.Any())
            {
                float totalActualFlow = gasMixers.Sum(g => (g.RuntimeSettings as GasMixerSettings).TotalActualFlowInCcm) / 1000;//ccm->lpm
                LabelActualFlow.Text = totalActualFlow.ToString("0.0##");

                float totalTargetFlow = gasMixers.Sum(g => (g.RuntimeSettings as GasMixerSettings).TotalTargetFlowInCcm) / 1000;//ccm->lpm
                LabelTargetFlow.Text = totalTargetFlow.ToString("0.0##");

                float targetConcentration = RuntimeSettings.GetConcentrationInPpm(totalTargetFlow);
                LabelTargetConcentration.Text = targetConcentration.ToString("0.0"); //ppm

                float actualConcentration = RuntimeSettings.GetConcentrationInPpm(totalActualFlow);
                LabelActualConcentration.Text = actualConcentration.ToString("0.0"); //ppm
            }
            else
            {
                LabelActualFlow.Text = LabelTargetFlow.Text = LabelTargetConcentration.Text = LabelActualConcentration.Text = "-";
            }

            if (remainingul < 0.0)
            {
                await DeviceCommander.Stop();
                // TextInitialVolume.EditValue = Convert.ToSingle(TextInitialVolume.EditValue) - RuntimeSettings.TargetVolume;

                //    MeasurementTimer.Stop();
                //LabelStatus.Text = "Stopped";
            }

        }


        public void StartMeasurementTimer() =>
            MeasurementTimer.Start();


        public async Task PauseMeasurementTimer()
        {
            if (MeasurementTimer.Enabled)
                await MeasurementTimer.Pause();
        }

        #endregion
    }

}
