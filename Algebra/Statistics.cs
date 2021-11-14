using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Algebra
{
    public static class Statistics
    {
        //modified version of:
        //https://www.johndcook.com/blog/csharp_erf/
        public static double Erf(double x)
        {
            // constants
            const double a1 = 0.254829592;
            const double a2 = -0.284496736;
            const double a3 = 1.421413741;
            const double a4 = -1.453152027;
            const double a5 = 1.061405429;
            const double p = 0.3275911;

            // Save the sign of x
            //erf(-x)=-erfx
            double sign = x >= 0 ? 1.0 : -1.0;
            if (x < 0)
                x = -x;

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            return sign * (1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x));
        }

        public static double ErfSmallX(double x) //used for 0<=x<=2
        {
            const double TWOOVERSQRTPI = 1.1283791670955125738961589031215;
            double sign = x >= 0 ? 1.0 : -1.0;
            if (x < 0)
                x = -x;
            return sign * TWOOVERSQRTPI * Math.Exp(-x * x); //?? wrong?
        }

        public static double CumulativeNormal(double x, double mean, double standardDeviation)
        {
            const double ONEOVERSQRT2 = 0.70710678118654752440084436210485;
            // A&S formula 7.1.22
            double z = (x - mean) / standardDeviation * ONEOVERSQRT2;

            return 0.5 * (1.0 + Erf(z));
            //return z <= 2 ? 0.5 * (1.0 + ErfSmallX(z)) : 0.5 * (1.0 + Erf(z));
        }
    }


}
