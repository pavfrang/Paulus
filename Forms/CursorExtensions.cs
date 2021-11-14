using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

using System.Windows.Forms;
using System.Drawing;


namespace Paulus.Forms
{
    public static class CursorExtensions
    {
        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        public static Cursor CreateCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            IntPtr ptr = bmp.GetHicon();
            IconInfo tmp = new IconInfo();
            GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            ptr = CreateIconIndirect(ref tmp);
            return new Cursor(ptr);
        }

        public static Cursor CreateSampleCursor()
        {
            Bitmap bitmap = new Bitmap(140, 25);
            Graphics g = Graphics.FromImage(bitmap);
            using (Font f = new Font("Arial",10))
                g.DrawString("{ } Switch On The Code", f, Brushes.Black, 0, 0);

            Cursor ret = CreateCursor(bitmap, 10, 3);

            bitmap.Dispose();

            return ret;
        }
    }
}
