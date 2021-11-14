using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Paulus.Web
{
    public class FileListDownloadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        public FileListDownloadProgressChangedEventArgs(int percentage, string source, string destination)
            :base(percentage,source)
        {
            this.source = source; this.destination = destination;
        }

        string source;

        public string Source
        {
            get { return source; }
        }

        string destination;

        public string Destination
        {
            get { return destination; }
        }
    }
}
