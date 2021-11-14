using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Numerical.Polynomials
{
    public class HermiteInterpolationPolynomial : Polynomial
    {
        /// <summary>
        /// Build a hermite interpolation polynomial from x, y, dydx pairs
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="dydx"></param>
        public HermiteInterpolationPolynomial(double[] x, double[] y, double[] dydx)
        {
            xs = x.ToArray();
            ys = y.ToArray();
            dydxs = dydx.ToArray();
            build();
        }

        public HermiteInterpolationPolynomial(double[,] xydydx)
        {
            long count = xydydx.GetLongLength(0);
            xs = new double[count];
            ys = new double[count];
            dydxs = new double[count];

            for (int i = 0; i < count; i++)
            {
                xs[i] = xydydx[i,0];
                ys[i] = xydydx[i,1];
                dydxs[i] = xydydx[i, 2];
            }
            build();
        }

        //coordinate arrays for x and y
        double[] xs;
        double[] ys;
        double[] dydxs;

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
        /// Generates the coefficients of the hermite interpolation polynomial.
        /// </summary>
        private void build()
        {
            int n = xs.Length;

            Polynomial p = Zero;
            //Ferziger p. 12
            for (int k = 0; k < n; k++)
            {

                //build the L polynomial coefficient for
                Polynomial L = Polynomial.One;
                for (int j = 0; j < n; j++)
                {
                    if (k == j) continue;
                    L *= (x - xs[j]) / (xs[k] - xs[j]);
                }

                Polynomial L2 = L ^ 2;


                //Ferziger p. 12
                //(~L) is the derivative
                Polynomial U = (1.0 - 2 * (~L)[xs[k]] * (x - xs[k])) * L2;
                Polynomial V = (x - xs[k]) * L2;

                p += U * ys[k] + V * dydxs[k];
            }

            a = p.a;
            trimLeadingZeros();
        }
    }
}
