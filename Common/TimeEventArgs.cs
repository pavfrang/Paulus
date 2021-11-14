using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Common
{
    public class TimeEventArgs:EventArgs
    {
        public TimeEventArgs(DateTime time)
        {
            this.time = time;
        }

        DateTime time;
        public DateTime Time { get { return time; } }
    }



   
}
