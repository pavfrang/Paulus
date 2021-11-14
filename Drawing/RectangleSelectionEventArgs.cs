using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Paulus.Drawing
{
    
    [Serializable]
    public class RectangleSelectionEventArgs : EventArgs
    {
        public RectangleSelectionEventArgs(Rectangle selection)
        {
            _selection = selection;
        }

        protected Rectangle _selection;
        public Rectangle Selection { get { return _selection; } }
    }

}
