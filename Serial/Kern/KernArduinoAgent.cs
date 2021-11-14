using Paulus.Common;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Paulus.Serial.Kern
{
    public class AutomationSettings
    {
        public int Scale2OffsetTimeDelayInSeconds { get; set; } //30*60 sec
        public int Scale2SettleTimeInSeconds { get; set; } //30 sec

        public int Scale1MovingPeriodTimeInSeconds { get; set; } //500 sec
        public int Scale2MovingPeriodTimeInSeconds { get; set; } //100 sec
        public int Scale1SteadyTimeInSeconds { get; set; }
        public float Scale1SteadyLimitInGrams { get; set; }

        public int LegislationSettleTimeInSeconds { get; set; }
        public float SettleLimitInGrams { get; set; } //2 gr
        public int PurgeTimeInSeconds { get; set; }
        public int SettleTimeAfterPurgeInSeconds { get; set; }
        public int LoopDelayInSeconds { get; set; }
    }

    public enum AutomationStep
    {
        Stopped,
        Started,
        WaitingForScale2Measurement,
        SettlingScale2BeforeMeasurement,
        WaitingLegislationSettleTime,
        Purging,
        WaitingForSettlingAfterPurge,
        LoopingDelay
    }



    public class KernArduinoAgent : SerialDevice
    {
        public KernArduinoAgent(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
            : this(new SerialPort(portName, baudRate, parity, dataBits, stopBits)) { }

        public KernArduinoAgent(SerialPort port)
            : base(port,ResponseMode.Monitor)
        {
            ReceiveMessageSuffix = "\r\n";
            //SendMessageSuffix = "\n";
        }

        public KernScaleAgent Scale1Agent, Scale2Agent;

        #region Send commands

        public bool SendUnderweight()
        {
            return SendMessage("U");
        }

        public bool SendMeasure()
        {
            return SendMessage("M");
        }

        public bool SendOverweight()
        {
            return SendMessage("O");
        }

        public bool SendClean()
        {
            return SendMessage("CL");
        }

        public bool SendClose()
        {
            return SendMessage("C");
        }

        public bool SendCheckVanes()
        {
            return SendMessage("C");
        }

        #endregion

        public AutomationSettings AutomationSettings { get; } = new AutomationSettings();

        private Timer tmrAutomation;

        private AutomationStep _currentAutomationStep;
        public AutomationStep CurrentAutomationStep
        {
            get { return _currentAutomationStep; }
            set
            {
                _currentAutomationStep = value;
                OnAutomationStepChanged();
            }

        }

        public bool StartAutomation()
        {
            tmrAutomation = new Timer();
            tmrAutomation.Interval = 250;

            tmrAutomation.Tick += tmrAutomation_Tick;

            lastScale1Values = new Queue<float>();
            lastScale2Values = new Queue<float>();
            lastScale1MovingAverageValues = new Queue<float>();

            OnAutomationStarted();

            tmrAutomation.Start();
            return true;
        }

        public bool IsAutomationRunning { get { return tmrAutomation.Enabled; } }

        public event EventHandler AutomationStarted;
        protected void OnAutomationStarted()
        {
            nextAutomationStepTime = DateTime.Now;
            CurrentAutomationStep = AutomationStep.Started;

            AutomationStarted?.Invoke(this, EventArgs.Empty);
        }

        //public event EventHandler AutomationStepChanging;
        //protected void OnAutomationStepChanging()
        //{
        //    AutomationStepChanging?.Invoke(this, EventArgs.Empty);
        //}

        public event EventHandler AutomationStepChanged;

        protected void OnAutomationStepChanged()
        {
            AutomationStepChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<TimeSpanEventArgs> WaitingForNextStep;
        protected void OnWaitingForNextStep(TimeSpanEventArgs e)
        {
            WaitingForNextStep?.Invoke(this, e);
        }


        DateTime nextAutomationStepTime = DateTime.Now;
        public float Scale1Offset { get; private set; }
        public float Scale2Offset { get; private set; }
        public float Scale1Loaded { get; private set; }
        public float Scale2Loaded { get; private set; }

        public float Scale1MovingAverage { get; private set; }
        public float Scale2MovingAverage { get; private set; }

        //the values which are used for the calculation of the moving averages
        private Queue<float> lastScale1Values;

        //the values which are used for the calculation of the moving averages
        private Queue<float> lastScale2Values;

        private Queue<float> lastScale1MovingAverageValues;


        DateTime? startTimeOfSettlingScale2;
        private void tmrAutomation_Tick(object sender, EventArgs e)
        {
            #region Calculate moving averages
            //fill the last values 
            if (lastScale1Values.Count == AutomationSettings.Scale1MovingPeriodTimeInSeconds)
                lastScale1Values.Dequeue();
            lastScale1Values.Enqueue(Scale1Agent.KernScale.Weight.ActualValue);

            if (lastScale2Values.Count == AutomationSettings.Scale2MovingPeriodTimeInSeconds)
                lastScale2Values.Dequeue();
            lastScale2Values.Enqueue(Scale2Agent.KernScale.Weight.ActualValue);

            Scale1MovingAverage = lastScale1Values.Average();
            Scale2MovingAverage = lastScale2Values.Average();

            //store the moving average values in order to check for minimum/maximum value each time
            if (lastScale1MovingAverageValues.Count == AutomationSettings.Scale1SteadyTimeInSeconds)
                lastScale1MovingAverageValues.Dequeue();
            lastScale1MovingAverageValues.Enqueue(Scale1MovingAverage);
            #endregion


            //check if the next automation step should be here
            //the scale 2 offset should continuously update during first automation steps
            if (CurrentAutomationStep == AutomationStep.SettlingScale2BeforeMeasurement ||
                CurrentAutomationStep == AutomationStep.WaitingForScale2Measurement)
                //Scale2Offset = Math.Min(Scale2Offset, Scale2Agent.KernScale.Weight.ActualValue);
                Scale2Offset = Math.Min(Scale2Offset, Scale2MovingAverage);

            bool shouldChangeStep; // = false;
            if (CurrentAutomationStep != AutomationStep.SettlingScale2BeforeMeasurement)
                shouldChangeStep = DateTime.Now >= nextAutomationStepTime;
            else //if (CurrentAutomationStep == AutomationStep.SettlingScale2BeforeMeasurement)
            {
                //get the scale1 minimum/maximum average for the last  AutomationSettings.Scale1SteadeTimeInSeconds
                float Scale1MinimumAverage = lastScale1MovingAverageValues.Min();
                float Scale1MaximumAverage = lastScale1MovingAverageValues.Max();

                //float scale2 = Scale2Agent.KernScale.Weight.ActualValue;
                //if (scale2 < AutomationSettings.SettleLimitInGrams + Scale2Offset)
                if (Scale2MovingAverage < AutomationSettings.SettleLimitInGrams + Scale2Offset ||
                    (Scale1MaximumAverage - Scale1MinimumAverage) >= AutomationSettings.Scale1SteadyLimitInGrams)
                {
                    startTimeOfSettlingScale2 = null; //just wait for the settle condition
                    shouldChangeStep = false;
                }
                else if (startTimeOfSettlingScale2 == null) //starting measuring the time!
                {
                    startTimeOfSettlingScale2 = DateTime.Now; //set the time for the first time
                    shouldChangeStep = false;
                    nextAutomationStepTime = DateTime.Now.AddSeconds(AutomationSettings.Scale2SettleTimeInSeconds);
                }
                else
                {
                    shouldChangeStep = (DateTime.Now - startTimeOfSettlingScale2.Value).TotalSeconds >= AutomationSettings.Scale2SettleTimeInSeconds;
                }
            }

            if (!shouldChangeStep)
            {
                //the time span has meaning except for the case:
                //AutomationStep.SettlingScale2BeforeMeasurement

                if (CurrentAutomationStep != AutomationStep.SettlingScale2BeforeMeasurement)
                    OnWaitingForNextStep(new TimeSpanEventArgs(nextAutomationStepTime - DateTime.Now));
                else if (startTimeOfSettlingScale2 != null)
                    OnWaitingForNextStep(new TimeSpanEventArgs(nextAutomationStepTime - DateTime.Now));
                else
                    OnWaitingForNextStep(new TimeSpanEventArgs(new TimeSpan(0)));

                return;
            }


            //we come here if we should change the step!

            switch (CurrentAutomationStep)
            {
                case AutomationStep.Started:
                    SendUnderweight();
                    //initialize the offset at the beginning of the automation loop
                    Scale2Offset = Scale2Agent.KernScale.Weight.ActualValue;
                    if (AutomationSettings.Scale2OffsetTimeDelayInSeconds > 0)
                    {
                        nextAutomationStepTime = DateTime.Now.AddSeconds(AutomationSettings.Scale2OffsetTimeDelayInSeconds);
                        CurrentAutomationStep = AutomationStep.WaitingForScale2Measurement;
                    }
                    else
                        CurrentAutomationStep = AutomationStep.SettlingScale2BeforeMeasurement;
                    break;
                case AutomationStep.SettlingScale2BeforeMeasurement:
                    SendMeasure();
                    nextAutomationStepTime = DateTime.Now.AddSeconds(AutomationSettings.LegislationSettleTimeInSeconds);
                    CurrentAutomationStep = AutomationStep.WaitingLegislationSettleTime;
                    break;
                case AutomationStep.WaitingLegislationSettleTime:
                    Scale1Loaded = Scale1Agent.KernScale.Weight.ActualValue;
                    Scale2Loaded = Scale2Agent.KernScale.Weight.ActualValue;
                    SendOverweight();
                    nextAutomationStepTime = DateTime.Now.AddSeconds(AutomationSettings.PurgeTimeInSeconds);
                    CurrentAutomationStep = AutomationStep.Purging;
                    break;
                case AutomationStep.Purging:
                    SendMeasure();
                    nextAutomationStepTime = DateTime.Now.AddSeconds(AutomationSettings.SettleTimeAfterPurgeInSeconds);
                    CurrentAutomationStep = AutomationStep.WaitingForSettlingAfterPurge;
                    break;
                case AutomationStep.WaitingForSettlingAfterPurge:
                    Scale1Offset = Scale1Agent.KernScale.Weight.ActualValue;
                    nextAutomationStepTime = DateTime.Now.AddSeconds(AutomationSettings.LoopDelayInSeconds);
                    CurrentAutomationStep = AutomationStep.LoopingDelay;
                    break;
                case AutomationStep.LoopingDelay:
                    nextAutomationStepTime = DateTime.Now;
                    CurrentAutomationStep = AutomationStep.Started;
                    break;
            }

        }

        #region Stopping Automation

        public bool StopAutomationAndClose()
        {
            tmrAutomation.Stop();

            bool sent = SendClose();
            if (sent)
                OnAutomationStopped();
            return sent;
        }


        public event EventHandler AutomationStopped;
        protected void OnAutomationStopped()
        {
            CurrentAutomationStep = AutomationStep.Stopped;

            AutomationStopped?.Invoke(this, EventArgs.Empty);
        }

        #region Stop automation, clean and close

        public bool StopAutomationCleanAndClose()
        {
            tmrAutomation.Stop();
            OnAutomationStopped();

            bool sent = SendClean();
            if (!sent) return false;

            DateTime startCleaningTime = DateTime.Now;
            tmrCleaningBeforeTermination = new Timer();
            tmrCleaningBeforeTermination.Interval = 250;
            nextAutomationStepTime = DateTime.Now.AddSeconds(60.0);

            tmrCleaningBeforeTermination.Tick +=
                (o, e) =>
                {
                    TimeSpan span = nextAutomationStepTime - DateTime.Now;
                    if (span.TotalSeconds > 0)
                        OnCleaningCountdown(new TimeSpanEventArgs(span));
                    else
                    {
                        tmrCleaningBeforeTermination.Stop();
                        SendClose();
                        OnCleaningFinished();
                    }
                };
            tmrCleaningBeforeTermination.Start();
            return true;
        }

        private Timer tmrCleaningBeforeTermination;

        //private DateTime startCleaningTime;

        public event EventHandler<TimeSpanEventArgs> CleaningCountdown;

        protected void OnCleaningCountdown(TimeSpanEventArgs e)
        {
            CleaningCountdown?.Invoke(this, e);
        }

        public event EventHandler CleaningFinished;
        protected void OnCleaningFinished()
        {
            CleaningFinished?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #endregion

        public Signal Temperature { get; } = new Signal("Temperature", false);

        protected override void OnMessageReceived()
        {
            //receive the arduino message for temperature
            float temperature;
            if (float.TryParse(LastSerialMessage.ReceivedFilteredMessage, out temperature))
                Temperature.ActualValue = temperature;

            base.OnMessageReceived();
        }

    }
}