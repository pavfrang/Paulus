using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paulus.Serial
{
    class AccurateTimer
    {
        private delegate void TimerEventDel(int id, int msg, IntPtr user, int dw1, int dw2);
        private const int TIME_PERIODIC = 1;
        private const int EVENT_TYPE = TIME_PERIODIC;// + 0x100;  // TIME_KILL_SYNCHRONOUS causes a hang ?!
        [DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(int msec);
        [DllImport("winmm.dll")]
        private static extern int timeEndPeriod(int msec);
        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimerEventDel handler, IntPtr user, int eventType);
        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        Action mAction;
        private int mTimerId;
        private TimerEventDel mHandler;  // NOTE: declare at class scope so garbage collector doesn't release it!!!

        public AccurateTimer(Action action, int delay)
        {
            mAction = action;
            timeBeginPeriod(1);
            mHandler = new TimerEventDel(TimerCallback);
            mTimerId = timeSetEvent(delay, 0, mHandler, IntPtr.Zero, EVENT_TYPE);
        }

        public void Stop()
        {
            int err = timeKillEvent(mTimerId);
            timeEndPeriod(1);
            System.Threading.Thread.Sleep(100);// Ensure callbacks are drained
        }

        private void TimerCallback(int id, int msg, IntPtr user, int dw1, int dw2)
        {
            if (mTimerId != 0)
                //mForm.BeginInvoke(mAction);
                mAction.Invoke();
        }
    }

    public class TimeEventArgs : EventArgs
    {
        public TimeSpan TimeSpan { get; }

        public DateTime DateTime { get; }

        public TimeEventArgs(TimeSpan timeSpan) => TimeSpan = timeSpan;

        public TimeEventArgs(DateTime dateTime) => DateTime = dateTime;

        public TimeEventArgs(DateTime dateTime, TimeSpan timeSpan)
        {
            DateTime = dateTime; TimeSpan = timeSpan;
        }

    }

    public class AsyncTimer
    {

        /// <summary>
        /// The time interval in milliseconds.
        /// </summary>
        public int Interval;

        public event EventHandler<TimeEventArgs> Tick;

        protected void OnTick(TimeEventArgs e) => Tick?.Invoke(this, e);


        private double f = Stopwatch.Frequency;

        public long StartTimeStamp { get; private set; }


        public DateTime StartTime { get; private set; }


        private long counter = 0;

        public Task RunTask { get; private set; }

        public bool Enabled { get; private set; }

        public Task Start()
        {
            StartTimeStamp = Stopwatch.GetTimestamp();
            StartTime = DateTime.Now;
            counter = 0;
            Enabled = true;

            return RunTask = Task.Run(() =>
           {
               while (Interlocked.Read(ref counter) == 0)
               {
                   long t = Stopwatch.GetTimestamp();
                   double time = (t - StartTimeStamp) / f;
                   DateTime absoluteTime = StartTime.AddSeconds(time);

                   OnTick(new TimeEventArgs(absoluteTime, new TimeSpan(0, 0, 0, 0, (int)(1000 * time))));

                   //if (Interlocked.Read(ref counter) > 0) break;
                   //Debug.WriteLine(counter);
                   Thread.Sleep(Interval);
               }

           });
        }



        public async Task Stop()
        {
            Interlocked.Increment(ref counter);
            if (RunTask != null) await RunTask;
            Enabled = false;
            RunTask = null;
        }
    }

    public struct Variable
    {
        public Variable(string name, string unit = "-")
        {
            Name = name;
            Unit = unit;
        }

        public string Name { get; }
        public string Unit { get; }
    }


    //see BoschRecorder for example
    //Variables and Values must be overriden
    public abstract class Recorder
    {
        public Recorder(string filePrefix, string basePath, int timeStepInMs)
        {
            //tmrRecorder = new System.Windows.Forms.Timer();
            //tmrRecorder = new System.Timers.Timer();
            tmrRecorder = new AsyncTimer();

            tmrRecorder.Interval = this.TimeStep = timeStepInMs;

            //tmrRecorder.Elapsed += tmrRecorder_Elapsed;
            //tmrRecorder.Tick += tmrRecorder_Tick;
            tmrRecorder.Tick += TmrRecorder_Tick;


            // initializeTimer();


            FilePrefix = filePrefix;
            BasePath = basePath;
        }



        protected CultureInfo en = CultureInfo.InvariantCulture;

        #region Timer and time

        //System.Windows.Forms.Timer tmrRecorder;
        //System.Timers.Timer tmrRecorder;
        AsyncTimer tmrRecorder;

        int _timeStep;
        /// <summary>
        /// The recorder time step in ms.
        /// </summary>
        public int TimeStep
        {
            get { return _timeStep; }

            set
            {
                if (value == _timeStep) return;

                if (IsRecording)
                    throw new InvalidOperationException("Cannot change recording interval while recording is active.");

                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(TimeStep), "Time step cannot be less than zero.");

                tmrRecorder.Interval = _timeStep = value;
            }
        }

        protected long startTime2; //used for precise timing
        protected DateTime startTime;
        public DateTime StartTime { get { return startTime; } }

        protected string currentFile;
        public string CurrentFilePath { get { return currentFile; } }


        // int iRecordStep = 0;

        public string FilePrefix { get; set; }

        public string BasePath { get; set; }

        public string SubfolderPathFormat { get; set; }

        #endregion

        #region Events
        public event EventHandler Starting;

        protected void OnStarting(CancelEventArgs e)
        {
            Starting?.Invoke(this, e);
        }

        public event EventHandler Started;
        protected void OnStarted()
        {
            Started?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Stopping;

        protected void OnStopping(CancelEventArgs e)
        {
            Stopping?.Invoke(this, e);
        }

        public event EventHandler Stopped;
        protected void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// The event is fired after the Pause() method is called.
        /// </summary>
        public event EventHandler Paused;
        protected void OnPaused()
        {
            Paused?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// The event is fired after the Resume() method is called.
        /// </summary>
        public event EventHandler Resumed;
        protected void OnResumed()
        {
            Resumed?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Start

        public bool IsRecording { get { return tmrRecorder.Enabled; } }

        /// <summary>
        /// Starts file recording. If recording is already active, then the call is ignored.
        /// </summary>
        public void Start()
        {
            if (IsRecording) return;

            CancelEventArgs e = new CancelEventArgs();
            OnStarting(e);
            if (e.Cancel)
                return;

            StartRecording();

            OnStarted();
        }

        private void StartRecording()
        {
            // iRecordStep = 0;
            startTime = DateTime.Now; startTime2 = Stopwatch.GetTimestamp();

            string fileName = GetRecorderFileName();

            string subfolder = DateTime.Now.ToString(SubfolderPathFormat);

            this.currentFile = string.IsNullOrWhiteSpace(SubfolderPathFormat) ?
                Path.Combine(BasePath, fileName) :
                Path.Combine(BasePath, subfolder, fileName);

            if (!string.IsNullOrWhiteSpace(SubfolderPathFormat) &&
                !Directory.Exists(Path.Combine(BasePath, subfolder)))
                Directory.CreateDirectory(Path.Combine(BasePath, subfolder));

            writeFirstLine();

            tmrRecorder.Start();
        }



        private void tmrRecorder_Tick(object sender, EventArgs e)
        {
            writeLine();
        }

        private void tmrRecorder_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            writeLine();
        }
        protected virtual void writeFirstLine()
        {
            using (StreamWriter writer = new StreamWriter(currentFile))
            {
                writer.Write("Time\tAbsolute_Time\t" + string.Join("\t", from v in Variables select v.Name) + "\t");
                writer.WriteLine("$iStartTime\t");

                writer.Write("s\t-\t" + string.Join("\t", from v in Variables select v.Unit) + "\t");
                writer.WriteLine("\t"); //for the recording time

                //DateTime currentTime = DateTime.Now;
                //double time = (currentTime - startTime).TotalSeconds; //; (iRecordStep++) * _timeStep / 1000.0f;
                //writer.Write($"{time:0.000}\t{currentTime:HH:mm:ss.fff}\t");
                writer.Write($"0.000\t{DateTime.Now:HH:mm:ss.fff}\t");
                writer.Write(string.Join("\t", Values) + "\t");
                writer.WriteLine($"{GetRecordingTime()}\t");
                writer.Flush();
            }
        }

        protected virtual void writeLine()
        {
            using (StreamWriter writer = new StreamWriter(currentFile, true))
            {

                //DateTime currentTime = DateTime.Now;
                //double time = (currentTime - startTime).TotalSeconds; //; (iRecordStep++) * _timeStep / 1000.0f;
                //writer.Write($"{time:0.000}\t{currentTime:HH:mm:ss.fff}\t");
                long currentTime = Stopwatch.GetTimestamp();
                double time = (currentTime - startTime2) / (double)Stopwatch.Frequency;
                writer.Write($"{time:0.000}\t{startTime.AddSeconds(time):HH:mm:ss.fff}\t");


                writer.WriteLine(string.Join("\t", Values) + "\t\t");
                writer.Flush();
            }
        }

        protected virtual void TmrRecorder_Tick(object sender, TimeEventArgs e)
        {
            using (StreamWriter writer = new StreamWriter(currentFile, true))
            {

                writer.Write($"{e.TimeSpan.TotalSeconds:0.000}\t{e.DateTime:HH:mm:ss.fff}\t");
                writer.WriteLine(string.Join("\t", Values) + "\t\t");
                writer.Flush();
            }
        }


        //protected static CultureInfo en = CultureInfo.GetCultureInfo("en-us");

        protected string GetRecordingTime()
        {
            return string.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
        }

        protected string GetRecorderFileName()
        {
            if (string.IsNullOrWhiteSpace(BasePath) || !Directory.Exists(BasePath))
                BasePath = Application.StartupPath;

            string subfolder = DateTime.Now.ToString(SubfolderPathFormat);
            string recorderParentPath = string.IsNullOrWhiteSpace(SubfolderPathFormat) ?
                BasePath : Path.Combine(BasePath, subfolder);

            if (Directory.Exists(recorderParentPath))
            {

                int[] indices = Directory.GetFiles(recorderParentPath, "*.txt").
                    Where(file => Regex.IsMatch(Path.GetFileName(file), $@"{FilePrefix}.*R(?<index>\d+).txt")).
                    Select(file =>
                    {
                        Match m = Regex.Match(Path.GetFileName(file), $@"{FilePrefix}.*R(?<index>\d+).txt");
                        return int.Parse(m.Groups["index"].Value);
                    }).ToArray();

                int newIndex = indices.Length > 0 ? indices.Max() + 1 : 1;
                return $"{FilePrefix} {DateTime.Now:yyyy-MM-dd HH_mm_ss} R{newIndex}.txt";
            }
            else
                return $"{FilePrefix} {DateTime.Now:yyyy-MM-dd HH_mm_ss} R1.txt";

        }
        #endregion

        #region Stop
        /// <summary>
        /// Stops the file recording. If the recording is already stopped (but not paused) then the call is ignored.
        /// </summary>
        public async Task Stop()
        {
            if (!IsRecording && !_isPaused)
                return;


            CancelEventArgs e = new CancelEventArgs();
            OnStopping(e);
            if (e.Cancel)
                return;

            await StopRecording();

            OnStopped();
        }

        protected async Task StopRecording()
        {
            await tmrRecorder.Stop();
            //tmrRecorder2.Stop();
        }
        #endregion


        #region Pause


        protected bool _isPaused = false;
        public bool IsPaused { get { return _isPaused; } }

        /// <summary>
        /// Pausing the recording does not change the recording file. The Pause method is typically called after a Start() call. The method call is ignored if not currently in recording state.
        /// </summary>
        public async void  Pause()
        {
            if (!IsRecording) return;

            await tmrRecorder.Stop();
            //tmrRecorder2.Stop();
            _isPaused = true;

            OnPaused();
        }


        /// <summary>
        /// Resume method is typically called after a Pause() call. The recording file does not change. The method call is ignored if not in pause.
        /// </summary>
        public void Resume()
        {
            if (!_isPaused) return;

            _isPaused = false;

            OnResumed();

            tmrRecorder.Start();
        }

        #endregion

        public abstract Variable[] Variables { get; }

        protected abstract object[] Values { get; }





    }
}
