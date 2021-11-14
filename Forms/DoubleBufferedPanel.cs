using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paulus.Forms
{
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            ResizeRedraw = true;
            DoubleBuffered = true;

            // or

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);

            //UpdateStyles();
        }
    }
}
