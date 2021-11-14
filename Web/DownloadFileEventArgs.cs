using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Web
{
    public class DownloadFileEventArgs : EventArgs
    {
        public DownloadFileEventArgs(string sourceURL, string destination)
        {
            this.sourceURL = sourceURL; this.destination = destination;
        }

        private string sourceURL;
        public string SourceURL
        {
            get { return sourceURL; }
        }

        private string destination;
        public string Destination
        {
            get { return destination; }
        }




    }
}
