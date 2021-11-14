using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows.Forms; //Control
using System.Drawing; //Bitmap
using System.Drawing.Imaging; //PixelFormat

namespace Paulus.Drawing
{
    /// <summary>
    /// Implements a memory bitmap, that assists in speed drawing (especially in combination with a double buffered control.
    /// </summary>
    /// <example>
    /// MemoryBitmap gridPainter;
    /// 
    ///  //initialize grid painter
    ///  ...
    ///  gridPainter = new MemoryBitmap(panel1, true);
    ///  gridPainter.Paint += gridPainter_Paint;
    ///  ...
    /// 
    /// //handle the paint event
    /// void gridPainter_Paint(object sender, PaintEventArgs e)
    /// {
    ///     cgrid.Draw(e.Graphics);
    /// }
    /// ...
    /// gridPainter.ResizeBitmap();
    /// ...
    /// </example>
    public class MemoryBitmap : IDisposable
    {
        #region Constructors
        public MemoryBitmap(Control control, bool autoResizeBitmap=true)
        {
            Control = control;
            AutoResetOnResize = autoResizeBitmap;
        }

        public MemoryBitmap() { }
        #endregion

        #region Properties
        protected Bitmap memory;
        /// <summary>
        /// Returns the bitmap that is in use.
        /// </summary>
        public Bitmap Bitmap { get { return memory; } }

        FastBitmap bitmapWrapper;
        public FastBitmap BitmapWrapper { get { return bitmapWrapper; } }

        private Control _control;
        /// <summary>
        /// The control to draw the rubber rectangle on.
        /// </summary>
        public Control Control
        {
            set
            {
                //if a previous control is attached then remove the handlers from the control events
                if (_control != null) detachHandlers();

                _control = value;
                if (_control != null)
                {
                    attachHandlers();
                    Reset();
                }
            }
            get { return _control; }
        }

        /// <summary>
        /// If set to true then the Bitmap is reset automatically each time the control is resized.
        /// </summary>
        public bool AutoResetOnResize { get; set; }
        #endregion

        #region Public methods

        /// <summary>
        /// Disposes current bitmap and resizes itself before creating a new one.
        /// </summary>
        public void Reset()
        {
            int w = _control.ClientRectangle.Width;
            int h = _control.ClientRectangle.Height;

            if (memory != null) memory.Dispose();
            memory = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            bitmapWrapper = new FastBitmap(memory);
        }
        #endregion

        #region Internal handlers and events
        private void attachHandlers()
        {
            _control.Paint += control_Paint;
            _control.Resize += control_Resize;
        }

        void control_Resize(object sender, EventArgs e)
        {
            if (AutoResetOnResize)
                Reset();
        }

        private void detachHandlers()
        {
            _control.Paint -= control_Paint;
            _control.Resize -= control_Resize;
        }
        void control_Paint(object sender, PaintEventArgs e)
        {
            //draw to memory first!
            using (Graphics gMemory = Graphics.FromImage(memory))
            {
                OnPaint(gMemory, e.ClipRectangle);

                e.Graphics.DrawImageUnscaled(memory, 0, 0);
            }

        }
        #endregion

        #region Public events
        /// <summary>
        /// The Paint event must be handled in order to enable the drawing operations of the MemoryBitmap.
        /// </summary>
        public event EventHandler<PaintEventArgs> Paint;
        protected void OnPaint(Graphics graphics, Rectangle clipRect)
        {
            var handler = Paint;
            if (handler != null) handler(this, new PaintEventArgs(graphics, clipRect));
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Disposes the internal memory bitmap.
        /// </summary>
        public void Dispose()
        {
            memory.Dispose();
        }

        void IDisposable.Dispose()
        {
            memory.Dispose();
        }
        #endregion
    }
}
