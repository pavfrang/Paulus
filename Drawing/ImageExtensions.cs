using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Drawing.Imaging;

namespace Paulus.Drawing
{
    public static class ImageExtensions
    {
        /// <summary>
        /// Returns an altered copy of an image with transparency.
        /// </summary>
        /// <param name="image">The image to be altered.</param>
        /// <param name="alpha">The alpha value is between 0 (transparent) and 255 (opaque/unaltered).</param>
        /// <returns></returns>
        public static Bitmap ToImageWithChangedAlpha(this Image image, int alpha)
        {
            int width = image.Width, height = image.Height;
            Bitmap targetImage = new Bitmap(width, height);

            //ColorMatrix: https://msdn.microsoft.com/en-us/library/vstudio/System.Drawing.Imaging.ColorMatrix(v=vs.100).aspx
            //ImageAttributes: https://msdn.microsoft.com/en-us/library/vstudio/system.drawing.imaging.imageattributes(v=vs.100).aspx

            //values less than 0 are practically empty
            if (alpha < 255 && alpha>0)
            {
                //initialize the identity matrix
                ColorMatrix colormatrix = new ColorMatrix();
                colormatrix.Matrix33 = (float)alpha / 255.0f;

                //initialize the image attributes and set the ColorMatrix information
                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                //apply the modifications to the image when drawing to the target bitmap
                using (Graphics graphics = Graphics.FromImage(targetImage))
                    graphics.DrawImage(image,
                        new Rectangle(0, 0, width, height), //destination rectangle
                        0, 0, //upper-left corner of source rectangle
                        width, //width of source rectangle
                        height, //height of source rectangle
                        GraphicsUnit.Pixel,
                        imageAttributes);
            }
            else if(alpha==255) //just create a clone
            {
                targetImage = (Bitmap)image.Clone();

                //using (Graphics graphics = Graphics.FromImage(targetImage))
                //    graphics.DrawImageUnscaled(image,
                //        0, 0, //upper-left corner of source rectangle
                //        width, height); //width and height of source rectangle
            }
            //else //for all invalid alpha values or alpha==0 return an empty bitmap


            return targetImage;
        }


        public static Bitmap GetPartialScreenshot(Point topLeft,Point bottomRight)
        {
            Size s = new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
            return GetPartialScreenshot(topLeft, s);
        }

        public static Bitmap GetPartialScreenshot(Point topLeft, Size size)
        {
            Bitmap bmp = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(bmp))
                g.CopyFromScreen(topLeft, Point.Empty, size, CopyPixelOperation.SourceCopy);
            return bmp;

        }
    }
}
