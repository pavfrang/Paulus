
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace Paulus.Web
{
    public class FileListDownloader
    {
        public FileListDownloader()
        {
            downloader = new FileDownloader();
            downloader.Downloaded += new EventHandler(downloader_DownloadComplete);
            downloader.ProgressChanged += new EventHandler<DownloadProgressEventArgs>(downloader_ProgressChanged);
        }


        Dictionary<string, string> _items = new Dictionary<string, string>();

        public Dictionary<string, string> Items { get { return _items; } }

        private FileDownloader downloader;

        /// <summary>
        /// Occurs when the whole list has been downloaded.
        /// </summary>

        public event EventHandler Downloaded;
        protected void OnDownloaded(EventArgs e)
        {
            EventHandler handler = Downloaded;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<FileListDownloadProgressChangedEventArgs> DownloadProgressChanged;
        protected void OnDownloadProgressChanged(FileListDownloadProgressChangedEventArgs e)
        {
            var handler = DownloadProgressChanged;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<DownloadFileEventArgs> DownloadedFile;
        protected void OnDownloadedFile(DownloadFileEventArgs e)
        {
            var handler = DownloadedFile;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Downloads the desired list of files asynchronously.
        /// </summary>
        public void Download()
        {
            counter = 0;
            Proceed();
        }

        //occurs when a part of the file has been downloaded
        void downloader_ProgressChanged(object sender, DownloadProgressEventArgs e)
        {
            int percentage = (int)((double)(counter - 1) / (double)(_items.Count) * 100.0);
            percentage += (int)((double)e.ProgressPercentage / (double)(_items.Count));
            OnDownloadProgressChanged(new FileListDownloadProgressChangedEventArgs(percentage, currentEntry.Key, currentEntry.Value));
        }

        //occurs when the whole currrent file has been downloaded
        void downloader_DownloadComplete(object sender, EventArgs e)
        {
            int percentage = (int)((double)counter / (double)(_items.Count) * 100.0);
            OnDownloadProgressChanged(new FileListDownloadProgressChangedEventArgs(percentage, currentEntry.Key, currentEntry.Value));
            OnDownloadedFile(new DownloadFileEventArgs(currentEntry.Key, currentEntry.Value));
            Proceed();
        }

        int counter;
        KeyValuePair<string, string> currentEntry;
        public KeyValuePair<string, string> CurrentEntry { get { return currentEntry; } }

        private void Proceed()
        {
            if ((++counter) <= _items.Count)
            {
                currentEntry = _items.ElementAt<KeyValuePair<string, string>>(counter - 1);
                downloader.AsyncDownload(currentEntry.Key, currentEntry.Value);
            }
            else
                OnDownloaded(EventArgs.Empty);
        }


    }
}