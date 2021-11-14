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
    public class RectangleAreaSelector : AreaSelector
    {
        #region Constructors
        public RectangleAreaSelector(Control control) : base(control) { SelectorPenColor = Color.ForestGreen; }
        public RectangleAreaSelector() { SelectorPenColor = Color.ForestGreen;}
        #endregion

        #region Properties
        protected Brush brush;
        protected Pen pen;
        protected Color _selectorColor;
        public Color SelectorPenColor
        {
            get { return _selectorColor; }
            set
            {
                _selectorColor = value;
                if (brush != null) brush.Dispose();
                if (pen != null) pen.Dispose();

                //high transparency solid brush
                brush = new SolidBrush(Color.FromArgb(50, _selectorColor));
                //high opacity pen
                pen = new Pen(Color.FromArgb(240, _selectorColor));
                pen.Width = 2;
                pen.DashStyle = global::System.Drawing.Drawing2D.DashStyle.Dash;
            }
        }

        #endregion

        #region Internal handlers and events

        protected override void OnSelecting(Graphics graphics, Rectangle selection)
        {
            graphics.FillRectangle(brush, selection);

            graphics.DrawRectangle(pen, selection);

            //this should be called to allow user to hook to the event
            base.OnSelecting(graphics, selection);
        }


        #endregion

        #region Dispose
        public override void Dispose()
        {
            base.Dispose();

            if (brush != null) brush.Dispose();
            if (pen != null) pen.Dispose();
        }
        #endregion





    }
}
