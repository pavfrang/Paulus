/*
 * Originally written by John Batte
 * Modifications, API changes and cleanups by Phil Crosby
 * http://codeproject.com/cs/library/downloader.asp
 */

using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using Paulus.IO;

namespace Paulus.Web
{
    /// <summary>
    /// Downloads and resumes files from HTTP, FTP, and File (file://) URLS
    /// </summary>
    public class FileDownloader
    {
        #region Public properties
        protected string destinationFilename;
        /// <summary>
        /// This is the name of the file we get back from the server when we
        /// try to download the provided url. It will only contain a non-null
        /// string when we've successfully contacted the server and it has started
        /// sending us a file.
        /// </summary>
        public string DestinationFilename
        {
            get { return destinationFilename; }
        }

        protected string sourceURL;
        public string SourceURL
        {
            get { return sourceURL; }
        }

        private long totalDownloaded;
        public long TotalDownloaded
        {
            get { return totalDownloaded; }
        }

        protected IWebProxy proxy = null;
        /// <summary>
        /// Proxy to be used for http and ftp requests.
        /// </summary>
        public IWebProxy Proxy
        {
            get { return proxy; }
            set { proxy = value; }
        }

        // Determines whether the user has canceled or not.
        protected bool canceled = false;
        public void Cancel()
        {
            this.canceled = true;
        }
        #endregion

        // Block size to download is by default 1K.
        private const int downloadBlockSize = 1024;

        #region Events
        /// <summary>
        /// Progress update
        /// </summary>
        public event EventHandler<DownloadProgressEventArgs> ProgressChanged;
        protected void OnProgressChanged(long current, long target)
        {
            var handler = ProgressChanged;
            if (handler != null)
                handler(this, new DownloadProgressEventArgs(target, current));
        }

        /// <summary>
        /// Fired when progress reaches 100%.
        /// </summary>
        public event EventHandler Downloaded;
        protected void OnDownloaded()
        {
            var handler = Downloaded;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion

        #region Download methods
        /// <summary>
        /// Begin downloading the file at the specified url, and save it to destFileName.
        /// </summary>
        public void Download(string URL, string destinationDirectory, string destinationFilename = "")
        {
            DownloadData data = null;
            this.canceled = false;
            this.totalDownloaded = 0;

            try
            {
                // get download details                
                data = DownloadData.Create(URL, destinationFilename, this.proxy);

                // Find out the name of the file that the web server gave us.
                if (destinationFilename == null || destinationFilename.Length == 0)
                    destinationFilename = Path.GetFileName(data.Response.ResponseUri.ToString());
                this.destinationFilename = destinationFilename;

                // The place we're downloading to (not from) must not be a URI,
                // because Path and File don't handle them...
                //destFolder = destFolder.Replace("file:///", "").Replace("file://", "");
                this.destinationFilename = destinationFilename.Replace("file:///", "").Replace("file://", "");
                this.destinationFilename = destinationFilename;

                if (this.destinationFilename.Length == 0) this.destinationFilename = "index.html";

                int readCount;
                string targetPath = string.IsNullOrWhiteSpace(destinationDirectory) ?
                    PathExtensions.CombineWithExecutablePath(this.destinationFilename) :
                    Path.Combine(destinationDirectory, this.destinationFilename);

                using (FileStream f = File.Open(targetPath, FileMode.Create, FileAccess.Write))
                {
                    // create the download buffer
                    byte[] buffer = new byte[downloadBlockSize];
                    // update how many bytes have already been read
                    totalDownloaded = data.StartPoint;

                    while ((readCount = data.DownloadStream.Read(buffer, 0, downloadBlockSize)) > 0)
                    {
                        // break on cancel (the variable could be set AFTER retrieving the buffer)
                        if (canceled) break;

                        // update total bytes read
                        totalDownloaded += readCount;

                        // save block to end of file
                        SaveToFile(f, buffer, readCount);

                        // send progress info
                        if (data.IsProgressKnown) OnProgressChanged(totalDownloaded, data.FileSize);

                        // break on cancel (the variable could be set AFTER saving operations)
                        if (canceled) break;
                    }
                }

                if (!canceled) OnDownloaded(); else data.Close();
            }
            catch (UriFormatException e)
            {
                throw new ArgumentException(
                    String.Format("Could not parse the URL \"{0}\" - it's either malformed or is an unknown protocol.", URL), e);
            }
            catch (ArgumentException a)
            {
                throw a;
            }
            finally
            {
                if (data != null) data.Close();
            }
        }

        /// <summary>
        /// Asynchronously download a file from the url to the destination.
        /// </summary>
        public void AsyncDownload(string URL, string destinationDirectory, string destinationFileName = "")
        {
            ThreadPool.QueueUserWorkItem(
                new WaitCallback(this.WaitCallbackMethod),
                new string[] { URL, destinationDirectory, destinationFileName });
        }

        /// <summary>
        /// A WaitCallback used by the AsyncDownload methods.
        /// </summary>
        private void WaitCallbackMethod(object data)
        {
            String[] strings = data as String[];
            this.Download(strings[0], strings[1], strings[2]);
        }

        private void SaveToFile(FileStream f, byte[] buffer, int count)
        {
            try
            {
                f.Write(buffer, 0, count);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(
                    String.Format("Error trying to save file \"{0}\": {1}", this.destinationFilename, e.Message), e);
            }
        }
        #endregion
    }

}