using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Math;

namespace Numerical
{
    public static class OdeSolver
    {
        public static double[,] RungeKutta2(
            Func<double, double, double> f, double x0, double y0,
            double h, double xn)
        {

            int intervals = (int)Round((xn - x0) / h);
            double[,] ret = new double[2, intervals + 1];
            ret[0, 0] = x0;
            ret[1, 0] = y0;

            double x = x0, y = y0;
            for (int i = 1; i <= intervals; i++)
            {
                double k1 = h * f(x, y);
                double k2 = h * f(x + 0.5 * h, y + 0.5 * k1);
                ret[0, i] = x0 + i * h; //x
                ret[1, i] = y + k2; //y

                x = ret[0, i];
                y = ret[1, i];
            }
            return ret;
        }

        public static double[,] RungeKutta4(
            Func<double, double, double> f, double x0, double y0,
            double h, double xn)
        {
            //return RungeKuttaGenericSolver.RungeKutta4.Solve(f, x0, y0, h, xn);

            //http://mathworld.wolfram.com/Runge-KuttaMethod.html
            int intervals = (int)Round((xn - x0) / h);
            double[,] ret = new double[2, intervals + 1];
            ret[0, 0] = x0;
            ret[1, 0] = y0;

            double x = x0, y = y0;
            for (int i = 1; i <= intervals; i++)
            {
                double k1 = h * f(x, y);
                double k2 = h * f(x + 0.5 * h, y + 0.5 * k1);
                double k3 = h * f(x + 0.5 * h, y + 0.5 * k2);
                double k4 = h * f(x + h, y + k3);
                ret[0, i] = x0 + i * h; //x
                ret[1, i] = y + k1 / 6.0 + k2 / 3.0 + k3 / 3.0 + k4 / 6.0; //y

                x = ret[0, i];
                y = ret[1, i];
            }
            return ret;
        }

     
    }
}
