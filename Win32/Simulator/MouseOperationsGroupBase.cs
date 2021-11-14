using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Win32.Simulator
{
    public class MouseOperationsGroupBase
    {
        protected DateTime _lastClickTime;
        public DateTime LastClickTime { get { return _lastClickTime; } }

        protected DateTime _startedTime;
        public TimeSpan TotalTimeElapsed()
        {
            return DateTime.Now - _startedTime;
        }

        public FirstClickTime FirstClickTime;
        public int DelayInterval; //in ms

    }
}
