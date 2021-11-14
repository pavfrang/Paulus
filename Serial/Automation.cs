using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Paulus.Serial
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TAutomationStep"></typeparam>
    ///<example>
    ///enum AutomationSample
    ///{
    ///	FirstOne,
    ///	SecondOne,
    ///	ThirdOne
    ///}
    ///
    ///Automation<AutomationSample> automation = new Automation<AutomationSample>();
    ///
    ///private void btnStart_Click(object sender, EventArgs e)
    ///{
    ///	automation.AutomationStepDurationsInSeconds = new Dictionary<AutomationSample, float>
    ///	{
    ///		{AutomationSample.FirstOne,5 },
    ///		{AutomationSample.SecondOne,4 },
    ///		{AutomationSample.ThirdOne,3 }
    ///	};
    ///	automation.MaximumLoops = 2;
    ///
    ///	automation.Started += (o, e2) => { lblStart.Text = "Started"; };
    ///	automation.AutomationStepChanged += (o, e2) => lblStep.Text = automation.CurrentAutomationStep.ToString();
    ///	automation.LoopCompleted += (o, e2) => lblEnd.Text = "Loops: " + automation.LoopsCompleted.ToString();
    ///	automation.WaitingForNextStep += (o, e2) => lblTime.Text = automation.TimeUntilNextEvent.TotalSeconds.ToString("0.0");
    ///	automation.Paused += (o, e2) => lblStart.Text = "PAUSED!";
    ///	automation.Resumed += (o, e2) => lblStart.Text = "RESUMED!";
    ///	automation.Stopped += (o, e2) => lblStart.Text = "STOPPED!";
    ///	automation.Finished += (o, e2) => lblStart.Text = "FINISHED!"; lblTime.Text = "";
    ///
    ///	btnStop.Enabled = btnPause.Enabled = true;
    ///	automation.Start(100);
    ///}
    ///
    ///private void btnPause_Click(object sender, EventArgs e)
    ///{
    ///	//works!
    ///	if (!automation.IsPaused)
    ///	{
    ///		automation.Pause();
    ///		btnPause.Text = "Resume";
    ///	}
    ///	else
    ///	{
    ///		automation.Resume();
    ///		btnPause.Text = "Pause";
    ///	}
    ///}
    ///
    ///private void btnStop_Click(object sender, EventArgs e)
    ///{
    ///	if (automation.IsRunning)
    ///	{
    ///		automation.Stop();
    ///		btnStop.Enabled = btnPause.Enabled = false;
    ///	}
    ///}
    /// </example>

    public class Automation<TAutomationStep> where TAutomationStep : struct, IComparable, IFormattable, IConvertible
    {
        public Automation()
        {
            automationSteps = Enum.GetValues(typeof(TAutomationStep)).Cast<TAutomationStep>().Distinct().ToList();
            tmrAutomation = new Timer();
            tmrAutomation.Tick += tmrAutomation_Tick;
        }

        private Timer tmrAutomation;

        /// <summary>
        /// The list of automation steps is automatically retrieved from the constructor.
        /// </summary>
        protected readonly List<TAutomationStep> automationSteps;
        public int AutomationStepsCount
        {
            get
            {
                return automationSteps.Count;
            }
        }

        /// <summary>
        /// The durations in seconds of each automation step.
        /// </summary>
        public Dictionary<TAutomationStep, float> AutomationStepDurationsInSeconds = new Dictionary<TAutomationStep, float>();

        /// <summary>
        /// The Maximum Loops must be set in order to constrain the total duration of the automation.
        /// </summary>
        public int MaximumLoops { get; set; } = int.MaxValue;

        #region Read-only properties during automation runnings
        public DateTime StartAutomationTime { get; private set; }
        public int LoopsCompleted { get; private set; }

        /// <summary>
        /// The event is fired each time the timer event handler is called. It is useful to retrieve the TimeUntilNextEvent in order to track the remaining time until the next automation step.
        /// </summary>
        public TimeSpan ElapsedAutomationTime { get { return DateTime.Now - StartAutomationTime; } }

        public TimeSpan TimeUntilNextEvent { get { return nextAutomationStepTime - DateTime.Now; } }

        private TAutomationStep _currentAutomationStep;
        public TAutomationStep CurrentAutomationStep
        {
            get { return _currentAutomationStep; }
            private set
            {
                _currentAutomationStep = value;
                OnAutomationStepChanged();
            }
        }

        protected int iAutomationStep;
        public int CurrentAutomationStepIndex { get { return iAutomationStep; } }
        #endregion

        #region Events

        /// <summary>
        /// Fired when Start() is called.
        /// </summary>
        public event EventHandler Started;
        protected void OnStarted() => Started?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Fired after each timer handling event. TimeUntilNextEvent is typically called when handling this event.
        /// </summary>
        public event EventHandler WaitingForNextStep;
        protected void OnWaitingForNextStep() => WaitingForNextStep?.Invoke(this, EventArgs.Empty);

        public event EventHandler AutomationStepChanged;
        protected void OnAutomationStepChanged() => AutomationStepChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Fired each time a loop is completed.
        /// </summary>
        public event EventHandler LoopCompleted;
        protected void OnLoopCompleted() => LoopCompleted?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Fired when Pause() is called.
        /// </summary>
        public event EventHandler Paused;
        protected void OnPaused() => Paused?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Fired when Resume() is called.
        /// </summary>
        public event EventHandler Resumed;
        protected void OnResumed() => Resumed?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Fired when Stop() is called. The Stop() function is also called when the total loops are completed..
        /// </summary>
        public event EventHandler Stopped;
        protected void OnStopped()
        {
            IsStopped = true;
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fired when the automation has been completed (after the Stopped event).
        /// </summary>
        public event EventHandler Finished;
        protected void OnFinished() => Finished?.Invoke(this, EventArgs.Empty);

        #endregion

        #region Start

        DateTime nextAutomationStepTime = DateTime.Now;

        public virtual bool Start(int timerWatchIntervalInMs = 400)
        {
            StartAutomationTime = DateTime.Now;

            IsPaused = IsStopped = false;
            LoopsCompleted = 0;
            OnLoopCompleted();
            iAutomationStep = 0;
            CurrentAutomationStep = automationSteps[0];
            nextAutomationStepTime = DateTime.Now.AddSeconds(AutomationStepDurationsInSeconds[_currentAutomationStep]);

            //initialize timer
            tmrAutomation.Interval = timerWatchIntervalInMs;

            OnStarted();

            tmrAutomation.Start();
            return true;
        }

        public bool IsRunning { get { return tmrAutomation?.Enabled ?? false; } }

        protected virtual void tmrAutomation_Tick(object sender, EventArgs e)
        {
            bool shouldChangeStep = DateTime.Now >= nextAutomationStepTime;

            if (!shouldChangeStep)
            {
                OnWaitingForNextStep();
                return;
            }

            if (iAutomationStep == AutomationStepDurationsInSeconds.Count - 1)
            {
                LoopsCompleted++;
                OnLoopCompleted();
                if (LoopsCompleted >= MaximumLoops)
                {
                    Stop();
                    OnFinished();
                    return;
                }

                iAutomationStep = -1; //reset automation step in order to go to 0
            }

            //we come here if we should change the step!
            CurrentAutomationStep = automationSteps[++iAutomationStep];
            nextAutomationStepTime = DateTime.Now.AddSeconds(AutomationStepDurationsInSeconds[_currentAutomationStep]);
        }

        #endregion

        public bool IsPaused { get; private set; }

        protected TimeSpan timeUntilNextEventInPauseMode; //this is a variable that stores the latest value when the Timer is paused.

        /// <summary>
        /// Pauses the automation. To resume the automation the ResumeAutomation should be called. The function is ignored if the timer has not started.
        /// </summary>
        public virtual void Pause()
        {
            //ignore the call if the automation timer is not enabled
            if (!tmrAutomation.Enabled) return;

            tmrAutomation.Stop();
            timeUntilNextEventInPauseMode = nextAutomationStepTime - DateTime.Now;
            IsPaused = true;

            OnPaused();
        }

        /// <summary>
        /// Resumes the automation. Should be called after Pause. If it is called after Stop, then the Start function is called.
        /// </summary>
        public virtual void Resume()
        {
            //ignore the call if the automation is not PAUSED
            if (!IsPaused) return;

            nextAutomationStepTime = DateTime.Now.AddSeconds(timeUntilNextEventInPauseMode.TotalSeconds);
            IsPaused = false;

            OnResumed();
            tmrAutomation.Start();
        }

        public bool IsStopped { get; private set; }

        /// <summary>
        /// Stops the automation. To begin the automation again, the StartAutomation should be called.
        /// </summary>
        public virtual void Stop()
        {
            tmrAutomation.Stop();
            IsStopped = true; IsPaused = false;
            OnStopped();
        }


    }
}
