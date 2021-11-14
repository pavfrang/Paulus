using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.ObjectModel;

namespace Paulus.Serial
{
    //see sample application Flap3Chart
    public class LiveChart : IEnumerable<LiveSeries>
    {
        public LiveChart(Chart chart, ChartArea chartArea, int pollingInterval)
        {
            this._chartArea = chartArea;
            this._chart = chart;

            this.timer = new Timer();
            timer.Interval = pollingInterval;
            timer.Tick += Timer_Tick;

        }
        protected internal Chart _chart;
        public Chart Chart { get { return _chart; } }

        protected internal ChartArea _chartArea;
        public ChartArea ChartArea { get { return _chartArea; } }

        #region Timer
        protected Timer timer;

        public int Interval { get { return timer.Interval; } set { timer.Interval = value; } }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        public bool Enabled
        {
            get { return timer.Enabled; }
            set
            {
                if (value && !_isInitialized)
                    initializeChart();

                timer.Enabled = value;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            updateChart();
        }



        #endregion


        #region Initialization and update

        public event EventHandler Initialized;
        protected void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }


        public void Reset()
        {
            _isInitialized = false;
            initializeChart();
        }

        protected bool _isInitialized = false;
        protected virtual void initializeChart()
        {
            ChartArea c = _chartArea;
            c.AxisX.LabelStyle.Format = "HH:mm:ss";
            c.AxisX.IntervalType = DateTimeIntervalType.Seconds;
            c.AxisX.Interval = 5;

            c.AxisX.MajorGrid.IntervalType = DateTimeIntervalType.Seconds;
            c.AxisX.MajorGrid.Interval = 1;

            _startTime = DateTime.Now;

            c.AxisX.Minimum = _startTime.ToOADate();
            c.AxisX.Maximum = _startTime.AddSeconds(_timeRangeIntervalInSeconds).ToOADate();

            c.AxisX.MajorGrid.LineColor = Color.FromArgb(30, Color.LightGray);
            c.AxisY.MajorGrid.LineColor = Color.FromArgb(30, Color.LightGray);
            //c.AxisX.MajorGrid.Enabled = false;
            //c.AxisY.MajorGrid.Enabled = false;


            // _maxNumberOfPointsPerSeries = _timeRangeIntervalInSeconds / Interval + 1;

            initializeSeries();

            _isInitialized = true;

            OnInitialized();

        }

        protected virtual void initializeSeries()
        {
            _liveSeries.Clear();
            _chart.Series.Clear();
        }

        protected internal Dictionary<string, LiveSeries> _liveSeries = new Dictionary<string, LiveSeries>();
        //ReadOnlyDictionary is available only from .NET 4.5 and later
        //public ReadOnlyDictionary<string, LiveSeries> SignalSeries { get { return _signalSeries; } }
        //public Dictionary<string, LiveSeries> SignalSeries { get { return _signalSeries; } }
        public LiveSeries this[string name]
        {
            get
            {
                return _liveSeries[name];
            }
        }

        public void Remove(string name)
        {
            _liveSeries.Remove(name);
        }


        public LiveSeries Add(string name, Color? customColor = null)
        {
            LiveSeries newLiveSeries = new LiveSeries(this, name, customColor);
            _liveSeries.Add(name, newLiveSeries);
            return newLiveSeries;
        }

        #endregion

        #region Chart updating

        //The start time is set when the chart is initialized.
        protected DateTime _startTime;

        protected DateTime _currentTime;
        /// <summary>
        /// The current time is updated once, internally in the updateChart function for consistency reasons.
        /// </summary>
        public DateTime CurrentTime { get { return _currentTime; } }

        public bool ShouldShiftSeries()
        {
            return _isInitialized &&
            (_currentTime - _startTime).TotalSeconds >= _timeRangeIntervalInSeconds;
        }

        private void updateChart()
        {
            _currentTime = DateTime.Now;

            updateValues();

            updateChartArea();
        }


        /// <summary>
        /// The event allows external live updating of values from outside.
        /// A LiveSeries is updated via the LiveSeries.AddValue method. The time and shift parameters passed 
        /// </summary>
        public event EventHandler UpdatingValues;
        protected void OnUpdatingValues()
        {
            UpdatingValues?.Invoke(this, EventArgs.Empty);
        }


        protected virtual void updateValues()
        {
            OnUpdatingValues();
        }

        protected virtual void updateChartArea()
        {
            if (this.ShouldShiftSeries())
            {
                _chartArea.AxisX.Minimum = (_currentTime.AddSeconds(-_timeRangeIntervalInSeconds)).ToOADate();
                _chartArea.AxisX.Maximum = _currentTime.ToOADate();
            }
        }

        #endregion

        public IEnumerator<LiveSeries> GetEnumerator()
        {
            return _liveSeries.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _liveSeries.Values.GetEnumerator();
        }

        int _timeRangeIntervalInSeconds = 20;
        public int TimeRangeIntervalInSeconds
        {
            get { return _timeRangeIntervalInSeconds; }
            set
            {
                if (value < 2)
                    throw new ArgumentOutOfRangeException(nameof(TimeRangeIntervalInSeconds));

                _timeRangeIntervalInSeconds = value;
            }
        }

    }

}
