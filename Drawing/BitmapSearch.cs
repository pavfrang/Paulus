using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Paulus.Drawing
{
    public static class BitmapSearch
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        static Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        public static Color GetColorAt(Point location)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return screenPixel.GetPixel(0, 0);
        }

        public static Point SearchForFirstColor(this Bitmap image, Color color)
        {
            Point pt;
            image.SearchForHorizontalIdentifier(new uint[] { (uint)color.ToArgb() }, out pt, null);

            return pt;
        }

        public static Bitmap GetPrimaryScreenBitmap()
        {
            //Create a new bitmap.
            Bitmap bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height,
                                           PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            using (Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot))
                // Take the screenshot from the upper left corner to the right bottom corner.
                gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                            Screen.PrimaryScreen.Bounds.Y,
                                            0,
                                            0,
                                            Screen.PrimaryScreen.Bounds.Size,
                                            CopyPixelOperation.SourceCopy);

            return bmpScreenshot;
        }

        public static Bitmap GetScreenBitmap(int screenIndex)
        {
            Screen s = Screen.AllScreens[screenIndex];

            //Create a new bitmap.
            Bitmap bmpScreenshot = new Bitmap(s.Bounds.Width, s.Bounds.Height,
                                           PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            using (Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot))

                // Take the screenshot from the upper left corner to the right bottom corner.
                gfxScreenshot.CopyFromScreen(s.Bounds.X,
                                            s.Bounds.Y,
                                            0,
                                            0,
                                            s.Bounds.Size,
                                            CopyPixelOperation.SourceCopy);

            return bmpScreenshot;

        }

        public static bool SearchForHorizontalIdentifier(this Bitmap image, uint[] identifier, out Point ptFound, Rectangle? subRect)
        {
            Rectangle rect = subRect.HasValue ? subRect.Value : new Rectangle(0, 0, image.Width, image.Height);

            BitmapData data = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);


            //            byte* DataPointer = (byte*)Data.Scan0;
            //10:  
            //11:         DataPointer = DataPointer + (y * Data.Stride) + (x * PixelSizeInBytes);
            //12:  
            //13:         if (PixelSizeInBytes == 3)
            //14:  
            //15:         {
            //16:  
            //17:             return Color.FromArgb(DataPointer[2], DataPointer[1], DataPointer[0]);
            //18:  
            //19:         }
            bool found = false;

            //works for 32-bit pixel format only
            int ymin = rect.Top, ymax = Math.Min(rect.Bottom, image.Height);
            int xmin = rect.Left, xmax = Math.Max(rect.Right, image.Width) - identifier.GetLength(0);

            int strideInPixels = data.Stride / 4; //4 bytes per pixel
            unsafe
            {
                uint* dataPointer = (uint*)data.Scan0;
                for (int y = ymin; y < ymax; y++)
                    for (int x = xmin; x < xmax; x++)
                    {
                        //works independently of the data.Stride sign
                        uint* pixelPointer = dataPointer + y * strideInPixels + x;
                        uint pixel = *pixelPointer;
                        bool firstPixelFound = pixel == identifier[0];
                        //bool firstPixelFound=image.GetPixel(x, y).ToArgb() == identifier[0];
                        if (firstPixelFound)
                        {
                            found = true; //assume that we have found the whole identifier from the beginning
                            for (int iColor = 1; iColor < identifier.Length; iColor++)
                            {
                                //pixelPointer++;
                                //pixel = dataPointer[yOffsetInPixels + x + iColor];
                                //pixel=image.GetPixel(x + iColor, y).ToArgb();
                                pixel = *(pixelPointer + iColor);
                                bool nextPixelFound = pixel == identifier[iColor];
                                if (!nextPixelFound)
                                {
                                    found = false; break;
                                }
                            }

                            if (found)
                            {
                                image.UnlockBits(data);
                                ptFound = new Point(x, y);
                                return true;
                            }
                        }
                    }
            }
            image.UnlockBits(data);
            ptFound = Point.Empty;
            return false;
        }

        public static bool SearchForRectangleIdentifier(this Bitmap image, int[,] identifier, out Point ptFound, Rectangle? subRect)
        {
            Rectangle rect = subRect.HasValue ? subRect.Value : new Rectangle(0, 0, image.Width, image.Height);

            bool found = false;

            int identifierWidth = identifier.GetLength(0);
            int identifierHeight = identifier.GetLength(1);


            for (int y = rect.Top; y < Math.Min(rect.Bottom, image.Height) - identifierHeight; y++)
                for (int x = rect.Left; x < Math.Min(rect.Right, image.Width) - identifierWidth; x++)
                {
                    if (image.GetPixel(x, y).ToArgb() == identifier[0, 0])
                    {
                        found = true;
                        for (int iColor = 1; iColor < identifier.Length; iColor++)
                            if (image.GetPixel(x + iColor, y).ToArgb() != identifier[iColor, 0])
                            { found = false; break; }
                        if (found)
                        {
                            ptFound = new Point(x, y);
                            return true;
                        }
                    }
                }
            ptFound = Point.Empty;
            return false;
        }

        internal static BitmapData LockImage(Bitmap bitmap)
        {
            return bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
               ImageLockMode.ReadWrite, bitmap.PixelFormat);
        }
    }
}
