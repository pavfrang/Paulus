using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Numerical
{
    /// <summary>
    /// Represents a pair (x, y).
    /// </summary>
    public struct XY
    {
        public double x, y;
        public XY(double x, double y) { this.x = x; this.y = y; }

        public bool IsValid()
        {
            return !double.IsNaN(x) && !double.IsNaN(y);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
        public string ToString(IFormatProvider provider)
        {
            string sx = x.ToString(provider);
            string sy = y.ToString(provider);
            return $"({sx}, {sy})";

        }

        public string ToString(string xFormat, string fxFormat)
        {
            string sx = x.ToString(xFormat);
            string sy = y.ToString(fxFormat);
            return $"({sx}, {sy})";
        }
        public string ToString(string xFormat, string fxFormat, IFormatProvider provider)
        {
            string sx = x.ToString(xFormat, provider);
            string sy = y.ToString(fxFormat, provider);
            return $"({sx}, {sy})";
        }
    }
}
