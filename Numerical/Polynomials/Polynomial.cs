using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Numerical.Polynomials
{
    public class Polynomial
    {
        #region Constructors
        /// <summary>
        /// Creates a zero order polynomial.
        /// </summary>
        /// <param name="c"></param>
        public Polynomial(double c) : this(true, c)
        { }

        ///// <summary>
        ///// Creates the first order polynomial ax+b.
        ///// </summary>
        ///// <param name="a"></param>
        //public Polynomial(double a, double b) : this(false, a, b)
        //{ }

        ///// <summary>
        ///// Creates the second order polynomial ax²+bx+c.
        ///// </summary>
        ///// <param name="a"></param>
        //public Polynomial(double a, double b, double c) : this(false, a, b, c)
        //{ }


        /// <summary>
        /// Create a polynomial from values in descending power order.
        /// </summary>
        /// <param name="a"></param>
        public Polynomial(params double[] a) //ok
             : this(true, (IEnumerable<double>)a)
        { }

        public Polynomial(bool inAscendingPowerOrder, params double[] a) //ok
            : this(inAscendingPowerOrder, (IEnumerable<double>)a)
        { }
        public Polynomial(bool inAscendingPowerOrder, IEnumerable<double> a) //ok
        {
            this.a = inAscendingPowerOrder ?
                a.ToArray() :
                a.Reverse().ToArray();
            trimLeadingZeros();
        }



        /// <summary>
        /// Trims leading order zeros.
        /// </summary>
        protected void trimLeadingZeros()
        {
            //the length check must be first in order to avoid checking the second equality
            while (a.Length > 1 && a[a.Length - 1] == 0)
            {
                double[] newa = new double[a.Length - 1];
                Array.Copy(a, newa, a.Length - 1);
                a = newa;
            }
        }

        public static Polynomial Zero //ok
        { get { return new Polynomial(0.0); } }
        public static Polynomial One //ok
        { get { return new Polynomial(1.0); } }

        /// <summary>
        /// Get the 'x' monomial.
        /// </summary>
        public static Polynomial x
        { get { return new Polynomial(new double[] { 0.0, 1.0 }); } }


        public static Polynomial Monomial(int order, double coefficient = 1.0)
        {
            double[] tmp = new double[order + 1];
            tmp[order] = coefficient;
            return new Polynomial(tmp);

        }
        #endregion

        #region Special polynomials

        public static Polynomial Hermite(int n) //ok
        {
            if (n == 0) return One; //H0(x)=1
            if (n == 1) return 2.0 * x; //H1(x)=2x

            //n>=2
            //H[n](x) = -2(n-1)H[n-2](x) + 2xH[n-1](x)

            //recursive function is slower
            //return -2.0 * (n - 1.0) * Hermite(n - 2) + 2.0 * x * Hermite(n - 1);
            Polynomial hnminus2 = One;
            Polynomial hnminus1 = 2.0 * x;
            Polynomial hn = null;
            for (int i = 2; i <= n; i++)
            {
                hn = -2.0 * (i - 1.0) * hnminus2 + 2.0 * x * hnminus1;

                //store for next iteration
                hnminus2 = hnminus1;
                hnminus1 = hn;
            }

            return hn;
        }

        public static LagrangeInterpolationPolynomial Lagrange(double[] x, double[] y)
            => new LagrangeInterpolationPolynomial(x, y);

        public static LagrangeInterpolationPolynomial Lagrange(XY[] xy)
            => new LagrangeInterpolationPolynomial(xy);

       public static HermiteInterpolationPolynomial HermiteInterpolation(double[] x, double[] y, double[] dydx)
            => new HermiteInterpolationPolynomial(x, y,dydx);
        #endregion

        #region Coefficients, derivative, integration
        /// <summary>
        /// The internal coefficients array in ascending power order.
        /// p(x) = a(0) + a(1)*x + a(2)*x² + ... + a(n)*x^n
        /// </summary>
        public double[] a { get; protected set; }

        public int Degree { get { return a.Length - 1; } }

        public double this[int i] //ok
        {
            get { return a[i]; }
            set { a[i] = value; }
        }

        /// <summary>
        /// Evaluate for x.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public virtual double this[double x] //ok
        {
            get
            {
                int n = a.Length;
                double p = a[n - 1];
                for (int i = n - 2; i >= 0; i--)
                    p = p * x + a[i];
                return p;
            }
        }

        /// <summary>
        /// Returns an array of polynomial evaluations the corresponding x.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double[] this[double[] x]
        {
            get
            {
                int n = x.Length;
                double[] tmp = new double[n];
                for (int i = 0; i < n; i++)
                    tmp[i] = this[x[i]];
                return tmp;
            }
        }

        public Polynomial Derivative() //ok
        {
            int n = a.Length;
            double[] tmp = new double[n - 1];
            for (int i = n - 1; i >= 1; i--)
                tmp[i - 1] = a[i] * i;
            return new Polynomial(tmp);
        }


        /// <summary>
        /// Return the indefinite integral as a new polynomial.
        /// </summary>
        /// <param name="c">The integration constant.</param>
        /// <returns>The integrated polynomial.</returns>
        public Polynomial Integrate(double c = 0.0) //ok
        {
            int n = a.Length;
            double[] tmp = new double[n + 1];
            for (int i = n; i > 0; i--)
                tmp[i] = a[i - 1] / i;
            tmp[0] = c;

            return new Polynomial(tmp);
        }


        /// <summary>
        /// Return the definite integral from a to b.
        /// </summary>
        /// <param name="a">The upper limit of the integral.</param>
        /// <param name="b">The lower limit of the integral.</param>
        /// <returns></returns>
        public double Integrate(double a, double b)
        {
            Polynomial intp = Integrate();
            return intp[b] - intp[a];
        }

        #endregion


        #region Operators

        #region Addition

        public static Polynomial operator +(Polynomial p, Polynomial q) //ok
        {
            int n1 = p.a.Length, n2 = q.a.Length;
            double[] a1 = p.a, a2 = q.a;

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
            return new Polynomial(tmp);
        }

        public static Polynomial operator +(Polynomial p, double x) //ok
        {
            //return p + new Polynomial(x);
            double[] tmp = p.a.ToArray();
            if (x == 0) return new Polynomial(tmp);
            tmp[0] += x;
            return new Polynomial(tmp);
        }

        public static Polynomial operator +(double x, Polynomial p) //ok
        {
            //return p + new Polynomial(x);
            double[] tmp = p.a.ToArray();
            if (x == 0) return new Polynomial(tmp);
            tmp[0] += x;
            return new Polynomial(tmp);
        }

        #endregion

        #region Subtraction
        public static Polynomial operator -(Polynomial p, Polynomial q)
        {
            int n1 = p.a.Length, n2 = q.a.Length;
            double[] a1 = p.a, a2 = q.a;

            bool isN1Max = n1 >= n2;
            double[] tmp;
            if (isN1Max)
            {
                tmp = new double[n1];
                for (int i = 0; i < n2; i++)
                    tmp[i] = a1[i] - a2[i];
                for (int i = n2; i < n1; i++)
                    tmp[i] = a1[i];
            }
            else
            {
                tmp = new double[n2];
                for (int i = 0; i < n1; i++)
                    tmp[i] = a1[i] - a2[i];
                for (int i = n1; i < n2; i++)
                    tmp[i] = -a2[i];
            }
            return new Polynomial(tmp);
        }

        public static Polynomial operator -(Polynomial p, double x) //ok
        {
            //return p + new Polynomial(x);
            double[] tmp = p.a.ToArray();
            tmp[0] -= x;
            return new Polynomial(tmp);
        }

        public static Polynomial operator -(double x, Polynomial p) //ok
        {
            int n = p.a.Length;
            double[] tmp = new double[n], a = p.a;

            for (int i = 0; i < n; i++)
                tmp[i] = -a[i];
            tmp[0] += x;
            return new Polynomial(tmp);
        }

        public static Polynomial operator -(Polynomial p) //ok
        {
            //return -1.0 * p;
            int n = p.a.Length;
            double[] tmp = p.a.ToArray();
            for (int i = 0; i < n; i++)
                tmp[i] *= -1.0;
            return new Polynomial(tmp);
        }
        #endregion

        #region Multiplication

        public static Polynomial operator *(Polynomial p, Polynomial q)
        {
            int n1 = p.a.Length, n2 = q.a.Length;
            double[] a1 = p.a, a2 = q.a;

            double[] tmp = new double[n1 + n2 - 1];
            for (int i = 0; i < n1; i++)
                for (int j = 0; j < n2; j++)
                    tmp[i + j] += a1[i] * a2[j];
            return new Polynomial(tmp);
        }

        public static Polynomial operator *(Polynomial p, double[] a) =>
            p * new Polynomial(a);
        public static Polynomial operator *(double[] a, Polynomial p) =>
            new Polynomial(a) * p;

        public static Polynomial operator *(Polynomial p, double x) //ok
        {
            if (x == 0) return Zero;

            int n = p.a.Length;
            double[] tmp = p.a.ToArray();

            for (int i = 0; i < n; i++)
                tmp[i] *= x;
            return new Polynomial(tmp);
        }


        public static Polynomial operator *(double x, Polynomial p) //ok
        {
            if (x == 0) return Zero;

            int n = p.a.Length;
            double[] tmp = p.a.ToArray();

            for (int i = 0; i < n; i++)
                tmp[i] *= x;
            return new Polynomial(tmp);
        }

        #endregion

        #region Division
        /// <summary>
        /// Divides a polynomial a by the polynomial d. (Chapra p. 182)
        /// </summary>
        /// <returns>The quotient/remainder tuple.</returns>
        public static Tuple<Polynomial, Polynomial> operator /(Polynomial p, Polynomial d)
        {
            int n = p.a.Length, m = d.a.Length;

            if (m == 1) //constant number
                return new Tuple<Polynomial, Polynomial>(p / d[0], Zero);

            if (n < m)
                return new Tuple<Polynomial, Polynomial>(Zero, p);

            if (m == 2) //do a hornet-division (simple)
            {
                double toDivide = d.a[1];
                double t = toDivide != 1.0 ? -d.a[0] / toDivide : -d.a[0];

                // Divides a polynomial by x-t. (Chapra p. 181)
                // The quotient/remainder tuple.
                double[] q = new double[n - 1],
                    a = p.a;

                double r = a[n - 1];
                for (int i = n - 2; i >= 0; i--)
                {
                    q[i] = r;
                    r = a[i] + r * t;
                }

                Polynomial quotient = new Polynomial(q);
                if (toDivide != 1.0) quotient /= toDivide;

                return new Tuple<Polynomial, Polynomial>(quotient, new Polynomial(r));
            } //ok


            { //ok
                double[] r = p.a.ToArray();
                double[] q = new double[n - m + 1];

                for (int k = n - m; k >= 0; k--)
                {
                    q[k] = r[m + k - 1] / d[m - 1];
                    for (int j = m + k - 2; j >= k; j--) //could be m+k-1 for full
                        r[j] -= q[k] * d[j - k];
                }

                double[] r2 = new double[m - 1];
                Array.Copy(r, r2, m - 1); //r[0] to r[m-2]

                return new Tuple<Polynomial, Polynomial>(new Polynomial(q), new Polynomial(r2));
            }
        }

        public static Polynomial operator /(Polynomial p, double x)
        {
            int n = p.a.Length;
            double[] tmp = p.a.ToArray();
            for (int i = 0; i < n; i++)
                tmp[i] /= x;
            return new Polynomial(tmp);
        }

        #endregion

        #region Power

        public static Polynomial operator ^(Polynomial p, int n)
        {
            if (n == 0) return One;
            if (n == 1) return new Polynomial(p.a);

            Polynomial r = new Polynomial(p.a);
            for (int i = 2; i <= n; i++)
                r *= p;
            return r;
        }

        #endregion

        /// <summary>
        /// Returns the derivative of p.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Polynomial operator ~(Polynomial p)
        {
            return p.Derivative();
        }


        #endregion

        #region ToString

        //public static CultureInfo GreekCulture { get; } = CultureInfo.GetCultureInfo("el-GR");

        //public static CultureInfo AmericanCulture { get; } = CultureInfo.GetCultureInfo("en-US");

        public override string ToString()
        {
            return ToString(" ", "G");
        }

        //https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
        public string ToString(string separator, string format = "G")
        {
            return "[" + string.Join(separator, a.Select(d => d.ToString(format))) + "]";
        }

        public string ToString2(string format = "G")
        {
            int n = a.Length;
            if (n == 1) return a[0].ToString("G");

            string separator = "";
            return string.Join(separator,
                Enumerable.Range(0, a.Length).Select(i =>
                {
                    if (a[i] == 0) return "";

                    string sValue = "";
                    if (i > 0
                        && a.Reverse().Skip(n - i).Any(v => v != 0.0)) //if not last non-zero term
                    {
                        if (a[i] == 1.0)
                            sValue = " + ";
                        else if (a[i] == -1.0)
                            sValue = " - ";
                        else if (a[i] > 0)
                            sValue = " + " + a[i].ToString(format);
                        else //if (a[i] < 0)
                            sValue = " - " + Math.Abs(a[i]).ToString(format);
                    }
                    else // i==0 or last non-zero term

                    if (i == 0 || Math.Abs(a[i]) != 1.0)
                        sValue = a[i].ToString(format);
                    else if (a[i] == 1.0) //i>0 
                        sValue = "";
                    else //if (a[i] == -1.0)//i>0
                        sValue = "-";

                    //else // <0
                    //    sValue = a[i].ToString(format);

                    string sx;
                    switch (i)
                    {
                        default: sx = $"x^{i}"; break;
                        //case 3: sx = "x³"; break;
                        //case 2: sx = "x²"; break;
                        case 1: sx = "x"; break;
                        case 0: sx = ""; break;
                    }

                    return $"{sValue}{sx}";
                }));
        }

        #endregion

        #region Casts

        public static explicit operator Polynomial(double[] a)
        {
            return new Polynomial(a);
        }

        public static explicit operator Polynomial(double c)
        {
            return new Polynomial(c);
        }

        #endregion

        #region Roots

        //the program must be compiled with as x86 in order for this to work
        //Public Declare Sub jenkinstraub Lib "jenkinstraub.dll" (op As Double, degree As Long, zeror As Double, zeroi As Double, fail As Boolean)
        [DllImport("jenkinstraub.dll")]
        private static extern void jenkinstraub(ref double op, ref int degree, ref double zeror, ref double zeroi, ref int fail);

        public Complex[] Roots()
        {
            double[] op = a.Reverse().ToArray();
            int degree = Degree;
            double[] zeror = new double[Degree];
            double[] zeroi = new double[Degree];

            int fail = 0;

            jenkinstraub(ref op[0], ref degree, ref zeror[0], ref zeroi[0], ref fail);

            if (fail != 0) return null;

            //collect output results
            Complex[] roots = new Complex[Degree];

            for (int i = 0; i < degree; i++)
                roots[i] = new Complex(zeror[i], zeroi[i]);

            //order by real component
            return roots.OrderBy(c => c.Real).ToArray();
        }

        public Result NewtonRaphson(double x0, int multiplicity = 1)
            => NonLinearSolver.NewtonRaphson(this, x0, multiplicity);

        #endregion


    }
}
