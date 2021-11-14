using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

using System.Windows.Forms;

namespace Paulus.Forms
{
    /// <summary>
    /// Enables the selection of an area on a control, by drawing a rubber rectangle.
    /// </summary>
    /// <example>
    /// RectangleSelector selector;
    /// ...
    /// //initialize selector
    /// selector = new RectangleSelector(panel1);
    /// selector.Selected += selector_RectangleSelected;
    /// ...
    /// //enable it
    /// selector.Enabled = true;
    /// ...
    /// void selector_RectangleSelected(object sender, EventArgs e)
    /// {
    ///   //custom example
    ///   Rectangle s = selector.Selection;
    ///   ...
    /// }
    /// </example>
    public class RubberRectangleSelector : IDisposable
    {
        #region Constructors
        public RubberRectangleSelector(Control control)
        {
            this.Control = control;
        }

        public RubberRectangleSelector() { }
        #endregion

        #region Public properties
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
                if (_control != null) attachHandlers();
            }
            get { return _control; }
        }

        private bool _enabled;
        public bool Enabled { get { return _enabled; } set { _enabled = value; } }

        private Rectangle _selection;
        public Rectangle Selection { get { return _selection; } }
        #endregion

        #region Internal handlers and events
        bool startedTracking;
        Point pStart = new Point();
        Point pLast = new Point();

        private void detachHandlers()
        {
            _control.MouseUp -= control_MouseUp;
            _control.MouseDown -= control_MouseDown;
            _control.MouseMove -= control_MouseMove;
        }

        private void attachHandlers()
        {
            _control.MouseUp += control_MouseUp;
            _control.MouseDown += control_MouseDown;
            _control.MouseMove += control_MouseMove;

        }

        private void control_MouseMove(object sender, MouseEventArgs e)
        {
            if (_enabled)
            {
                Point ptCurrent = new Point(e.X, e.Y);
                // If we "have the mouse", then we draw our lines.
                if (startedTracking)
                {
                    // If we have drawn previously, draw again in
                    // that spot to remove the lines.
                    if (pLast.X != -1)
                    {
                        drawReversibleRectangle(pStart, pLast);
                    }
                    // Update last point.
                    pLast = ptCurrent;
                    // Draw new lines.
                    drawReversibleRectangle(pStart, ptCurrent);
                }
            }
        }

        private void control_MouseDown(object sender, MouseEventArgs e)
        {
            if (_enabled)
            {
                // Make a note that we "have the mouse".
                startedTracking = true;
                // Store the "starting point" for this rubber-band rectangle.
                pStart = new Point(e.X, e.Y);
                // Special value lets us know that no previous
                // rectangle needs to be erased.
                pLast.X = -1;
                pLast.Y = -1;

            }
        }

        private void control_MouseUp(object sender, MouseEventArgs e)
        {
            if (_enabled)
            {
                // Set internal flag to know we no longer "have the mouse".
                startedTracking = false;
                // If we have drawn previously, draw again in that spot
                // to remove the lines.
                if (pLast.X != -1)
                {
                    Point ptCurrent = new Point(e.X, e.Y);
                    drawReversibleRectangle(pStart, pLast);

                    //set the last rectangle
                    _selection = getNormalizedRectangle(pStart, pLast);
                    OnSelected();
                }
                // Set flags to know that there is no "previous" line to reverse.
                pLast.X = -1;
                pLast.Y = -1;
                pStart.X = -1;
                pStart.Y = -1;
            }
        }

        /// <summary>
        /// Draws the reversible frame.
        /// </summary>
        /// <param name="p1">A corner of the frame in control coordinates.</param>
        /// <param name="p2">The opposite corner of p1 in control coordinates.</param>
        private void drawReversibleRectangle(Point p1, Point p2)
        {

            // Convert the points to screen coordinates.
            p1 = _control.PointToScreen(p1);
            p2 = _control.PointToScreen(p2);

            Rectangle rc = getNormalizedRectangle(p1, p2);

            // Draw the reversible frame.
            ControlPaint.DrawReversibleFrame(rc,
                            Color.Red, FrameStyle.Dashed);
        }

        /// <summary>
        /// Returns the produced rectangle in control coordinates.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        private Rectangle getNormalizedRectangle(Point p1, Point p2)
        {
            Rectangle rc = new Rectangle();

            // Normalize the rectangle.
            if (p1.X < p2.X)
            {
                rc.X = p1.X;
                rc.Width = p2.X - p1.X;
            }
            else
            {
                rc.X = p2.X;
                rc.Width = p1.X - p2.X;
            }
            if (p1.Y < p2.Y)
            {
                rc.Y = p1.Y;
                rc.Height = p2.Y - p1.Y;
            }
            else
            {
                rc.Y = p2.Y;
                rc.Height = p1.Y - p2.Y;
            }

            return rc;
        }
        #endregion

        #region Public events
        public event EventHandler Selected;
        protected void OnSelected()
        {
            var handler = Selected;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion

        #region Dispose
        public void Dispose()
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
