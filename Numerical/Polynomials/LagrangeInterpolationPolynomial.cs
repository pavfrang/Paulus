using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Numerical.Polynomials
{
    /// <summary>
    /// The class is also accessed via the Polynomial.Lagrange method.
    /// </summary>
    public class LagrangeInterpolationPolynomial : Polynomial
    {
        //build a lagrange polynomial from x,y pairs
        public LagrangeInterpolationPolynomial(double[] x, double[] y)
        {
            xs = x.ToArray();
            ys = y.ToArray();
            build();
        }

        public LagrangeInterpolationPolynomial(double[,] xy)
        {
            long count = xy.GetLongLength(0);
            xs = new double[count];
            ys = new double[count];
            for (int i = 0; i < count; i++)
            {
                xs[i] = xy[i, 0];
                ys[i] = xy[i, 1];
            }

            build();
        }

        public LagrangeInterpolationPolynomial(XY[] xy)
        {
            xs = xy.Select(p => p.x).ToArray();
            ys = xy.Select(p => p.y).ToArray();
            build();
        }

        //coordinate arrays for x and y
        double[] xs;
        double[] ys;

        /// <summary>
        /// The overriden method is needed in order to give the yi when x equals one of xs.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public override double this[double x]
        {
            get
            {
                int i = Array.IndexOf(xs, x);
                return i == -1 ? base[x] : ys[i];
            }
        }

        /// <summary>
        /// Generates the coefficients of the lagrange polynomial.
        /// </summary>
        private void build()
        {
            int n = xs.Length;

            Polynomial p = Zero;
            for (int i = 0; i < n; i++)
            {
                Polynomial L = new Polynomial(ys[i]);
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    L *= (x - xs[j]) / (xs[i] - xs[j]);
                }
                p += L;
            }

            a = p.a;
            trimLeadingZeros();
        }
    }
}
