using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using System.Windows.Forms;
using Paulus.Drawing;

namespace Paulus.Forms
{
    public abstract class AreaSelector : IDisposable
    {
        #region Constructors
        public AreaSelector(Control control)
        {
            this.Control = control;
        }

        public AreaSelector() { }
        #endregion

        #region Public properties
        protected Control _control;
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
                    //set the most common size parameters for speed
                    width = _control.ClientRectangle.Width;
                    height = _control.ClientRectangle.Height;
                }
            }
            get { return _control; }
        }

        protected bool _enabled;
        public bool Enabled { get { return _enabled; } set { _enabled = value; } }

        protected Rectangle _selection;
        public Rectangle Selection { get { return _selection; } }

        protected bool _startedSelecting;
        public bool StartedSelecting { get { return _startedSelecting; } }

        protected int _imageBackgroundAlpha = 200;
        /// <summary>
        /// Gets or sets the background image opacity. 0 is for transparent and 255 for opaque. Default value is 200.
        /// </summary>
        public int ImageBackgroundAlpha { get { return _imageBackgroundAlpha; } set { _imageBackgroundAlpha = value; } }

        private bool isBackgroundOpaque { get { return _imageBackgroundAlpha == 255; } }


        protected MouseButtons _mouseButton = MouseButtons.Left;
        /// <summary>
        /// Gets or sets the  mouse button that will start the selection. Default value is MouseButtons.Left.
        /// </summary>
        public MouseButtons MouseButton { get { return _mouseButton; } set { _mouseButton = value; } }
        #endregion

        private void attachHandlers()
        {
            _control.MouseDown += control_MouseDown;
            _control.MouseMove += control_MouseMove;
            _control.MouseUp += control_MouseUp;
            _control.Resize += control_Resize;
        }
        private void detachHandlers()
        {
            _control.MouseDown -= control_MouseDown;
            _control.MouseMove -= control_MouseMove;
            _control.MouseUp -= control_MouseUp;
            _control.Resize -= control_Resize;
        }


        //start and last points of the selection rectangle
        protected Point pStart, pLast;


        //width and height of the client rectangle are set when setting the control and by the resize event
        protected int width, height;
        
        void control_Resize(object sender, EventArgs e)
        {
            width = _control.ClientRectangle.Width;
            height = _control.ClientRectangle.Height;
        } 
        
        //internal bitmaps
        protected Bitmap originalImage, transparentOriginalImage, selectionBitmap;


        protected virtual void control_MouseDown(object sender, MouseEventArgs e)
        {

            if (_enabled && e.Button == _mouseButton)
            {
                //set starting point and selection status
                _startedSelecting = true;
                pStart = e.Location;

                //copy original image of control
                originalImage = new Bitmap(width, height);
                _control.DrawToBitmap(originalImage, _control.ClientRectangle);

                //create transparent copy of bitmap
                if (!isBackgroundOpaque)
                    transparentOriginalImage = originalImage.ToImageWithChangedAlpha(_imageBackgroundAlpha);

                //initialize bitmap to store the selection bitmap data
                selectionBitmap = new Bitmap(width, height);

                //redraw background if the background bitmap is transparent
                if (!isBackgroundOpaque)
                    using (Graphics gControl = Graphics.FromHwnd(_control.Handle))
                        gControl.DrawImageUnscaled(transparentOriginalImage, 0, 0);
            }
        }

        /// <summary>
        /// Returns a normalized rectangle based on the current pStart and pLast points.
        /// </summary>
        private Rectangle getNormalizedRectange(Point pStart, Point pLast, bool constrainToClientSpace = true)
        {
            Point topLeft = new Point(Math.Min(pStart.X, pLast.X), Math.Min(pStart.Y, pLast.Y));
            Point bottomRight = new Point(Math.Max(pStart.X, pLast.X), Math.Max(pStart.Y, pLast.Y));
            const bool constraint = true;
            if (constraint)
            {
                if (topLeft.X < 0) topLeft.X = 0;
                if (topLeft.Y < 0) topLeft.Y = 0;
                if (bottomRight.X > _control.ClientRectangle.Right) bottomRight.X = _control.ClientRectangle.Right;
                if (bottomRight.Y > _control.ClientRectangle.Bottom) bottomRight.Y = _control.ClientRectangle.Bottom;
            }
            Size size = new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
            return new Rectangle(topLeft, size);
        }

        protected virtual void control_MouseMove(object sender, MouseEventArgs e)
        {
            if (_enabled && e.Button == _mouseButton)
            {
                //set last point
                pLast = e.Location;

                //set the current selection
                _selection = getNormalizedRectange(pStart, pLast, true);

                using (Graphics g = Graphics.FromImage(selectionBitmap))
                {
                    g.Clear(SystemColors.ButtonFace);

                    g.DrawImageUnscaled(transparentOriginalImage, 0,0,width,height);

                    //here the inheriter must draw the selection rectangle
                    OnSelecting(g, _selection);

                    //draw selection to control
                    using (Graphics gControl = Graphics.FromHwnd(_control.Handle))
                        gControl.DrawImageUnscaled(selectionBitmap, 0, 0);
                }
            }
        }

        protected virtual void control_MouseUp(object sender, MouseEventArgs e)
        {
            if (_enabled && e.Button == _mouseButton)
            {
                _startedSelecting = false;

                //set last point
                pLast = e.Location;

                //set the current selection
                _selection = getNormalizedRectange(pStart, pLast, true);

                OnSelected(_selection);
            }
        }

        #region Public events
        public event EventHandler<PaintEventArgs> Selecting;
        protected virtual void OnSelecting(Graphics graphics, Rectangle selection)
        {
            var handler = Selecting;
            if (handler != null) handler(this, new PaintEventArgs(graphics, selection));
        }

        public event EventHandler<RectangleSelectionEventArgs> Selected;
        protected virtual void OnSelected(Rectangle selection)
        {
            var handler = Selected;
            if (handler != null) handler(this, new RectangleSelectionEventArgs(selection));
        }
        #endregion

        #region Dispose
        public virtual void Dispose()
        {
            if (_control != null)
                detachHandlers();
        }
        void IDisposable.Dispose()
        {
            Dispose();
        }
        #endregion

    }
}
