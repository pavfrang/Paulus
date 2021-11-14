using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Paulus.Win32.Simulator
{
    public enum TimeUnits
    {
        Seconds, Minutes
    }

    public enum FirstClickTime
    {
        AfterFirstInterval, AfterDelay
    }

    public class AutoClicker : MouseOperationsGroupBase
    {
        public AutoClicker()
        {
            initializeTimers();
        }

        #region Properties

        private List<SimpleMouseOperation> _mouseOperations = new List<SimpleMouseOperation>();
        public List<SimpleMouseOperation> MouseOperations { get { return _mouseOperations; } set { _mouseOperations = value; } }


        public bool HasStarted { get { return tmrInterval.Enabled || tmrDelay.Enabled; } }

        private int _currentInterval;
        public int CurrentInterval
        {
            get { return _currentInterval; }
        }

        private int[] _intervals; //all sender intervals in ms
        public int[] Intervals { get { return _intervals; } set { _intervals = value; } }
        #endregion

        #region Timers

        private Timer tmrDelay,tmrInterval,  tmrMouseOperation;
        private void initializeTimers()
        {
            tmrInterval = new Timer();
            tmrDelay = new Timer();
            tmrMouseOperation = new Timer();

            tmrInterval.Tick += new EventHandler(tmrSender_Tick);
            tmrDelay.Tick += new EventHandler(tmrDelay_Tick);
            tmrMouseOperation.Tick += new EventHandler(tmrMouseOperation_Tick);
        }

        #endregion

        #region Timer handlers
        public event EventHandler<DateTimeEventArgs> Clicked;
        private void tmrSender_Tick(object sender, EventArgs e)
        {
            //send the click
            StartMouseOperations();

            //update the last click time
            _lastClickTime = DateTime.Now;
            if (Clicked != null) Clicked(this, new DateTimeEventArgs(_lastClickTime));

            //set the next interval
            if (++_currentInterval == _intervals.Length) _currentInterval = 0;
            tmrInterval.Interval = _intervals[_currentInterval];
        }

        public event EventHandler AfterDelay;
        private void tmrDelay_Tick(object sender, EventArgs e)
        {
            tmrDelay.Stop();
            if (AfterDelay != null) AfterDelay(this, EventArgs.Empty);
            _lastClickTime = DateTime.Now;
            if (FirstClickTime == FirstClickTime.AfterDelay) StartMouseOperations();
            tmrInterval.Start();
        }

        private void tmrMouseOperation_Tick(object sender, EventArgs e)
        {
            NextMouseOperation();
        }
        #endregion

        public event EventHandler Stopped;
        public void Stop()
        {
            tmrMouseOperation.Stop();
            tmrInterval.Stop();
            tmrDelay.Stop();
            if (Stopped != null) Stopped(this, EventArgs.Empty);
        }

        private int CurrentMouseOperationIndex;

        private void StartMouseOperations()
        {
            CurrentMouseOperationIndex = -1;
            NextMouseOperation();
        }

        private void NextMouseOperation()
        {
            if (++CurrentMouseOperationIndex < _mouseOperations.Count)
            {
                SimpleMouseOperation currentMouseOperation = _mouseOperations[CurrentMouseOperationIndex];
                Cursor.Position = currentMouseOperation.Position;
                MouseSimulator.MultiClick(currentMouseOperation.Button, (int)currentMouseOperation.ClickType);
                if (currentMouseOperation.Delay > 0)
                {
                    tmrMouseOperation.Interval = currentMouseOperation.Delay;
                    tmrMouseOperation.Start(); //start in case that the mouse has not started yet
                }
                else NextMouseOperation();
            }
            else tmrMouseOperation.Stop();
        }

        public event EventHandler Starting, BeforeDelay;

        public void Start()
        {
            if (Starting != null) Starting(this, EventArgs.Empty);
            _startedTime = DateTime.Now;
            tmrInterval.Interval = _intervals[0];
            _currentInterval = 0;
            _lastClickTime = DateTime.Now;

            if (DelayInterval > 0)
            {
                if (BeforeDelay != null) BeforeDelay(this, EventArgs.Empty);
                tmrDelay.Interval = DelayInterval;
                tmrDelay.Start();
            }
            else
            {
                if (FirstClickTime == FirstClickTime.AfterDelay) StartMouseOperations();
                if (AfterDelay != null) AfterDelay(this, EventArgs.Empty);
                tmrInterval.Start();
            }
        }

        //retrieve the total mouse operation time in seconds
        public int GetTotalMouseOperationsTime()
        {
            int sum = 0;
            foreach (SimpleMouseOperation op in _mouseOperations) sum += op.Delay;
            return sum;
        }

        /// <summary>
        /// Valid intervals are less than 5 seconds and less than the total mouse operations time.
        /// </summary>
        /// <returns></returns>
        public bool ValidateIntervals()
        {
            bool isValid = true; //do not allow less than 5 seconds
            int totalMouseOperationsTime = GetTotalMouseOperationsTime();

            for (int i = 0; i < _intervals.Length; i++)
                if (_intervals[i] < 5000 ||
                    _intervals[i] < GetTotalMouseOperationsTime()) { isValid = false; break; }
            return isValid;
        }

        public TimeSpan GetRemainingTimeToNextEvent()
        {
            if (!tmrInterval.Enabled)
                return _lastClickTime.AddMilliseconds(tmrDelay.Interval) - DateTime.Now;
            else
                return _lastClickTime.AddMilliseconds((double)_intervals[_currentInterval]) - DateTime.Now;

        }

        public void LoadFromFile(string filePath,bool appendAtTheEndOfExistingMouseOperations)
        {
            //SettingsFile file = new SettingsFile(filePath,true);
            //Section operationsSection = file["mouse operations"];
            //if (!appendAtTheEndOfExistingMouseOperations) _mouseOperations.Clear(); //remove only if the append is set to false
            //_mouseOperations.AddRange(SimpleMouseOperation.GetSimpleMouseOperations(operationsSection.Lines));


            //Section settingsSection = file["timer settings"];
            //settingsSection.PopulateDictionary();
        
            //DelayInterval = int.Parse(settingsSection["Delay"]);
            //FirstClickTime = settingsSection["First click type"] == "AfterDelay" ? FirstClickTime.AfterDelay : FirstClickTime.AfterFirstInterval;
            //_intervals = GetIntervals(settingsSection["Intervals"], TimeUnits.Seconds);
        }      
        
        public void Save(string filePath)
        {
            //SettingsFile file = new SettingsFile(filePath);

            ////x,y,button,clicktype,delay,description
            //List<string> data = new List<string>(); ;
            //foreach (SimpleMouseOperation op in _mouseOperations)
            //    data.Add(string.Format("{0},{1},{2},{3},{4},{5}",
            //        op.Position.X, op.Position.Y, op.Button, op.ClickType, op.Delay, op.Description));

            //Section newSection = new Section("mouse operations",1);
            //newSection.Lines = data;
            //file.Sections.Add(newSection.Name, newSection);

            //newSection = new Section("timer settings",2);
            //newSection.Dictionary.Add("Delay", DelayInterval.ToString());
            //newSection.Dictionary.Add("First click type", FirstClickTime.ToString());
            //newSection.Dictionary.Add("Intervals", BuildSenderIntervalsLine());
            //file.Sections.Add(newSection.Name, newSection);

            //file.Save();
        }

        public string BuildSenderIntervalsLine() //always in seconds
        {
            StringBuilder builder = new StringBuilder();
            if (_intervals.Length > 0)
                foreach (int interval in _intervals)
                    builder.Append(interval / 1000 + ",");
            return builder.ToString().TrimEnd(',');
        }

        public static int[] GetIntervals(string text, TimeUnits unit)
        {
            string[] sIntervals = text.Split(',');
            int[] tmp = new int[sIntervals.Length];
            int multiplier = unit == TimeUnits.Seconds ? 1 : 60;
            for (int i = 0; i < sIntervals.Length; i++)
                tmp[i] = int.Parse(sIntervals[i]) * 1000 * multiplier;

            return tmp;
        }

    }
}