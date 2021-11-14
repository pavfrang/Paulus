using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Win32.Simulator
{
    
    [Serializable]
    public class DateTimeEventArgs : EventArgs
    {
        public DateTimeEventArgs(DateTime dateTime)
        {
            _dateTime = dateTime;
        }

        protected DateTime _dateTime;
        public DateTime DateTime { get { return _dateTime; } }
    }

}
