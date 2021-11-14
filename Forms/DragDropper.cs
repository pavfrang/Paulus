using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Paulus.Forms
{
    public class FileDragDropper : DragDropper
    {
        public FileDragDropper(Control control,
            DragDropEffects dragDropEffect = DragDropEffects.Move, bool enableNow = true)
            : base(control, DataFormats.FileDrop, dragDropEffect, enableNow)
        { }

        public event EventHandler<FilesEventArgs> DragDropFiles;

        protected void OnDragDropFiles(string[] files)
        {
            var handler = DragDropFiles;
            if (handler != null) handler(this, new FilesEventArgs(files));
        }

        protected override void control_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(_dataFormat))
            {
                string[] files = (string[])e.Data.GetData(_dataFormat);
                OnDragDropFiles(files);
            }
        }
    }


    [Serializable]
    public class FilesEventArgs : EventArgs
    {
        public FilesEventArgs(string[] files) 
        {
            _files = files;
        }

        protected string[] _files;
        public string[] Files { get { return _files; } }
    }


    public class DragDropper : IDisposable
    {
        //use the System.Windows.Forms.DataFormats static class for the dataFormat
        public DragDropper(Control control, string dataFormat,
            DragDropEffects dragDropEffect = DragDropEffects.Move, bool enableNow = true)
        {
            _control = control;
            _dataFormat = dataFormat;
            _dragDropEffect = dragDropEffect;

            control.DragEnter += control_DragEnter;
            control.DragDrop += control_DragDrop;

            control.AllowDrop = enableNow;
        }

        protected DragDropEffects _dragDropEffect;
        public DragDropEffects DragDropEffect
        {
            get { return _dragDropEffect; }
            set { _dragDropEffect = value; }
        }


        protected string _dataFormat;
        public string DataFormat
        {
            get { return _dataFormat; }
        }

        protected Control _control;
        public Control Control { get { return _control; } }

        public void Enable() { _control.AllowDrop = true; }

        public void Disable() { _control.AllowDrop = false; }

        protected virtual void control_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(_dataFormat))
            {
                //string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                //foreach (string file in files)
                //    MessageBox.Show(file);
            }
        }

        protected virtual void control_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(_dataFormat))
                e.Effect = _dragDropEffect;
        }


        #region IDisposable
        public void Dispose()
        {
            _control.DragDrop -= this.control_DragDrop;
            _control.DragEnter -= this.control_DragEnter;
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }
        #endregion
    }
}
