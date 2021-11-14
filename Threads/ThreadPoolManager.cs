using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Threading.Tasks;

using System.Threading; //ManualResetEventSlim, WaitHandle

namespace Paulus.Threads
{

    /// <summary>
    /// Class that manages many threads inside a ThreadPool.
    /// </summary>
    /// <example>
    ///  //initialize with number of threads
    ///  ThreadPoolManager copyProcessor = new ThreadPoolManager(8);
    ///  copyProcessor.DoWork += processor_CopyGrid;
    ///  ...
    ///  protected void processor_CopyGrid(object sender, ThreadIDEventArgs e)
    ///  {
    ///        int threadID = e.ThreadID;
    ///        //make calculations based on threadID ...
    ///  }
    ///  ...
    ///  //run all threads and return after the completion of all tasks
    ///  copyProcessor.Run(true);
    /// </example>
    public class ThreadPoolManager
    {
        public ThreadPoolManager(int threadsCount = 8)
        {
            ThreadsCount = threadsCount;
        }


        private int _threadsCount;
        /// <summary>
        /// The number of threads. This property must be set before executing the Run method.
        /// </summary>
        public int ThreadsCount
        {
            get { return _threadsCount; }
            set
            {
                if (_isRunning) throw new InvalidOperationException("Cannot reset the number of threads while running.");
                if (value <= 0) throw new ArgumentOutOfRangeException("ThreadsCount", "The number of threads must be nonzero positive.");

                _threadsCount = value;
                setResetEventsAndHandles();
            }
        }

        private bool _isRunning;
        /// <summary>
        /// Returns True if at least one task is running.
        /// </summary>
        public bool IsRunning { get { return _isRunning; } }

        #region Thread internals (handles and events)
        private ManualResetEventSlim[] doneEvents;
        private WaitHandle[] waitHandles; //to be passed to the WaitAll method
        private void setResetEventsAndHandles()
        {

            doneEvents = new ManualResetEventSlim[_threadsCount];
            waitHandles = new WaitHandle[_threadsCount];

            for (int iEvent = 0; iEvent < _threadsCount; iEvent++)
            {
                doneEvents[iEvent] = new ManualResetEventSlim(false);
                waitHandles[iEvent] = doneEvents[iEvent].WaitHandle;
            }
        }
        #endregion


        /// <summary>
        /// Runs all defined tasks asynchronously. Tasks are managed by handling the DoWork event. 
        /// </summary>
        /// <param name="waitAll">If set to true then the method returns after finishing all tasks.</param>
        public void Run(bool waitAll = true)
        {
            _isRunning = true;

            //reset manual handles first
            for (int iThread = 0; iThread < _threadsCount; iThread++)
                doneEvents[iThread].Reset(); //set all ResetEvent to false

            //run each task asynchronously
            for (int iThread = 0; iThread < _threadsCount; iThread++)
                ThreadPool.QueueUserWorkItem(actionPerThread, iThread);

            //wait for all tasks to complete if the user asks for it
            if (waitAll)
            {
                WaitHandle.WaitAll(waitHandles);
                _isRunning = false;
                OnWorkCompleted();
            }
        }

        /// <summary>
        /// This is the WaitCallback method that is used by the Run method.
        /// </summary>
        /// <param name="threadContext"></param>
        private void actionPerThread(object threadContext)
        {
            int threadID = (int)threadContext;
            OnDoWork(threadID);
            //set the handle flag at the end
            doneEvents[threadID].Set();
        }

        #region Events
        /// <summary>
        /// The DoWork event must be handled, in order to use this class. The event procedure passes a ThreadID which is in the range 0 to ThreadsCount-1.
        /// </summary>
        public event EventHandler<ThreadIDEventArgs> DoWork;
        protected void OnDoWork(int threadID)
        {
            var handler = DoWork;
            if (handler != null) handler(this, new ThreadIDEventArgs(threadID));
        }

        /// <summary>
        /// Raised when the work of all threads has been completed.
        /// </summary>
        public event EventHandler WorkCompleted;
        protected void OnWorkCompleted()
        {
            var handler = WorkCompleted;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion



    }
}
