using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Reflection;
using System.Drawing;


namespace Paulus.Forms
{
    public static class ControlExtensions
    {
        /// <summary>
        /// Changes the style or a combination of style flags of the control.
        /// </summary>
        /// <param name="control">The target control to change the style of.</param>
        /// <param name="styles">The style flags.</param>
        /// <param name="enable">If true then the style is enabled, else it is disabled.</param>
        public static void SetCustomStyles(this Control control, ControlStyles styles, bool enable = true)
        {
            control.GetType().GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, new object[] { styles, enable });
        }

        /// <summary>
        /// Enables double buffering for the control. 
        /// </summary>
        /// <param name="control">The target control.</param>
        public static void EnableDoubleBuffer(this Control control)
        {
            control.GetType().GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control,
                new object[] { ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true });
            //control.SetCustomStyles(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer);
        }


        #region Custom drawing and scale

        //should review the use of page/world coordinate systems
        public static PointF ToCustomCoordinates(this Control control, Point location, RectangleF canvas)
        {
            return new PointF(
                location.X / (float)control.ClientRectangle.Width * canvas.Width + canvas.Left,
                location.Y / (float)control.ClientRectangle.Height * canvas.Height + canvas.Top);
        }

        public static PointF ToLocalCoordinates(this Control control, PointF location, RectangleF canvas)
        {
            return new PointF(
                (location.X - canvas.Left) * (float)control.ClientRectangle.Width / canvas.Width,
                (location.Y - canvas.Top) * (float)control.ClientRectangle.Height / canvas.Height);
        }
        public static RectangleF ToLocalCoordinates(this Control control, RectangleF rectangle, RectangleF canvas, bool alignToCenter)
        {
            PointF newPoint = new PointF(
                (rectangle.X - canvas.Left) * (float)control.ClientRectangle.Width / canvas.Width,
                (rectangle.Y - canvas.Top) * (float)control.ClientRectangle.Height / canvas.Height);

            SizeF newSize = new SizeF(
                rectangle.Width / canvas.Width * (float)control.ClientRectangle.Width,
                rectangle.Height / canvas.Height * (float)control.ClientRectangle.Height
                );

            RectangleF convertedRectangle = new RectangleF(newPoint, newSize);

            if (alignToCenter) convertedRectangle.Offset(-newSize.Width / 2.0f, -newSize.Height / 2.0f);

            return convertedRectangle;
        }

        public static void DrawAxisX(this Control control, Graphics graphics, Pen pen, RectangleF canvas)
        {
            PointF left = control.ToLocalCoordinates(new PointF(canvas.Left, 0.0f), canvas);
            PointF right = control.ToLocalCoordinates(new PointF(canvas.Right, 0.0f), canvas);

            graphics.DrawLine(pen, left, right);
        }
        public static void DrawGridLinesX(this Control control, Graphics graphics, Pen pen, RectangleF canvas, float yInterval)
        {
            int linesCount = (int)Math.Abs(Math.Round(canvas.Height / (float)yInterval));

            if (canvas.Bottom < canvas.Top)
            {
                for (int iLine = 0; iLine < linesCount; iLine++)
                {
                    PointF left = control.ToLocalCoordinates(new PointF(canvas.Left, canvas.Bottom + iLine * yInterval), canvas);
                    PointF right = control.ToLocalCoordinates(new PointF(canvas.Right, canvas.Bottom + iLine * yInterval), canvas);

                    graphics.DrawLine(pen, left, right);
                }
            }
            else
            {
                for (int iLine = 0; iLine < linesCount; iLine++)
                {
                    PointF left = control.ToLocalCoordinates(new PointF(canvas.Left, canvas.Top + iLine * yInterval), canvas);
                    PointF right = control.ToLocalCoordinates(new PointF(canvas.Right, canvas.Top + iLine * yInterval), canvas);

                    graphics.DrawLine(pen, left, right);
                }

            }
        }

        public static void DrawGridLinesY(this Control control, Graphics graphics, Pen pen, RectangleF canvas, float xInterval)
        {
            int linesCount = (int)Math.Abs(Math.Round(canvas.Width / (float)xInterval));

            if (canvas.Left < canvas.Right)
            {
                for (int iLine = 0; iLine < linesCount; iLine++)
                {
                    PointF top = control.ToLocalCoordinates(new PointF(canvas.Left + iLine * xInterval, canvas.Top), canvas);
                    PointF bottom = control.ToLocalCoordinates(new PointF(canvas.Left + iLine * xInterval, canvas.Bottom), canvas);

                    graphics.DrawLine(pen, top, bottom);
                }
            }
            else
            {
                for (int iLine = 0; iLine < linesCount; iLine++)
                {
                    PointF top = control.ToLocalCoordinates(new PointF(canvas.Right + iLine * xInterval, canvas.Top), canvas);
                    PointF bottom = control.ToLocalCoordinates(new PointF(canvas.Right + iLine * xInterval, canvas.Bottom), canvas);

                    graphics.DrawLine(pen, top, bottom);
                }

            }
        }


        public static void DrawAxisY(this Control control, Graphics graphics, Pen pen, RectangleF canvas)
        {
            PointF bottom = control.ToLocalCoordinates(new PointF(0.0f, canvas.Bottom), canvas);
            PointF top = control.ToLocalCoordinates(new PointF(0.0f, canvas.Top), canvas);

            graphics.DrawLine(pen, bottom, top);
        }



        #endregion

    }
}
