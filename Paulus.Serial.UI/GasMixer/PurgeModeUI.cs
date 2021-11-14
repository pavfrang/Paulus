using DevExpress.XtraEditors;
using DevExpress.XtraGauges.Win.Gauges.State;
using DevExpress.XtraGrid.Views.Grid;
using Paulus.Common;
using Paulus.Serial.GasMixer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Serial.UI.GasMixer
{
    public class PurgeModeUI : DeviceUIBase<GasMixerCommander, GasMixerSettings>
    {
        public PurgeModeUI ( GridView gridView,
            SimpleButton updateButton, SimpleButton stopButton, LabelControl labelFlow, StateIndicatorComponent flowGauge,
            GasMixerUI mainUI) : base(mainUI)
        {
            GridViewUI = new GridViewPurgeUI(gridView, this);


            UpdateButton = updateButton;
            UpdateButton.Click += UpdateButton_Click;
            StopButton = stopButton;
            StopButton.Click += StopButton_Click;
            LabelFlow = labelFlow;
            FlowGauge = flowGauge;

            MeasurementTimer = new global::System.Windows.Forms.Timer();
            MeasurementTimer.Interval = 1000;
            MeasurementTimer.Tick += MeasurementTimer_Tick;

        }



        public GridViewPurgeUI GridViewUI { get; }

        #region Update

        public SimpleButton UpdateButton { get; }
        private async void UpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                await pauseTimerSendCommandAndResume(GasMixerCommands.PurgeUpdate());
            }
            catch { } //timeout is possible here!
        }

        #endregion

        #region Stop

        public SimpleButton StopButton { get; }
        private async void StopButton_Click(object sender, EventArgs e) =>
             await pauseTimerSendCommandAndResume(GasMixerCommands.Stop());
        #endregion

        public LabelControl LabelFlow { get; set; }

        public StateIndicatorComponent FlowGauge { get; set; }


        private DataTable PurgeModeTable
        {
            get { return EditSettings.DataTables.PurgeModeTable; }
        }

        /// <summary>
        /// When going to concentration mode then all the current concentration values are assigned, the balance the total target flow.
        /// In the end the measurement timer is started.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GotoPurgeMode()
        {
            DisableControls();

            bool success = await GridViewUI.AssignTargetConcentrationsAfterConnection();
            if (!success)
                return false;

            EnableControls();
            StartMeasurementTimer();
            return true;
        }

        #region Measurement Timer

        public System.Windows.Forms.Timer MeasurementTimer { get; }

        //this is used in order to wait for the last command after stopping the timer
        Task<SimpleSerialCommandWithResponse<bool?>> lastLowFlowTask;

        private async void MeasurementTimer_Tick(object sender, EventArgs e)
        {
            if (!MeasurementTimer.Enabled) return; //do not begin if the timer is stopped

            //update the data because actual concentration is retrieved again
            // this.GridViewUI.GridView.RefreshData();

            try
            {
                lastLowFlowTask = DeviceCommander.ReadIsLowFlow();
                var response = await lastLowFlowTask;
                if (response.Success)
                {
                    bool isLowFlow = response.Reply.Value;
                    if (isLowFlow)
                    {
                        LabelFlow.Text = "LOW FLOW";
                        FlowGauge.StateIndex = 1; //RED
                    }
                    //else if (DeviceCommander.GasMixer.TotalActualFlow == 0.0f)
                    //{
                    //    LabelFlow.Text = "NO FLOW";
                    //    FlowGauge.StateIndex = 2; //ORANGE
                    //}
                    else //unknown flow
                    {
                        LabelFlow.Text = "NO LOW FLOW";
                        FlowGauge.StateIndex = 0;
                    }
                }
                lastLowFlowTask = null;
            }
            catch { }
        }

        public async Task PauseMeasurementTimer()
        {
            if (MeasurementTimer != null && MeasurementTimer.Enabled)
                MeasurementTimer.Stop();


            if (lastLowFlowTask != null && !lastLowFlowTask.IsCompleted)
                await lastLowFlowTask;
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

                //timeout is possible here in purgee mode!
                //if (!reply.Success)
                //    OnExceptionThrown(reply.Exception);

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

        public override void DisableControls()
        {
            StopButton.Enabled = UpdateButton.Enabled = false;
        }

        public override void EnableControls()
        {
            StopButton.Enabled = UpdateButton.Enabled = true;
        }


    }
}
