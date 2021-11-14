using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Algebra.LinearSystems
{
    public struct TridiagonalMatrix
    {
        public double[] a;
        public double[] b;
        public double[] c;

        public static TridiagonalMatrix CreateFromConstants(double aValues, double bValues, double cValues, int n)
        {
            return new TridiagonalMatrix
            {
                a = Enumerable.Repeat(aValues, n).ToArray(),
                b = Enumerable.Repeat(bValues, n).ToArray(),
                c = Enumerable.Repeat(cValues, n).ToArray(),
            };
        }

        //d is the vector of constants
        public double[] Solve(double[] d)
        {
            return Solve(a, b, c, d);
        }
        public static double[] Solve(double aValues, double bValues, double cValues, int n, double[] d)
        {
            return CreateFromConstants(aValues, bValues, cValues, n).Solve(d);
        }

        public static double[] Solve(double[] a, double[] b, double[] c, double[] d)
        {
            //Thomas algorithm
            //https://en.wikipedia.org/wiki/Tridiagonal_matrix_algorithm
            //https://www3.ul.ie/wlee/ms6021_thomas.pdf

            //(matlab code test -validated)
            //n = 10;
            //a = full(gallery('tridiag', n, -1, 2, -1));
            //b = (1:n)'
            //x = a\b

            int n = a.Length;
            //a (2..n)
            //b (1..n)
            //c (1..n-1)
            double[] c2 = new double[n - 1];
            double[] d2 = new double[n];
            c2[0] = c[0] / b[0];
            d2[0] = d[0] / b[0];
            //Parallel.For(1, n - 1, i =>
            for (int i = 1; i <= n - 2; i++)
            //Parallel.ForEach(Partitioner.Create(1,n-1,100000),range=>
            //{
            //    for (int i = range.Item1; i < range.Item2; i++)
            {
                double denom = b[i] - a[i] * c2[i - 1];
                c2[i] = c[i] / denom;
                d2[i] = (d[i] - a[i] * d2[i - 1]) / denom;
               }
            //});
            d2[n - 1] = (d[n - 1] - a[n - 1] * d2[n - 2]) / (b[n - 1] - a[n - 1] * c2[n - 2]);

            for (int i = n - 2; i >= 0; i--)
                d2[i] -= c2[i] * d2[i + 1];

            return d2; //d2 stores the final solution
        }

        public double[] SolveParallel(double[] d)
        {
            return SolveParallel(a, b, c, d);
        }

        public static double[] SolveParallel(double aValues, double bValues, double cValues, int n, double[] d)
        {
            return CreateFromConstants(aValues, bValues, cValues, n).SolveParallel(d);
        }

        public static double[] SolveParallel(double[] a, double[] b, double[] c, double[] d)
        {
            //Thomas algorithm
            //https://en.wikipedia.org/wiki/Tridiagonal_matrix_algorithm

            //(matlab code test -validated)
            //n = 10;
            //a = full(gallery('tridiag', n, -1, 2, -1));
            //b = (1:n)'
            //x = a\b

            int n = a.Length;
            //a (2..n)
            //b (1..n)
            //c (1..n-1)
            double[] c2 = new double[n - 1];
            double[] d2 = new double[n];
            c2[0] = c[0] / b[0];
            d2[0] = d[0] / b[0];
           // Parallel.For(1, n - 1, i =>
            Parallel.ForEach(Partitioner.Create(1,n-1,100000),range=>
            {
            for (int i = range.Item1; i < range.Item2; i++)
            {
                double denom = b[i] - a[i] * c2[i - 1];
                c2[i] = c[i] / denom;
                d2[i] = (d[i] - a[i] * d2[i - 1]) / denom;
                }
            });
            d2[n - 1] = (d[n - 1] - a[n - 1] * d2[n - 2]) / (b[n - 1] - a[n - 1] * c2[n - 2]);

            for (int i = n - 2; i >= 0; i--)
                d2[i] -= c2[i] * d2[i + 1];

            return d2; //d2 stores the final solution
        }

    }

}
