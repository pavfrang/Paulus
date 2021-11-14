using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;
using Paulus.Common;
using System.Drawing;
using System.Threading;
using Paulus.Serial.GasMixer;
using DevExpress.XtraGauges.Win.Gauges.State;

namespace Paulus.Serial.UI.GasMixer
{
    public class ConcentrationModeUI : DeviceUIBase<GasMixerCommander, GasMixerSettings>
    {
        public ConcentrationModeUI(
            GridView gridView,
            TextEdit textTargetFlow, SimpleButton applyTargetFlowButton, LabelControl labelActualFlow,
            SimpleButton updateButton, SimpleButton stopButton, LabelControl labelFlow, StateIndicatorComponent flowGauge,
            GasMixerUI mainUI) : base(mainUI)
        {
            GridViewUI = new GridViewConcentrationUI(gridView, this);

            ApplyTargetFlowButton = applyTargetFlowButton;
            ApplyTargetFlowButton.Click += ApplyTargetFlowButton_Click;
            TargetFlowTextEdit = textTargetFlow;
            TargetFlowTextEdit.TextChanged += TargetFlowTextEdit_TextChanged;
            textTargetFlow.EditValue = EditSettings.TotalTargetFlowInCcm / 1000.0f;
            TargetFlowTextEdit.KeyDown += (o, e) =>
             {
                 if (ApplyTargetFlowButton.Enabled && e.KeyCode == Keys.Enter)
                     ApplyTargetFlowButton.PerformClick();
             };

            LabelActualFlow = labelActualFlow;

            UpdateButton = updateButton;
            UpdateButton.Click += UpdateButton_Click;
            StopButton = stopButton;
            StopButton.Click += StopButton_Click;
            LabelFlow = labelFlow;
            FlowGauge = flowGauge;

            initializeMeasurementTimer();

            //MeasurementTimer = new global::System.Windows.Forms.Timer();
            //MeasurementTimer.Interval = 500; //2000;
            //MeasurementTimer.Tick += MeasurementTimer_Tick;

            //CascadeException = true;
        }

        public GridViewConcentrationUI GridViewUI { get; }

        #region Update

        public SimpleButton UpdateButton { get; }
        private async void UpdateButton_Click(object sender, EventArgs e) =>
            await pauseTimerSendCommandAndResume(GasMixerCommands.ConcentrationUpdate());

        #endregion

        #region Stop

        public SimpleButton StopButton { get; }
        private async void StopButton_Click(object sender, EventArgs e) =>
             await pauseTimerSendCommandAndResume(GasMixerCommands.Stop());
        #endregion

        #region Total target flow

        public TextEdit TargetFlowTextEdit { get; }

        public SimpleButton ApplyTargetFlowButton { get; }

        private async void ApplyTargetFlowButton_Click(object sender, EventArgs e)
        {
            bool success =
                await pauseTimerSendCommandAndResume(GasMixerCommands.AssignTotalTargetFlow(EditSettings.TotalTargetFlowInCcm));
            if (success)
            {
                GasMixerSettings rs = deviceCommander.RuntimeSettings as GasMixerSettings;
                rs.TotalTargetFlowInCcm = EditSettings.TotalTargetFlowInCcm;

                //recalculate target flow for each MFC!
                foreach (DataRow row in ConcentrationModeTable.Rows)
                //foreach (DataRow row in (GridViewPorts.GridControl.DataSource as DataTable).AsEnumerable())
                {
                    //the settings do not need to store the warnings internally
                    int mfcID = (int)row["MFC"];
                    MFC mfc = (DeviceCommander.RuntimeSettings as GasMixerSettings).MFCs[mfcID];
                    Port currentPort = mfc.CurrentPort;
                    string unit = (string)row["Unit"];
                    if (!mfc.CurrentPort.Cylinder.IsEmptyCylinder())
                    {
                        mfc.TargetFlowInCcm = rs.TotalTargetFlowInCcm * mfc.TargetConcentrationInPpm / currentPort.ConcentrationInPpm / currentPort.KFactor;
                        row["Target Flow"] = UnitConversions.ToLpm(mfc.TargetFlowInCcm);

                        float sizeRatio = mfc.SizeInCcm / rs.TotalTargetFlowInCcm;
                        float maximumConcentration = (sizeRatio > 1 ? 1 : sizeRatio) * currentPort.ConcentrationInPpm * currentPort.KFactor;
                        row["Maximum Concentration"] = unit == "%" ? UnitConversions.ToPerCent(0.9f * maximumConcentration) : 0.9f * maximumConcentration;
                        row["Minimum Concentration"] = unit == "%" ? UnitConversions.ToPerCent(0.1f * maximumConcentration) : 0.1f * maximumConcentration; ;

                        float maximumFlow = (sizeRatio > 1f ? rs.TotalTargetFlowInCcm : mfc.SizeInCcm) / 1000.0f / mfc.CurrentPort.KFactor;
                        row["Minimum Flow"] = maximumFlow * 0.1f;
                        row["Maximum Flow"] = maximumFlow * 0.9f;
                        row["Absolute Maximum Flow"] = maximumFlow; //in lpm

                    }
                }


                TargetFlowTextEdit.BackColor = Color.White;
            }
        }

        private void TargetFlowTextEdit_TextChanged(object sender, EventArgs e)
        {
            TargetFlowTextEdit.BackColor = Color.LightYellow;
            float newValue;
            if (float.TryParse(TargetFlowTextEdit.Text, out newValue))
                EditSettings.TotalTargetFlowInCcm = newValue * 1000.0f;
        }

        #endregion

        public LabelControl LabelFlow { get; set; }

        public StateIndicatorComponent FlowGauge { get; set; }

        public LabelControl LabelActualFlow { get; set; }

        private DataTable ConcentrationModeTable
        {
            get { return EditSettings.DataTables.ConcentrationModeTable; }
        }

        /// <summary>
        /// When going to concentration mode then all the current concentration values are assigned, the balance the total target flow.
        /// In the end the measurement timer is started.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GotoConcentrationMode()
        {
            bool success = await GridViewUI.AssignTargetConcentrationsAndBalanceAfterConnection();
            if (!success)
                return false;

            //then set the total flow
            var reply = await DeviceCommander.AssignTotalTargetFlow(EditSettings.TotalTargetFlowInCcm);
            if (!reply.Success)
            {
                OnExceptionThrown(reply.Exception);
                return false;
            }
            TargetFlowTextEdit.BackColor = Color.White;

            EnableControls();
            StartMeasurementTimer();
            return true;
        }

        #region Measurement Timer

        public GasMixerCommandTimer MeasurementTimer { get; private set; }

        private void initializeMeasurementTimer()
        {
            MeasurementTimer = new GasMixerCommandTimer(deviceCommander);
            MeasurementTimer.CurrentCommandCompleted += MeasurementTimer_CurrentCommandCompleted;
        }

        private void MeasurementTimer_CurrentCommandCompleted(object sender, EventArgs e)
        {
            switch (MeasurementTimer.CurrentCommandId)
            {
                case MeasurementTimerCommand.ReadFlowTotalActual:
                case MeasurementTimerCommand.ReadFlowTotalActual2:
                    Task<SimpleSerialCommandWithResponse<float?>> lastReadTotalFlowTask = (Task<SimpleSerialCommandWithResponse<float?>>)
                            MeasurementTimer.CurrentCommandTask;
                    if (lastReadTotalFlowTask.Result.Success)
                    {
                        LabelActualFlow.Text = (UnitConversions.ToLpm((DeviceCommander.RuntimeSettings as GasMixerSettings).TotalActualFlowInCcm)).ToString("0.000");

                        //changing the total flow, will also change the flow for each channel
                        foreach (DataRow row in ConcentrationModeTable.Rows)
                        //foreach (DataRow row in (GridViewPorts.GridControl.DataSource as DataTable).AsEnumerable())
                        {
                            //the settings do not need to store the warnings internally
                            int mfcID = (int)row["MFC"];
                            MFC mfc = (DeviceCommander.RuntimeSettings as GasMixerSettings).MFCs[mfcID];
                            row["Actual Flow"] = UnitConversions.ToLpm(mfc.ActualFlowInCcm);
                        }
                    }
                    break;
                case MeasurementTimerCommand.ReadActualConcentrations:
                case MeasurementTimerCommand.ReadActualConcentrations2:
                    Task<SimpleSerialCommandWithResponse<List<float>>> lastReadActualConcentrationsTask = (Task<SimpleSerialCommandWithResponse<List<float>>>)
                        MeasurementTimer.CurrentCommandTask;
                    if (lastReadActualConcentrationsTask.Result.Success)
                    {
                        foreach (DataRow row in ConcentrationModeTable.Rows)
                        {
                            //  DataRow row = ConcentrationModeTable.Rows[iRow];
                            //the settings do not need to store the warnings internally
                            int mfcID = (int)row["MFC"];
                            string unit = (string)row["Unit"];
                            MFC mfc = (DeviceCommander.RuntimeSettings as GasMixerSettings).MFCs[mfcID];
                            row["Actual Concentration"] = UnitConversions.GetPpmOrPercent(mfc.ActualConcentrationInPpm, unit);
                            row["Actual Flow"] = UnitConversions.ToLpm(mfc.ActualFlowInCcm);
                        }

                    }

                    break;
                case MeasurementTimerCommand.ReadIsLowFlow:
                    Task<SimpleSerialCommandWithResponse<bool?>> lastLowFlowTask = (Task<SimpleSerialCommandWithResponse<bool?>>)
                        MeasurementTimer.CurrentCommandTask;
                    if (lastLowFlowTask.Result.Success)
                    {
                        bool isLowFlow = lastLowFlowTask.Result.Reply.Value;
                        if (isLowFlow)
                        {
                            LabelFlow.Text = "LOW FLOW";
                            FlowGauge.StateIndex = 1; //RED
                        }
                        else if ((DeviceCommander.RuntimeSettings as GasMixerSettings).TotalActualFlowInCcm == 0.0f)
                        {
                            LabelFlow.Text = "NO FLOW";
                            FlowGauge.StateIndex = 2; //ORANGE
                        }
                        else //with flow
                        {
                            LabelFlow.Text = "NORMAL FLOW";
                            FlowGauge.StateIndex = 3;
                        }
                    }

                    break;
                case MeasurementTimerCommand.ReadWarnings:
                    Task<SimpleSerialCommandWithResponse<List<int>>> lastReadWarningsTask = (Task<SimpleSerialCommandWithResponse<List<int>>>)
                        MeasurementTimer.CurrentCommandTask;
                    if (lastReadWarningsTask.Result.Success)
                    {
                        for (int iRow = 0; iRow < ConcentrationModeTable.Rows.Count; iRow++)//foreach (DataRow row in (GridViewPorts.GridControl.DataSource as DataTable).AsEnumerable())
                        {
                            DataRow row = ConcentrationModeTable.Rows[iRow];

                            //the settings do not need to store the warnings internally
                            int mfcID = (int)row["MFC"];
                            row["Status"] = (DeviceCommander.RuntimeSettings as GasMixerSettings).MFCs[mfcID].Warning;
                        }

                    }
                    break;
            }
        }

        public async Task PauseMeasurementTimer()
        {
            await MeasurementTimer.Pause();
        }

        public void StartMeasurementTimer() =>
            MeasurementTimer.Start();

        private async Task<bool> pauseTimerSendCommandAndResume(SimpleSerialCommand command)
        {
            if (MeasurementTimer.Enabled)
            {
                DisableControls();

                await PauseMeasurementTimer();

                var reply = await command.SendAndGetCommand(deviceCommander);
                EnableControls();
                if (!reply.Success)
                    OnExceptionThrown(reply.Exception);
                //the measurement must be continued to track mfc warnings
                StartMeasurementTimer();
                if (reply.IsError) return false;

            }
            return true;
        }

        #endregion

        protected override async void Agent_Disconnecting(object sender, EventArgs e)
        //protected override void Agent_Disconnecting(object sender, EventArgs e)
        {
            if (MeasurementTimer.Enabled)
            {
                await PauseMeasurementTimer();
                //PauseMeasurementTimer();
                DisableControls();
            }
        }

        protected override void Agent_Disconnected(object sender, EventArgs e)
        {
        }

        protected override void Agent_Connected(object sender, EventArgs e)
        { }

        public override void DisableControls()
        {
            ApplyTargetFlowButton.Enabled = StopButton.Enabled = UpdateButton.Enabled = false;
        }

        public override void EnableControls()
        {
            ApplyTargetFlowButton.Enabled = StopButton.Enabled = UpdateButton.Enabled = true;
        }


    }
}
