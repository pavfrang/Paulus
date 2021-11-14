using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Numerical.Polynomials
{
    /// <summary>
    /// Static class for polynomials optimized for speed.
    /// </summary>
    public static class PolynomialMath
    {
        //Function names are the same with MATLAB.
        //The arrays are in ascending power order.

        #region Polynomial values and derivative values
        /// <summary>
        /// Calculates the value of the polynomial for x.
        /// (Chapra p. 179)
        /// </summary>
        /// <param name="a">The coefficients of the polynomial. The index of the polynomial corresponds to the power of x, i.e. a[3] corresponds to x³.</param>
        /// <param name="n">The size of the array. The degree of the polynomial is n-1.</param>
        /// <param name="x">The value to be evaluated.</param>
        public static double polyval(double[] a, int n, double x)
        {
            double p = a[n - 1];
            for (int i = n - 2; i >= 0; i--)
                p = p * x + a[i];
            return p;
        }

        public static double polyval(double[] a, double x) => polyval(a, a.Length, x);

        /// <summary>
        /// Calculates the value of the derivative of the polynomial for x.
        /// (Chapra p. 179)
        /// </summary>
        /// <param name="a">The coefficients of the polynomial. The index of the polynomial corresponds to the power of x, i.e. a[3] corresponds to x³.</param>
        /// <param name="n">The size of the array. The degree of the polynomial is n-1.</param>
        /// <param name="x">The value to be evaluated.</param>
        /// <param param name="poly">The value of the polynomial is optionally exported.</param>
        public static double polyder(double[] a, int n, double x, out double poly)
        {
            double p = a[n - 1]; //n>=1
            double df = 0;
            for (int i = n - 2; i >= 0; i--)
            {
                df = df * x + p;
                p = p * x + a[i];
            }
            poly = p;
            return df;
        }


        public static double polyder(double[] a, double x)
        {
            double p;
            return polyder(a, a.Length, x, out p);
        }


        /// <summary>
        /// Calculates the coefficient of the derivative of the polynomial.
        /// </summary>
        /// <param name="a">The coefficients of the polynomial. The index of the polynomial corresponds to the power of x, i.e. a[3] corresponds to x³.</param>
        /// <param name="n">The size of the array. The degree of the polynomial is n-1.</param>
        public static double[] polyder(double[] a, int n)
        {
            double[] tmp = new double[n - 1];
            for (int i = n - 1; i >= 1; i--)
                tmp[i - 1] = a[i] * i;
            return tmp;
        }

        public static double[] polyder(double[] a) =>
            polyder(a, a.Length);
        #endregion

        #region Addition
        public static double[] add(double[] a1, int n1, double[] a2, int n2)
        {
            bool isN1Max = n1 >= n2;
            double[] tmp;
            if (isN1Max)
            {
                tmp = new double[n1];
                for (int i = 0; i < n2; i++)
                    tmp[i] = a1[i] + a2[i];
                for (int i = n2; i < n1; i++)
                    tmp[i] = a1[i];
            }
            else
            {
                tmp = new double[n2];
                for (int i = 0; i < n1; i++)
                    tmp[i] = a1[i] + a2[i];
                for (int i = n1; i < n2; i++)
                    tmp[i] = a2[i];
            }
            return tmp;
        }

        public static double[] add(double[] a1, double[] a2) =>
            add(a1, a1.Length, a2, a2.Length);

        public static double[] add(double[] a, int n, double x)
        {
            double[] tmp = new double[n];
            for (int i = 0; i < n; i++)
                tmp[i] = a[i] + x;
            return tmp;
        }

        public static double[] add(double[] a, double x) =>
            add(a, a.Length, x);
        #endregion

        #region Multiplication
         public static double[] mult(double[] a, int n1, double[] b, int n2)
        {
            double[] tmp = new double[n1 + n2];
            for (int i = 0; i < n1; i++)
                for (int j = 0; j < n2; j++)
                    tmp[i + j] += a[i] * b[j];
            return tmp;
        }
       public static double[] mult(double[] a, int n, double x)
        {
            double[] tmp = new double[n];
            if (x == 0) return tmp;

            for (int i = 0; i < n; i++)
                tmp[i] = a[i] * x;
            return tmp;
        }

        public static double[] mult(double[] a, double x) =>
            mult(a, a.Length, x);


        public static double[] mult(double[] a, double[] b) =>
            mult(a, a.Length, b, b.Length);


        #endregion

        #region Create polynomials
        public static double[] xPower(int n, double coefficient = 1.0)
        {
            double[] tmp = new double[n + 1];
            tmp[n] = coefficient; return tmp;
        }

        /// <summary>
        /// Creates the first order polynomial ax+b.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="coefficient"></param>
        /// <returns></returns>
        public static double[] poly1(double a, double b)
        {
            return new double[] { b, a };
        }

        /// <summary>
        /// Creates the second order polynomial ax²+bx+c.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double[] poly2(double a, double b, double c)
        {
            return new double[] { c, b, a };
        }

        #endregion

        #region Division

        /// <summary>
        /// Divides a polynomial by x-t. (Chapra p. 181)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n">The length of a</param>
        /// <param name="t">Defines the monomial factor x-t.</param>
        /// <param name="r">The remainder.</param>
        /// <returns></returns>
        public static double[] div(double[] a, int n, double t, out double r)
        {
            double[] q = new double[n - 1];

            r = a[n - 1];
            for (int i = n - 2; i >= 0; i--)
            {
                q[i] = r;
                r = a[i] + r * t;
            }

            return q;
        }

        public static double[] div(double[] a, double t, out double r) =>
            div(a, a.Length, t, out r);

        /// <summary>
        /// Divides a polynomial a by the polynomial d. (Chapra p. 182)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n">The length of a</param>
        /// <param name="d">The divi</param>
        /// <param name="m">The length of d</param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static double[] div(double[] a, int n, double[] d, int m, out double[] r)
        {
            double[] q = new double[n - m + 1];
            r = (double[])a.Clone();
            //wrong
            for (int k = n - m - 1; k >= 0; k--)
            {
                q[k + 1] = r[m + k] / d[m - 1];
                for (int j = m + k - 1; j >= k; j--)
                    r[j] -= q[k + 1] * d[j - k];

            }
            for (int j = m; j < n; j++) r[j] = 0;
            return q;
        }

        #endregion

        #region ToString

        public static CultureInfo GreekCulture { get; } = CultureInfo.GetCultureInfo("el-GR");

        public static CultureInfo AmericanCulture { get; } = CultureInfo.GetCultureInfo("en-US");

        //https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
        public static string ToString(double[] a, string separator = " ", string format = "G")
        {
            return string.Join(separator, a.Select(d => d.ToString(format)));
        }

        public static string ToString2(double[] a, string separator = " ", string format = "G")
        {
            return string.Join(separator,
                Enumerable.Range(0, a.Length).Select(i =>
                 {
                     if (a[i] == 0) return "";

                     bool isNegative = a[i] < 0;
                     string sValue = a[i] != 1.0 || i == 0 ? a[i].ToString(format) : "";


                     string sx;
                     switch (i)
                     {
                         default: sx = $"x^{i}"; break;
                         //case 3: sx = "x³"; break;
                         case 2: sx = "x²"; break;
                         case 1: sx = "x"; break;
                         case 0: sx = ""; break;
                     }

                     return isNegative || i == 0 ? $"{sValue}{sx}" : $"+ {sValue}{sx}";
                 }));
        }

        #endregion

        public static double[] Hermite(int n)
        {
            if (n == 0) return new double[] { 1.0 }; //H0(x)=1
            if (n == 1) return new double[] { 0.0, 2.0 }; //H1(x)=2

            //n>2
            //H[n](x) = -2(n-1)H[n-2](x) + 2xH[n-1](x)
            return add(mult(Hermite(n - 2), -2.0 * (n - 1)), //-2(n-1)H[n-2](x)
                mult(Hermite(n - 1), new double[] { 0.0, +2.0 })); // 2xH[n-1](x)
        }
    }
}
