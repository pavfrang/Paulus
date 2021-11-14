using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Paulus.Drawing
{
    //protected  override void OnPaint(PaintEventArgs  e)
    //{
    //base.OnPaint(e);

    //GraphicsPath path = RoundedRectangle.Create(5, 5, 20, 20);
    //e.Graphics.DrawPath(Pens.Black, path);

    //path = RoundedRectangle.Create(30, 5, 40, 40, 5);
    //e.Graphics.FillPath(Brushes.Blue, path);

    //path = RoundedRectangle.Create(8, 50, 50, 50, 5);
    //e.Graphics.DrawPath(Pens.Black, path);

    //e.Graphics.SetClip(path);
    //using (Font f = new Font("Tahoma", 12, FontStyle.Bold))
    //e.Graphics.DrawString("Draw Me!!", f, Brushes.Red, 0, 70);
    //e.Graphics.ResetClip();

    //}


    public static class RoundedRectangle //GraphicsPath cannot be inherited
    {
        public enum Corners
        {
            None = 0, TopLeft = 1, TopRight = 2, BottomLeft = 4, BottomRight = 8,
            All = TopLeft | TopRight | BottomLeft | BottomRight
        }

        public static Region Create(int x, int y, int width, int height,
                                          int radius, Corners corners)
        {
            int xw = x + width, xwr = xw - radius;
            int yh = y + height, yhr = yh - radius;
            int xr = x + radius, yr = y + radius;
            int r2 = radius * 2;
            int xwr2 = xw - r2;
            int yhr2 = yh - r2;

            GraphicsPath p = new GraphicsPath();
            p.StartFigure();

            //Top Left Corner
            if ((Corners.TopLeft & corners) == Corners.TopLeft)
                p.AddArc(x, y, r2, r2, 180, 90);
            else
            {
                p.AddLine(x, yr, x, y);
                p.AddLine(x, y, xr, y);
            }

            //Top Edge
            p.AddLine(xr, y, xwr, y);

            //Top Right Corner
            if ((Corners.TopRight & corners) == Corners.TopRight)
                p.AddArc(xwr2, y, r2, r2, 270, 90);
            else
            {
                p.AddLine(xwr, y, xw, y);
                p.AddLine(xw, y, xw, yr);
            }

            //Right Edge
            p.AddLine(xw, yr, xw, yhr);

            //Bottom Right Corner
            if ((Corners.BottomRight & corners) == Corners.BottomRight)
                p.AddArc(xwr2, yhr2, r2, r2, 0, 90);
            else
            {
                p.AddLine(xw, yhr, xw, yh);
                p.AddLine(xw, yh, xwr, yh);
            }

            //Bottom Edge
            p.AddLine(xwr, yh, xr, yh);

            //Bottom Left Corner
            if ((Corners.BottomLeft & corners) == Corners.BottomLeft)
                p.AddArc(x, yhr2, r2, r2, 90, 90);
            else
            {
                p.AddLine(xr, yh, x, yh);
                p.AddLine(x, yh, x, yhr);
            }

            //Left Edge
            p.AddLine(x, yhr, x, yr);

            p.CloseFigure();

            //Create the Region
            Region outputRegion = new Region(p);

            //Clean up
            p.Dispose();

            //Finish
            return outputRegion;
        }

        public static Region Create(int x, int y, int width, int height)
        { return Create(x, y, width, height, 5); }

        public static Region Create(int x, int y, int width, int height, int radius)
        { return Create(x, y, width, height, radius, Corners.All); }

        public static Region Create(Rectangle rect, int radius, Corners c)
        { return Create(rect.X, rect.Y, rect.Width, rect.Height, radius, c); }

        public static Region Create(Rectangle rect, int radius)
        { return Create(rect.X, rect.Y, rect.Width, rect.Height, radius); }

        public static Region Create(Rectangle rect)
        { return Create(rect.X, rect.Y, rect.Width, rect.Height); }
    }
}