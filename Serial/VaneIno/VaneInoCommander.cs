using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Paulus.Common;
using System.Threading;
using System.Diagnostics;

namespace Paulus.Serial.VaneIno
{
    public struct VaneStateStep
    {
        public bool[] IsVaneOn;
        public int DurationInMilliSeconds;

        //0110,10
        public static VaneStateStep Parse(string text)
        {
            string[] tokens = text.Split(',');
            string state = tokens[0];

            int durationInMilliSeconds = int.Parse(tokens[1]);
            bool[] isVaneOn = state.Select(c => c != '0').ToArray();
            return new VaneStateStep() { DurationInMilliSeconds = durationInMilliSeconds, IsVaneOn = isVaneOn };
        }
    }

    public class AutomationSettings
    {
        public List<VaneStateStep> VaneStateSteps { get; set; }

        //public int LoopDelayInSeconds { get; set; }

        public int TotalLoopsCount { get; set; }

        public int TotalTimeInMilliSeconds { get; private set; }


        public void LoadFromFile(string path)
        {
            //#comments
            //0110,10
            List<string> lines = File.ReadAllLines(path).Select(
                l =>
            {
                if (l.Contains("#"))
                    l = l.Substring(0, l.IndexOf("#"));
                return l.Trim();
            }).Where(l =>
           Regex.IsMatch(l, @"\d{4},\d+")).ToList();

            VaneStateSteps = lines.Select(l => VaneStateStep.Parse(l)).ToList();

            TotalTimeInMilliSeconds = VaneStateSteps.Sum(v => v.DurationInMilliSeconds);

        }
    }

    public enum AutomationStep
    {
        Paused,
        Started,
        Finished
    }

    public class VaneInoCommander : SerialDevice
    {


        public VaneInoCommander(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
            : this(new SerialPort(portName, baudRate, parity, dataBits, stopBits)) { }

        public VaneInoCommander(SerialPort port)
            : base(port, ResponseMode.Immediate)
        {
            ReceiveMessageSuffix = "\r\n";
            SendMessageSuffix = "\n";

            VaneIno = new VaneIno(false);

            serialPort.WriteTimeout = 20;
            serialPort.ReadTimeout = 50;
        }


        public VaneIno VaneIno;

        #region Send commands

        public override async Task<bool> TestCommunication()
        {
            return await ReadVanes();
        }

        public async Task<bool> SetVane(int iv, bool isOn)
        {
            string vanesState = "";
            for (int i = 1; i <= 4; i++)
                if (i != iv) vanesState += (int)VaneIno.Vane(i).ActualValue == 1 ? "1" : "0";
                else vanesState += isOn ? "1" : "0";

            var m = await SendMessageAndReadResponse($"SET{vanesState}");

            if (m.ReceivedFullMessage == $"[SET]{vanesState}")
            {
                VaneIno.Vane(iv).ActualValue = isOn ? 1.0f : 0.0f;
                return true;
            }
            return false;
        }

        public async Task<bool> StoreVanes()
        {
            string vanesState = "";
            for (int i = 1; i <= 4; i++)
                vanesState += (int)VaneIno.Vane(i).ActualValue == 1 ? "1" : "0";
            var m = await SendMessageAndReadResponse("STORE");
            return m.ReceivedFullMessage == $"[STORE]{vanesState}";
        }

        public async Task<bool> SetVanes(bool[] isOn)
        {
            string vanesState = "";
            foreach (bool b in isOn)
                vanesState += b ? "1" : "0";

            var m = await SendMessageAndReadResponse($"SET{vanesState}");

            if (m.ReceivedFullMessage == $"[SET]{vanesState}")
            {
                for (int i = 1; i <= 4; i++)
                    VaneIno.Vane(i).ActualValue = isOn[i - 1] ? 1.0f : 0.0f;
                return true;
            }
            return false;
        }

        int lastValue = 0;
        public async Task<int?> ReadAnalog()
        {
            var m = await SendMessageAndReadResponse("READA");

            int value = 0;
            bool parsed = int.TryParse(m.ReceivedFullMessage, out value);
            if (parsed)
            {
                VaneIno.Analog.ActualValue = lastValue = value;
                return value;
            }
            else
                return lastValue;
        }

        public async Task<bool> ReadAnalog2()
        {
            var m = await SendMessageAndReadResponse("READA2");

            int value1 = 0, value2 = 0, time = 0;
            if (m.IsError || m.ReceivedFullMessage == null) return false;



            string[] tokens = m.ReceivedFullMessage.Split(',');

            bool parsed = int.TryParse(tokens[0], out value1); if (!parsed) return false;
            parsed = int.TryParse(tokens[1], out time); if (!parsed) return false;
            parsed = int.TryParse(tokens[2], out value2); if (!parsed) return false;

            VaneIno.Analog1.ActualValue = value1;
            VaneIno.TimeBetween.ActualValue = time;
            VaneIno.Analog2.ActualValue = value2;

            return true;
        }



        public async Task<bool> ReadVanes()
        {
            var m = await SendMessageAndReadResponse("READV"); //0011

            if (m.ReceivedFilteredMessage == null) return false;

            bool parsed = Regex.IsMatch(m.ReceivedFullMessage, @"[01]{4}");

            if (parsed)
            {
                for (int i = 0; i < m.ReceivedFullMessage.Length; i++)
                    VaneIno.Vane(i + 1).ActualValue = m.ReceivedFullMessage[i] == '1' ? 1.0f : 0.0f;
            }

            return parsed;
        }

        #endregion

        #region Automation
        public AutomationSettings AutomationSettings { get; set; }

        private global::System.Windows.Forms.Timer tmrAutomation;

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

        public DateTime StartAutomationTime { get; private set; }

        public TimeSpan ElapsedAutomationTime { get { return DateTime.Now - StartAutomationTime; } }

        public int LoopsCompleted { get; private set; }

        #endregion

        public bool StartAutomation()
        {
            StartAutomationTime = DateTime.Now;
            LoopsCompleted = 0;

            tmrAutomation = new global::System.Windows.Forms.Timer();
            tmrAutomation.Interval = 250;

            tmrAutomation.Tick += tmrAutomation_Tick;

            OnAutomationStarted();

            tmrAutomation.Start();
            return true;
        }

        public bool IsAutomationRunning { get { return tmrAutomation?.Enabled ?? false; } }

        public event EventHandler AutomationStarted;
        protected void OnAutomationStarted()
        {
            nextAutomationStepTime = DateTime.Now;
            CurrentAutomationStep = AutomationStep.Started;
            CurrentVaneStep = -1;


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

        public int CurrentVaneStep { get; private set; }

        DateTime nextAutomationStepTime = DateTime.Now;
        public TimeSpan GetTimeUntilNextEvent { get { return nextAutomationStepTime - DateTime.Now; } }

        public TimeSpan GetTimeUntilTheEnd
        {
            get
            {
                TimeSpan untilNextEvent = nextAutomationStepTime - DateTime.Now;
                if (CurrentVaneStep < AutomationSettings.VaneStateSteps.Count - 1)
                    untilNextEvent = untilNextEvent.Add(new TimeSpan(0, 0, 0, 0, AutomationSettings.VaneStateSteps.Skip(CurrentVaneStep + 1).Select(v => v.DurationInMilliSeconds).Sum()));

                return untilNextEvent.Add(new TimeSpan(0, 0, 0, 0, (AutomationSettings.TotalLoopsCount - LoopsCompleted - 1) * AutomationSettings.TotalTimeInMilliSeconds));
            }
        }

        #region Pausing Automation

        TimeSpan timeUntilNextEvent;

        public void PauseAutomation()
        {
            //ignore the call if the automation timer is not enabled
            if (!tmrAutomation.Enabled) return;

            tmrAutomation.Stop();
            timeUntilNextEvent = nextAutomationStepTime - DateTime.Now;
            IsAutomationPaused = true;
        }

        public void ResumeAutomation()
        {
            //ignore the call if the automatio is not paused
            if (!IsAutomationPaused) return;

            IsAutomationPaused = false;
            nextAutomationStepTime = DateTime.Now.AddSeconds(timeUntilNextEvent.TotalSeconds);
            tmrAutomation.Start();
        }

        public bool IsAutomationPaused { get; private set; }

        public event EventHandler AutomationPaused;
        protected void OnAutomationPaused()
        {
            CurrentAutomationStep = AutomationStep.Paused;
            AutomationPaused?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Stopping Automation

        public bool StopAutomation()
        {
            tmrAutomation.Stop();
            return true;
        }

        public event EventHandler AutomationStopped;
        protected void OnAutomationStopped()
        {
            CurrentAutomationStep = AutomationStep.Paused;

            AutomationStopped?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Automation is finished

        public event EventHandler AutomationFinished;
        protected void OnAutomationFinished()
        {
            CurrentAutomationStep = AutomationStep.Finished;
            AutomationFinished?.Invoke(this, EventArgs.Empty);
        }

        #endregion


        DateTime lastReadAnalogTime = DateTime.Now;

        DateTime lastWaitingForNextStepTime = DateTime.Now;

        private async void tmrAutomation_Tick(object sender, EventArgs e)
        {

            var now = DateTime.Now;

            bool shouldChangeStep = now >= nextAutomationStepTime;

            if (!shouldChangeStep)
            {
                //should inform every 500 ms maximum
                OnWaitingForNextStep(new TimeSpanEventArgs(nextAutomationStepTime - now));
                return;
            }

            //we come here if we should change the step!
            var a = AutomationSettings;

            CurrentVaneStep++;

            if (CurrentVaneStep == a.VaneStateSteps.Count)
            {
                CurrentVaneStep = 0; LoopsCompleted++;

                if (LoopsCompleted >= this.AutomationSettings.TotalLoopsCount)
                {
                    //OnAutomationStopped();
                    tmrAutomation.Stop();
                    OnAutomationFinished();
                    return;
                }
            }

            //proceed to next step
            var vs = a.VaneStateSteps[CurrentVaneStep];
            await SetVanes(vs.IsVaneOn);

            //  Debug.WriteLine($"Step: {CurrentVaneStep}, Date: {DateTime.Now:HH:mm:ss.fff},  Time: {vs.DurationInMilliSeconds}");

            nextAutomationStepTime = now.AddMilliseconds(vs.DurationInMilliSeconds);

            OnAutomationStepChanged();
        }
    }
}
