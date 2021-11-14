using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paulus.Serial.UI
{
    public abstract class DeviceTimer<TCommander, TTimerCommand>
        where TCommander : DeviceCommander
        where TTimerCommand : struct, IConvertible //enum
    {
        public DeviceTimer(TCommander deviceCommander)
        {
            DeviceCommander = deviceCommander;

            //boxing is necessary
            //the first command should always be None
            CurrentCommandId = (TTimerCommand)(object)0;

            CommandsCount = Enum.GetValues(typeof(TTimerCommand)).Length;

            //initialize the timer
            timer = new Timer();
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
        }

        public TCommander DeviceCommander { get; }
        public TTimerCommand CurrentCommandId { get; protected set; }

        protected int CommandsCount { get; }

        public Task CurrentCommandTask { get; protected set; }

        //the handling of a command when it is finished
        public event EventHandler CurrentCommandCompleted;

        protected void OnCurrentCommandCompleted() =>
            CurrentCommandCompleted?.Invoke(this, EventArgs.Empty);

        public void Start()
        {
            timer.Start();
        }

        public bool Enabled { get { return timer.Enabled; } }

        private Timer timer;

        public async Task Pause()
        {
            timer.Stop();

            if (CurrentCommandTask != null && !CurrentCommandTask.IsCompleted)
            {
                await CurrentCommandTask;
                OnCurrentCommandCompleted();
            }
        }



        private void Timer_Tick(object sender, EventArgs e)
        {
            //proceed only if the current command is completed
            if (!DeviceCommander.IsConnected || CurrentCommandTask != null && !CurrentCommandTask.IsCompleted) return;

            //the current command has just completed
            //notify the handlers in order to update UI
            if (CurrentCommandTask != null)
                OnCurrentCommandCompleted();

            //proceed to next command

            //CurrentCommandId++;
            //boxing and unboxing is the only practical way to increment commandid
            CurrentCommandId = (TTimerCommand)(object)((int)(object)CurrentCommandId + 1);
            if ((int)(object)CurrentCommandId == CommandsCount)
                //go to the first command (0 is always None and 1 is the next command)
                CurrentCommandId = (TTimerCommand)(object)1;

            sendNextCommand();
        }

        /// <summary>
        /// Next command will be sent only if the previous command has been completed.
        /// </summary>
        protected abstract void sendNextCommand();

    }

}
