using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Numerical
{
    public static class Math2
    {
        /// <summary>
        /// Works for powers >=1.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double pow(double v, int x) //x>=1
        {
            double tmp = v;
            for (int i = 2; i <= x; i++) tmp *= v;
            return tmp;
        }

    }
}
