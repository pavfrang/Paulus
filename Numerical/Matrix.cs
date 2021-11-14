using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Numerical
{
    public class Matrix : ICloneable
    {
        //TODO: Determinant.
        //TODO: Inverse.
        public Matrix(double[,] a)
        {
            this.a = a;
        }
        public Matrix(int m, int n)
        {
            this.a = new double[m, n];
        }

        double[,] a;

        public static Matrix Zero(int m, int n)
        {
            return new Matrix(m, n);
        }

        public static Matrix Eye(int n)
        {
            return new Matrix(eye(n, n));
        }

        public override string ToString()
        {
            return ToString("G", "\t");
        }

        //public string ToString(string separator)
        //{
        //    return ToString(a, separator);
        //}
        public string ToString(string format, string separator)
        {
            return ToString(a, format, separator);
        }

        public double this[int i, int j]
        {
            get { return a[i, j]; }
            set { a[i, j] = value; }
        }

        public Matrix Transpose()
        {
            return new Matrix(transpose(a));
        }

        #region Operators

        public static implicit operator Matrix(double[,] a)
            => new Matrix(a);

        public static explicit operator double[,] (Matrix m)
            => m.a;

        public static Matrix operator *(Matrix a, Matrix b)
        {
            return new Matrix(matmul(a.a, b.a));
        }
        public static Matrix operator *(Matrix a, double k)
        {
            return new Matrix(mult(a.a, k));
        }
        public static Matrix operator *(double k, Matrix a)
        {
            return new Matrix(mult(a.a, k));
        }
        public static Matrix operator +(Matrix a, Matrix b)
        {
            return new Matrix(add(a.a, b.a));
        }
        public static Matrix operator +(Matrix a, double k)
        {
            return new Matrix(add(a.a, k));
        }
        public static Matrix operator +(double k, Matrix a)
        {
            return new Matrix(add(a.a, k));
        }

        public static Matrix operator -(Matrix a, Matrix b)
        {
            return new Matrix(subtract(a.a, b.a));
        }
        public static Matrix operator -(Matrix a, double k)
        {
            return new Matrix(subtract(a.a, k));
        }
        public static Matrix operator -(double k, Matrix a)
        {
            return new Matrix(subtract(k, a.a));
        }
        public static Matrix operator -(Matrix a)
        {
            return new Matrix(opposite(a.a));
        }

        public static Matrix operator ^(Matrix a, int n) //i>=1
        {
            //Matrix m = new Matrix(a.a.Clone() as double[,]);
            //for (int i = 2; i <= n; i++)
            //    m *= a;
            //return m;
            return new Matrix(pow(a.a, n));
        }

        /// <summary>
        /// Transposes matrix.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Matrix operator ~(Matrix a)
        {
            return a.Transpose();
        }

        #endregion

        public object Clone()
        {
            return new Matrix(a.Clone() as double[,]);
        }

        #region Fast array only operations

        public static double[,] matmul(double[,] a, double[,] b)
        {
            //a [mxk], b [kxn]
            int m = a.GetLength(0);
            int k = a.GetLength(1);
            int n = b.GetLength(1);

            double[,] r = new double[m, n];

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                {
                    double s = 0.0;
                    for (int l = 0; l < k; l++)
                        s += a[i, l] * b[l, j];
                    r[i, j] = s;
                }
            return r;
        }

        public static double[] matmul(double[,] a, double[] v)
        {
            //a [mxk], b [kx1]
            int m = a.GetLength(0);
            int k = a.GetLength(1);

            double[] r = new double[m];

            for (int i = 0; i < m; i++)
            {
                double s = 0.0;
                for (int l = 0; l < k; l++)
                    s += a[i, l] * v[l];
                r[i] = s;
            }
            return r;
        }

        public static double[,] mult(double[,] a, double k)
        {
            if (k == 1) return a.Clone() as double[,];

            int m = a.GetLength(0);
            int n = a.GetLength(1);

            double[,] r = new double[m, n];
            if (k == 0) return r;
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    r[i, j] = k * a[i, j];
            return r;
        }




        public static double[,] pow(double[,] a, int n)
        {
            double[,] m = a.Clone() as double[,];
            for (int i = 2; i <= n; i++)
                m = matmul(m, a);
            return m;
        }

        public static double[,] add(double[,] a, double[,] b)
        {
            int m = a.GetLength(0);
            int n = a.GetLength(1);

            double[,] r = new double[m, n];

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    r[i, j] = a[i, j] + b[i, j];
            return r;
        }

        public static double[,] add(double[,] a, double k)
        {
            if (k == 0) return a.Clone() as double[,];

            int m = a.GetLength(0);
            int n = a.GetLength(1);

            double[,] r = new double[m, n];

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    r[i, j] = a[i, j] + k;
            return r;
        }

        public static double[,] subtract(double[,] a, double[,] b)
        {
            int m = a.GetLength(0);
            int n = a.GetLength(1);

            double[,] r = new double[m, n];

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    r[i, j] = a[i, j] - b[i, j];
            return r;
        }
        public static double[,] subtract(double[,] a, double k)
        {
            if (k == 0) return a.Clone() as double[,];

            int m = a.GetLength(0);
            int n = a.GetLength(1);

            double[,] r = new double[m, n];

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    r[i, j] = a[i, j] - k;
            return r;
        }

        public static double[,] subtract(double k, double[,] a)
        {
            int m = a.GetLength(0);
            int n = a.GetLength(1);

            double[,] r = new double[m, n];

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    r[i, j] = k - a[i, j];
            return r;
        }
        public static double[,] opposite(double[,] a)
        {
            int m = a.GetLength(0);
            int n = a.GetLength(1);

            double[,] r = new double[m, n];

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    r[i, j] = -a[i, j];
            return r;
        }


        public static double[,] transpose(double[,] a)
        {
            int m = a.GetLength(0);
            int n = a.GetLength(1);

            double[,] r = new double[n, m];

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    r[i, j] = a[j, i];
            return r;
        }
        public static double[,] eye(int m, int n)
        {
            double[,] r = new double[m, n];
            for (int i = 0; i < Math.Min(m, n); i++)
                r[i, i] = 1.0;
            return r;
        }

        //https://en.wikipedia.org/wiki/Outer_product
        public static double[,] outer(double[] u, double[] v)
        {
            int m = u.Length;
            int n = v.Length;
            double[,] ret = new double[m, n];
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    ret[i, j] = u[i] * v[j];
            return ret;
        }

        public static double[] diag(double[,] a)
        {
            int n = Math.Min(a.GetLength(0), a.GetLength(1));
            double[] ret = new double[n];
            for (int i = 0; i < n; i++)
                ret[i] = a[i, i];
            return ret;
        }

        #region Vector operations
        public static double[] add(double[] u, double[] v)
        {
            int m = u.GetLength(0);

            double[] r = new double[m];

            for (int i = 0; i < m; i++)
                r[i] = u[i] + v[i];
            return r;
        }
        public static double[] add(double[] u, double k)
        {
            if (k == 0.0) return (double[])u.Clone();

            int m = u.GetLength(0);

            double[] r = new double[m];

            for (int i = 0; i < m; i++)
                r[i] = u[i] + k;
            return r;
        }
        public static double[] add(double k, double[] u)
        {
            if (k == 0.0) return (double[])u.Clone();

            int m = u.GetLength(0);

            double[] r = new double[m];

            for (int i = 0; i < m; i++)
                r[i] = u[i] + k;
            return r;
        }

        public static double[] subtract(double[] u, double[] v)
        {
            int m = u.GetLength(0);

            double[] r = new double[m];

            for (int i = 0; i < m; i++)
                r[i] = u[i] - v[i];
            return r;
        }

        public static double[] mult(double[] v, double k)
        {
            if (k == 1) return v.Clone() as double[];
            int m = v.GetLength(0);
            double[] r = new double[m];
            if (k == 0) return r;
            for (int i = 0; i < m; i++)
                r[i] = k * v[i];
            return r;
        }
        public static double[] mult(double[] u, double[] v)
        {
            int m = v.GetLength(0);
            double[] r = new double[m];
            for (int i = 0; i < m; i++)
                r[i] = u[i] * v[i];
            return r;
        }

        public static double[] mult(double k, double[] v)
        {
            if (k == 1) return v.Clone() as double[];
            int m = v.GetLength(0);
            double[] r = new double[m];
            if (k == 0) return r;
            for (int i = 0; i < m; i++)
                r[i] = k * v[i];
            return r;
        }
        public static double[] opposite(double[] v)
        {
            int m = v.GetLength(0);

            double[] r = new double[m];

            for (int i = 0; i < m; i++)
                r[i] = -v[i];
            return r;
        }
        public static double[] abs(double[] v)
        {
            int n = v.Length;
            double[] ret = new double[n];
            for (int i = 0; i < n; i++)
                ret[i] = Math.Abs(v[i]);
            return ret;
        }

        public static double dotProduct(double[] u, double[] v)
        {
            //we assume that u and v have the same dimensions
            int n = u.Length;
            double s = 0.0;
            for (int i = 0; i < n; i++)
                s += u[i] * v[i];
            return s;
        }

        public static double norm(double[] x)
        {
            //L^2 norm of X
            double s = 0.0;
            for (int i = 0; i < x.Length; i++)
                s += x[i] * x[i];
            return Math.Sqrt(s);
        }

        #endregion


        public static string ToString(double[,] a)
        {
            return ToString(a, "G", "\t");
        }
        public static string ToString(double[,] a, string format, string separator)
        {
            int m = a.GetLength(0);
            int n = a.GetLength(1);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < m - 1; i++)
            {
                for (int j = 0; j < n - 1; j++)
                    sb.Append(a[i, j].ToString(format) + separator);
                sb.AppendLine(a[i, n - 1].ToString(format));
            }
            for (int j = 0; j < n - 1; j++)
                sb.Append(a[m - 1, j].ToString(format) + separator);
            sb.Append(a[m - 1, n - 1].ToString(format));

            return sb.ToString();
        }
        #endregion

    }
}