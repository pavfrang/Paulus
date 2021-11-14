using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Threading.Tasks;

using System.Threading; //ManualResetEventSlim, WaitHandle

namespace Paulus.Threads
{

    [Serializable]
    public class ThreadIDEventArgs : EventArgs
    {
        public ThreadIDEventArgs(int threadID)
        {
            _threadID = threadID;
        }

        protected int _threadID;
        public int ThreadID { get { return _threadID; } }
    }
}