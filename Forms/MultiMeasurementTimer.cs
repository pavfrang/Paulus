using Paulus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Paulus.Forms
{
    public class MultiMeasurementTimer : Timer
    {
        #region Events
        public event EventHandler ResetCompleted;
        protected void OnResetCompleted()
        {
            var handler = ResetCompleted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler<TimeEventArgs> Started;
        protected void OnStarted()
        {
            var handler = Started;
            if (handler != null) handler(this, new TimeEventArgs(startTime));
        }

        public event EventHandler<TimeEventArgs> Paused;
        protected void OnPaused(DateTime time)
        {
            var handler = Paused;
            if (handler != null) handler(this, new TimeEventArgs(time));
        }

        public event EventHandler<TimeEventArgs> Resuming;
        protected void OnResuming(DateTime time)
        {
            var handler = Resuming;
            if (handler != null) handler(this, new TimeEventArgs(time));
        }

        public event EventHandler<TimeSpanEventArgs> Proceeded;
        protected void OnProceeded(TimeSpan duration)
        {
            var handler = Proceeded;
            if (handler != null) handler(this, new TimeSpanEventArgs(duration));
        }

        #endregion

        /// <summary>
        /// Set by: UpdateAverage, Reset
        /// Read by: UpdateAverage
        /// </summary>
        TimeSpan totalDuration; //total duration of completed tasks
        /// <summary>
        /// Set by: Reset, Start, Proceed
        /// Read by: 
        /// </summary>

        
         //the index of the current task (called by Reset, Start, Proceed, UpdateAverage)
        private int _measurementIndex;
        public int MeasurementIndex
        {
            get { return _measurementIndex; }
        }
     
        
        /// <summary>
        /// Set by:
        /// Proceed, Resume, Start
        /// Read by:
        /// Proceed, Pause, timer_Tick
        /// </summary>
        DateTime currentTime;
        /// <summary>
        /// Set by:
        /// Start, Proceed, Reset
        /// Read by:
        /// UpdateAverage
        /// </summary>
        DateTime startTime; //the start time of the current running task, and the start time of the first task

        public void Reset()
        {
            base.Stop();
            //reset internal variables
            totalDuration = new TimeSpan(); //needed to calculate the average
            _measurementIndex = 0; _hasStarted = false; _isPaused = false;

            OnResetCompleted();
        }

        bool _hasStarted = false;
        public bool HasStarted
        {
            get { return _hasStarted; }
        }

        //should have virtual but ok
        public new void Start()
        {
            if (_hasStarted) throw new InvalidOperationException("The timer is already running. Call Reset() prior to calling Start().");

            startTime = currentTime = DateTime.Now; _hasStarted = true;
            _measurementIndex = 1;

            OnStarted();

            base.Start();
        }

        public void ProceedToNextMeasurement()
        {
            if (!_hasStarted) throw new InvalidOperationException(
                "ProceedToNextMeasurement() is allowed only if Start() is preceded and the current timer is running.");
            DateTime tm = DateTime.Now;
            TimeSpan duration = tm - currentTime;
            currentTime = DateTime.Now;
            UpdateAverage(duration);
            OnProceeded(duration);

            _measurementIndex++;
        }

        TimeSpan lastCurrentDuration, lastTotalDuration;


        protected bool _isPaused = false;
        public bool IsPaused { get { return _isPaused; } }

        public void Pause() //only if paused=false
        {
            if (_isPaused) throw new InvalidOperationException("The timer is already paused. Call Resume() to continue or Reset() and Start() to begin from scratch.");
            if (!_hasStarted) throw new InvalidOperationException("The timer must have started in order to pause.");

            base.Stop();
            _isPaused = true;
            DateTime now = DateTime.Now;
            lastCurrentDuration = now - currentTime;
            lastTotalDuration = now - startTime;

            OnPaused(now);
        }

        public void Resume() //allowed only after a Pause call (pause=true)
        {
            if (!_isPaused) throw new InvalidOperationException("The Resume() operation is allowed after a Pause() call.");
            if (!_hasStarted) throw new InvalidOperationException("The timer must have started in order to Resume().");
            _isPaused = false;
            DateTime now = DateTime.Now;

            //reset the current and total time (omit the pause time)
            currentTime = now - lastCurrentDuration;
            startTime = now - lastTotalDuration;
            OnResuming(now);
            base.Start();
        }

        //called at the Proceed function
        private void UpdateAverage(TimeSpan duration)
        {
            totalDuration += duration;
            int averageSeconds = (int)(totalDuration.TotalSeconds / _measurementIndex);
            int averageMinutes = averageSeconds / 60;
            averageSeconds -= averageMinutes * 60;

            averageDuration = new TimeSpan(0, averageMinutes, averageSeconds);
        }

        TimeSpan averageDuration;
        public TimeSpan AverageDuration { get { return averageDuration; } }

        TimeSpan currentDuration;
        public TimeSpan CurrentDuration
        {
            get { return currentDuration; }
        }

        TimeSpan currentTotalDuration;
        public TimeSpan CurrentTotalDuration
        {
            get { return currentTotalDuration; }
        }

        protected override void OnTick(EventArgs e)
        {
            currentDuration = DateTime.Now - currentTime;
            currentTotalDuration = DateTime.Now - startTime;
            base.OnTick(e);
        }
    }
}
