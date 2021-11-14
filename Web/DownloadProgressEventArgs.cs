using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Paulus.Web
{
    /// <summary>
    /// Progress of a downloading file.
    /// </summary>
    public class DownloadProgressEventArgs : ProgressChangedEventArgs
    {
        #region Constructors
        public DownloadProgressEventArgs(long totalFileSize, long currentFileSize)
            : base((int)((double)currentFileSize / (double)totalFileSize * 100.0), currentFileSize)
        {
            this.totalFileSize = totalFileSize;
            this.currentFileSize = currentFileSize;
        }

        public DownloadProgressEventArgs(int percentDone)
            : base(percentDone, null) { }
        #endregion

        #region Properties

        private long totalFileSize;
        public long TotalFileSize
        {
            get { return totalFileSize; }
        }

        private long currentFileSize;
        public long CurrentFileSize
        {
            get { return currentFileSize; }
        }
        #endregion

    }

}
