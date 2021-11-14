using Paulus.Serial.Fluke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Paulus.Serial.Bosch
{
    public enum SensorStatus
    {
        Undefined, //POWER (?)
        WaitForMeasurement, //POWER
        WaitForOtherSensorMeasurement, //WAIT
        Measuring, //MEASURE (inside measurement)
        Regenerating //REGEN
    }

    //TODO resistance threshold as a float
    public struct AutomationSettings
    {
        public int FlukeMeasureIntervalInSeconds, FlukeMeasureDurationInSeconds,
            ResistanceThresholdInMOhm, RegenerationDurationInSeconds;
    }

    public class BoschSensor : ISensorWithResistance
    {
        public BoschSensor(string name, Fluke.Fluke fluke)
        {
            initializeAutomationTimer();
            Name = name;

            Fluke = fluke;
        }

        public string Name { get; set; }

        public Fluke.Fluke Fluke { get; private set; }

        public override string ToString()
        {
            return string.Format($"{Name} ({CurrentStatus})");
        }

        public AutomationSettings AutomationSettings;
        public DateTime CurrentStepStartTime { get; private set; }
        public DateTime NextStepStartTime { get; private set; }
        public double DurationUntilNextStep { get; private set; }

        public SensorStatus CurrentStatus { get; private set; }

        public double Resistance { get; set; }

        private bool _autoRegen;
        public bool AutoRegen
        {
            get { return _autoRegen; }
            set
            {
                _autoRegen = value;

                if (_autoRegen && isAutomationRunning) regenerate();

            }
        }

        #region Automation commands

        Timer timerAutomation;

        private void initializeAutomationTimer()
        {
            Random rnd = new Random();
            //the timer is needed in order to report intermediate times 
            timerAutomation = new Timer();
            timerAutomation.Interval = 200;
            timerAutomation.Tick += timerAutomation_Elapsed;
        }

        bool isAutomationRunning = false;
        public bool IsAutomationRunning { get { return isAutomationRunning; } }

        public void StartAutomation()
        {
            isAutomationRunning = true;
            CurrentStatus = SensorStatus.WaitForMeasurement;
            CurrentStepStartTime = DateTime.Now;
            OnStatusChanged();

            if (timerAutomation.Enabled)
                throw new InvalidOperationException("The automation is still running. You should stop the automation first.");

            timerAutomation.Start();
        }

        public void StopAutomation()
        {
             timerAutomation.Stop();
            isAutomationRunning = false;

           //unlock the exclusive use of the fluke which is used for measurement
            if (Fluke.CurrentSensor == this)
                Fluke.UnlockForExclusiveUse();

            CurrentStatus = SensorStatus.Undefined;
            OnStatusChanged();
        }

        private void waitForOtherSensor()
        {
            CurrentStatus = SensorStatus.WaitForOtherSensorMeasurement;
            CurrentStepStartTime = DateTime.Now;
            OnStatusChanged();
        }

        private void measure()
        {
            CurrentStatus = SensorStatus.Measuring;
            CurrentStepStartTime = DateTime.Now;

            //Fluke.AddToQueueForMeasurement(this);
            Fluke.StartStoringNormalValues();

            OnStatusChanged();
        }
        void ISensorWithResistance.measure()
        {
            measure();
        }

        private void waitForMeasurement()
        {
            CurrentStatus = SensorStatus.WaitForMeasurement;
            CurrentStepStartTime = DateTime.Now;

            OnStatusChanged();
        }

        private void regenerate()
        {
            CurrentStatus = SensorStatus.Regenerating;
            CurrentStepStartTime = DateTime.Now;

            OnStatusChanged();
        }

        public event EventHandler StatusChanged;
        private void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void timerAutomation_Elapsed(object sender, EventArgs e)
        {
            calculateDurationUntilNextStep();

            if (DurationUntilNextStep > 0)
                OnDurationUntilNextStepChanged();
            else
            {
                //if measuring status is finished than grab the last valid fluke value
                if (CurrentStatus == SensorStatus.Measuring)
                {
                    float lastNonOverloadValue = Fluke.StopStoringNormalValues();
                    Resistance = lastNonOverloadValue == 0.0f ? Fluke.Resistance.ActualValue : lastNonOverloadValue;

                    Fluke.UnlockForExclusiveUse();
                }

                // Debug.WriteLine($"Proceeding to next step {ToString()}...");
                OnStatusChanging();

                proceedToNextStep();
                // Debug.WriteLine($"Set to step {ToString()}...");
            }
        }
        public event EventHandler StatusChanging;
        private void OnStatusChanging()
        {
            StatusChanging?.Invoke(this, EventArgs.Empty);
        }


        public event EventHandler DurationUntilNextStepChanged;
        private void OnDurationUntilNextStepChanged()
        {
            DurationUntilNextStepChanged?.Invoke(this, EventArgs.Empty);
        }

        private void calculateDurationUntilNextStep()
        {
            switch (CurrentStatus)
            {
                case SensorStatus.WaitForMeasurement:
                    NextStepStartTime = CurrentStepStartTime.AddSeconds(AutomationSettings.FlukeMeasureIntervalInSeconds);
                    break;
                case SensorStatus.WaitForOtherSensorMeasurement: //the other sensor status should be SensorStatus.MeasureSensor
                    //wait for 500 ms after the other sensor measurement
                    //  _otherSensorWithSameMeasurementSystem.NextStepStartTime.AddSeconds(0.5);
                    NextStepStartTime = Fluke.NextAvailableMeasureTime;
                    break;
                case SensorStatus.Measuring:
                    NextStepStartTime = CurrentStepStartTime.AddSeconds(AutomationSettings.FlukeMeasureDurationInSeconds);
                    break;
                case SensorStatus.Regenerating:
                    NextStepStartTime = CurrentStepStartTime.AddSeconds(AutomationSettings.RegenerationDurationInSeconds);
                    break;
            }

            DurationUntilNextStep = (NextStepStartTime - DateTime.Now).TotalSeconds;
        }

        private void proceedToNextStep()
        {
            switch (CurrentStatus)
            {
                //get current event
                case SensorStatus.WaitForMeasurement:
                case SensorStatus.WaitForOtherSensorMeasurement: //the other sensor status should be SensorStatus.MeasureSensor
                    if (Fluke.TryLockForExclusiveUse(this))
                        measure();
                    else
                        waitForOtherSensor();
                    break;
                case SensorStatus.Measuring:
                    if (Resistance < AutomationSettings.ResistanceThresholdInMOhm * 1e6 && this.AutoRegen)
                        regenerate();
                    else
                        waitForMeasurement();
                    break;
                case SensorStatus.Regenerating:
                    waitForMeasurement();
                    break;
            }
        }

        public void ForceRegen()
        {
            regenerate();
        }

        public void ForceMeasure()
        {
            measure();
        }




        #endregion
    }


}
