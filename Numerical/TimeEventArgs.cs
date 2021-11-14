using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Numerical
{

    [Serializable]
    public class SolverEventArgs : EventArgs
    {

        public SolverEventArgs(double x, double[] y, double h)
        {
            this.x = x;
            this.y = y;
            this.h = h;
        }
        public double[] y { get; protected set; }

        public double x { get; protected set; }
        public double h { get; protected set; }
    }


}
