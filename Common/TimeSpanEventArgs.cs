using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Common
{
    public class TimeSpanEventArgs : EventArgs
    {
        public TimeSpanEventArgs(TimeSpan time)
        {
            this.time = time;
        }

        TimeSpan time;
        public TimeSpan Time { get { return time; } }
    }
}
