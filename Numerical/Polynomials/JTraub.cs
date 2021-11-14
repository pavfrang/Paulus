using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace Numerical.Polynomials
{
    public static class JTraub
    {
        //the program must be compiled with as x86 in order for this to work
        //Public Declare Sub jenkinstraub Lib "jenkinstraub.dll" (op As Double, degree As Long, zeror As Double, zeroi As Double, fail As Boolean)
        [DllImport("jenkinstraub.dll")]
        private static extern void jenkinstraub(ref double op, ref int degree, ref double zeror, ref double zeroi, ref int fail);

        public static void TestDll()
        {
            //var p = (x + 1.0) * (x - 2.0);
            var x = Polynomial.x;
            var p = Polynomial.Hermite(17);

            var roots = p.Roots();

            //for (int i = 0; i < p.Degree; i++)
            //    Console.WriteLine(roots[i]);

            Console.Write($"{roots[0].Real} | ");
            Console.WriteLine(p.NewtonRaphson(-5.0));
            Console.Write($"{roots[1].Real} | ");
            Console.WriteLine(p.NewtonRaphson(-4.0));
            Console.Write($"{roots[2].Real} | ");
            Console.WriteLine(p.NewtonRaphson(-3.4));
            Console.Write($"{roots[3].Real} | ");
            Console.WriteLine(p.NewtonRaphson(-2.7));

            //test(out d);

            //  double t = tf();
        }

        /// <summary>
        /// The global common block used by:
        /// rpoly, fxshffr, quadit, realit, calcsc, nextk, newest. 
        /// </summary>
        public struct Globals
        {
            //arrays of size '101'
            public double[] p, qp, k, qk, svk;

            public double sr, si, u, v, a, b, c, d,
                  a1, a2, a3, a6, a7, e, f, g, h, szr, szi,
                  lzr, lzi;

            #region Machine constants
            //The following statements set machine constants used in various parts of the program.
            //The values below correspond to the Burroughs B6700.

            /// <summary>
            /// The base of the floating-point number system used.
            /// </summary>
            public const float @base = 8.0f;

            /// <summary>
            /// The maximum relative representation error which can be described as the smallest positive floating point number such that 1.D0+eta is greater than 1.
            /// </summary>
            public static readonly double eta = 0.5 * Pow(@base, 1 - 26);

            /// <summary>
            /// The largest floating-point number.
            /// </summary>
            public const double infin = 4.3e68;

            /// <summary>
            /// The smallest positive floating-point number. If the exponent range differs in single and double precision then smalno and infin should indicate the smaller range.
            /// </summary>
            public const double smalno = 1e-45;

            public static readonly double lo = smalno / eta;

            #endregion

            /// <summary>
            /// The unit error in addition.
            /// </summary>
            public static readonly double are = eta;
            /// <summary>
            /// The unit error in multiplication.
            /// </summary>
            public static readonly double mre = eta;

            /// <summary>
            /// Order of the polynomial. 
            /// </summary>
            public int n;
            /// <summary>
            /// Number of p, qp, k, qk svk elements. It equals to n+1.
            /// </summary>
            public int nn;
        }

        /// <summary>
        /// Variable-shift h polynomial iteration for a real zero.
        /// </summary>
        /// <param name="sss">Starting iterate.</param>
        /// <param name="nz">Number of zero found.</param>
        /// <param name="iflag">Flag to indicate a pair of zeros near real axis.</param>
        public static void realit(ref double sss, out double nz, out int iflag, ref Globals g)
        {
            int nm1 =g.n - 1;
            nz = 0;
            double s = sss;
            iflag = 0;

            int j = 0;

            //Main loop
                //Evaluate p at s
                double pv = g.p[0];
                g.qp[0] = pv;
                for (int i = 1; i < g.nn; i++)
                {
                    pv = pv * s + g.p[i];
                    g.qp[i] = pv;
                }
                //Compute a rigorous bound on the error in evaluating p.
                double mp = Abs(pv);
                double ms = Abs(s);
                double ee = (Globals.mre / (Globals.are + Globals.mre)) * Abs(g.qp[0]);
                for (int i = 1; i < g.nn; i++)
                    ee = ee * ms + Abs(g.qp[i]);

                //Iteration has converged sufficiently if the polynomial value is less than 20 times this bound.
                if (mp <= 20.0 * (Globals.are + Globals.mre) * ee - Globals.mre * mp)
                {
                    nz = 1;
                    g.szr = s;
                    g.szi = 0.0;
                    return;
                }

            //Stop iteration after 10 steps.


            ////A cluster of zeros near the real axis has been encountered return with iflag set to initiate a quadratic iteration.
            //if (j<2)
            //{
            //    iflag = 1;
            //    sss = s;
            //    return;
            //}

       if (j > 10) return;



    }

        public static void calcsc(out int type, ref Globals g)
        {
            //Synthetic division of k by the quadratic 1,u,v
            quadsd(g.n, g.u, g.v, g.k, out g.qk, out g.c, out g.d);

            if (!(Abs(g.c) > Abs(g.k[g.n - 1]) * 100.0 * Globals.eta ||
                Abs(g.d) > Abs(g.k[g.n - 2]) * 100.0 * Globals.eta))
                //Type=3 indicates the quadratic is almost a factor.
                type = 3;
            else if (Abs(g.d) >= Abs(g.c))
            {
                //Type=2 indicates that all formulas are divided by d.
                type = 2;
                g.e = g.a / g.d;
                g.f = g.c / g.d;
                g.g = g.u * g.b;
                g.h = g.v * g.b;
                g.a3 = (g.a + g.g) * g.e + g.h * (g.b / g.d);
                g.a1 = g.b * g.f - g.a;
                g.a7 = (g.f + g.u) * g.a + g.h;
            }
            else
            {
                //Type=1 indicates that all formulas are divided by c
                type = 1;
                g.e = g.a / g.c;
                g.f = g.d / g.c;
                g.g = g.u * g.e;
                g.h = g.v * g.b;
                g.a3 = g.a * g.e + (g.h / g.c + g.g) * g.b;
                g.a1 = g.b - g.a * (g.d / g.c);
                g.a7 = g.a + g.g * g.d + g.h * g.f;
            }
        }

        /// <summary>
        /// Computes the next k polynomials using scalars computed in calcsc.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="g"></param>
        public static void nextk(int type, ref Globals g)
        {
            if (type == 3)
            {
                g.k[0] = g.k[1] = 0.0;
                for (int i = 2; i < g.n; i++)
                    g.k[i] = g.qk[i - 2];
            }
            double temp = g.a;
            if (type == 1) temp = g.b;
            if (Abs(g.a1) <= Abs(temp) * Globals.eta * 10.0)
            {
                //If a1 is nearly zero then use a special form of the recurrence.
                g.k[0] = 0.0;
                g.k[1] = -g.a7 * g.qp[0];
                for (int i = 2; i < g.n; i++)
                    g.k[i] = g.a3 * g.qk[i - 2] - g.a7 * g.qp[i - 1];
            }
            else
            //Use scaled form of the recurrence.
            {
                g.a7 /= g.a1;
                g.a3 /= g.a1;
                g.k[0] = g.qp[0];
                g.k[1] = g.qp[1] - g.a7 * g.qp[0];
                for (int i = 2; i < g.n; i++)
                    g.k[i] = g.a3 * g.qk[i - 2] - g.a7 * g.qp[i - 1] + g.qp[i];
            }
        }

        /// <summary>
        /// Compute new estimates of the quadratic coefficients using the scalars computed in calcsc.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="uu"></param>
        /// <param name="vv"></param>
        public static void newest(int type, out double uu, out double vv, ref Globals g)
        {
            //Use formulas appropriate to setting of type.
            if (type == 3) //if type=3 the quadratic is zero
            {
                uu = vv = 0.0;
                return;
            }

            double a4, a5;
            if (type == 2)
            {
                a4 = (g.a + g.g) * g.f + g.h;
                a5 = (g.f + g.u) * g.c + g.v * g.d;
            }
            else
            {
                a4 = g.a + g.u * g.b + g.h * g.f;
                a5 = g.c + (g.u + g.v * g.f) * g.d;
            }

            //Evaluate new quadratic coefficients.
            double b1 = -g.k[g.n - 1] / g.p[g.nn - 1];
            double b2 = -(g.k[g.n - 2] + b1 * g.p[g.n - 1]) / g.p[g.nn - 1];
            double c1 = g.v * b2 * g.a1;
            double c2 = b1 * g.a7;
            double c3 = b1 * b1 * g.a3;
            double c4 = c1 - c2 - c3;
            double temp = a5 + b1 * a4 - c4;
            if (temp == 0.0)
                uu = vv = 0.0;
            else
            {
                uu = g.u - (g.u * (c3 + c2) + g.v * (b1 * g.a1 + b2 * g.a7)) / temp;
                vv = g.v * (1.0 + c4 / temp);
            }
        }

        //P: unclear and not verified by matlab - it should be p/(x^2+ux+v)
        //P: q seems correct q(1 to nn-2) and a. However, b is always wrong (and possibly is not used in the generic algorithm)
        /// <summary>
        /// Divides p by the quadratic  1, u, v  placing the quotient in q and the remainder in a,b.
        /// </summary>
        public static void quadsd(int nn, //order of nn
            double u, double v,
            double[] p,//1 to NN
            out double[] q, //1 to NN
            out double a, out double b)
        {
            //q = new double[nn - 2]; //P: this is the correct
            q = new double[nn];
            b = p[0];
            q[0] = b;
            a = p[1] - u * b;
            q[1] = a;

            for (int i = 2; i < nn; i++)
            {
                double c = p[i] - u * a - v * b;
                q[i] = c; //P: this is redundant!
                b = a;
                a = c;
            }
        }

        #region Quad
        /// <summary>
        /// Calculate the zeros of the quadratic a* z**2+b1* z+c.
        /// The quadratic formula, modified to avoid overflow, is used to find the larger zero if the zeros are real and both zeros are complex.
        /// The smaller real zero is found directly from the product of the zeros c/a.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b1"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Tuple<Complex, Complex> quad(double a, double b1, double c)
        {
            double sr, si, lr, li;
            quad(a, b1, c, out sr, out si, out lr, out li);
            return new Tuple<Complex, Complex>(
                new Complex(sr, si), new Complex(lr, li));
        }

        //rewritten quad function without the gotos

        /// <summary>
        /// Calculate the zeros of the quadratic a* z**2+b1* z+c.
        /// The quadratic formula, modified to avoid overflow, is used to find the larger zero if the zeros are real and both zeros are complex.
        /// The smaller real zero is found directly from the product of the zeros c/a.
        /// </summary>
        public static void quad(double a, double b1, double c,
               out double sr, out double si, out double lr, out double li)
        {
            if (a == 0.0)
            {
                sr = 0.0;
                if (b1 != 0.0) sr = -c / b1;
                lr = 0.0;
                si = li = 0.0;
                return;
            }

            //a != 0.0
            if (c == 0.0)
            {
                sr = 0.0;
                lr = -b1 / a;
                si = li = 0.0;
                return;
            }

            //COMPUTE DISCRIMINANT AVOIDING OVERFLOW
            double b = b1 / 2.0;
            double d, e;
            if (Abs(b) >= Abs(c))
            {
                e = 1.0 - (a / b) * (c / b);
                d = Sqrt(Abs(e)) * Abs(b);
            }
            else
            {
                e = a;
                if (c < 0.0) e = -a;
                e = b * (b / Abs(c)) - e;
                d = Sqrt(Abs(e)) * Sqrt(Abs(c));
            }

            if (e < 0.0) //complex conjugate zeros
            {
                sr = -b / a;
                lr = sr;
                si = Abs(d / a);
                li = -si;
                return;
            }

            //real zeros
            if (b >= 0.0) d = -d;
            lr = (-b + d) / a;
            sr = 0.0;
            if (lr != 0.0) sr = c / lr / a;
            si = li = 0.0;
        }

        //original quad function

        // CALCULATE THE ZEROS OF THE QUADRATIC A*Z**2+B1* Z+C.
        // THE QUADRATIC FORMULA, MODIFIED TO AVOID
        // OVERFLOW, IS USED TO FIND THE LARGER ZERO IF THE
        // ZEROS ARE REAL AND BOTH ZEROS ARE COMPLEX.
        // THE SMALLER REAL ZERO IS FOUND DIRECTLY FROM THE
        // PRODUCT OF THE ZEROS C/A.
        public static void quad_original(double a, double b1, double c,
                out double sr, out double si, out double lr, out double li)
        {
            double b, d, e; //, dabs, dsqrt; external functions
            if (a != 0.0) goto l20;

            sr = 0.0;
            if (b1 != 0.0) sr = -c / b1;
            lr = 0.0;

            l10:
            si = li = 0.0;
            return;

            l20:
            if (c != 0.0) goto l30;
            sr = 0.0;
            lr = -b1 / a;
            goto l10;

            //COMPUTE DISCRIMINANT AVOIDING OVERFLOW
            l30:
            b = b1 / 2.0;
            if (Abs(b) < Abs(c)) goto l40;
            e = 1.0 - (a / b) * (c / b);
            d = Sqrt(Abs(e)) * Abs(b);
            goto l50;

            l40:
            e = a;
            if (c < 0.0) e = -a;
            e = b * (b / Abs(c)) - e;
            d = Sqrt(Abs(e)) * Sqrt(Abs(c));

            l50:
            if (e < 0.0) goto l60;
            //real zeros
            if (b >= 0.0) d = -d;
            lr = (-b + d) / a;
            sr = 0.0;
            if (lr != 0.0) sr = c / lr / a;
            goto l10;

            //complex conjugate zeros
            l60:
            sr = -b / a;
            lr = sr;
            si = Abs(d / a);
            li = -si;
        }
        #endregion
    }
}
