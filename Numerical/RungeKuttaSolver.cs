using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static System.Math;

namespace Numerical
{
    public class RungeKuttaSolver
    {
        //https://en.wikipedia.org/wiki/List_of_Runge%E2%80%93Kutta_methods
        //Butcher Tableau vectors and matrix
        public double[] c;
        public double[] b;
        public double[,] a;

        //optional array which is used by embedded methods to produce an estimate of the local truncation error of a single Runge-Kutta step
        //https://en.wikipedia.org/wiki/List_of_Runge%E2%80%93Kutta_methods
        public double[] b2;
        public int hOrder;  //hOrder is needed for tolerance checking e.g. an order 4 method would have hOrder = 4

        public RungeKuttaSolver(double[,] a, double[] b, double[] c, double[] b2 = null, int hOrder = 0)
        {
            this.a = a; this.b = b; this.c = c; this.b2 = b2; this.hOrder = hOrder;
        }

        public static RungeKuttaSolver RungeKutta4
        {
            get
            {
                double[] c = new double[] { 0.0, 0.5, 0.5, 1.0 };
                double[] b = new double[] { 1 / 6.0, 1 / 3.0, 1 / 3.0, 1 / 6.0 };

                double[,] a = new double[4, 4];
                a[1, 0] = 0.5; a[2, 1] = 0.5; a[3, 2] = 1.0;

                return new RungeKuttaSolver(a, b, c);
            }
        } //ok

        public static RungeKuttaSolver RungeKutta3
        {
            get
            {
                double[] c = new double[] { 0.0, 0.5, 1.0 };
                double[] b = new double[] { 1.0 / 6.0, 2.0 / 3.0, 1.0 / 6.0 };

                double[,] a = new double[3, 3];
                a[1, 0] = 0.5; a[2, 0] = -1.0; a[2, 1] = 2.0;

                return new RungeKuttaSolver(a, b, c);
            }
        }

        /// <summary>
        /// Butcher method (Chapra p.737).
        /// </summary>
        public static RungeKuttaSolver Butcher5 //ok
        {
            get
            {
                double[] c = new double[] { 0.0, 0.25, 0.25, 0.5, 0.75, 1.0 };
                double[] b = new double[] { 7.0 / 90.0, 0.0, 16.0 / 45.0, 2.0 / 15.0, 16.0 / 45.0, 7.0 / 90.0 };
                double[,] a = new double[6, 6];
                a[1, 0] = 0.25;
                a[2, 0] = 0.125; a[2, 1] = 0.125;
                a[3, 1] = -0.5; a[3, 2] = 1.0;
                a[4, 0] = 3.0 / 16.0; a[4, 3] = 9.0 / 16.0;
                a[5, 0] = -3.0 / 7.0; a[5, 1] = 2.0 / 7.0; a[5, 2] = 12.0 / 7.0; a[5, 3] = -12.0 / 7.0; a[5, 4] = 8.0 / 7.0;

                return new RungeKuttaSolver(a, b, c);
            }
        }

        /// <summary>
        /// 7 stage 6th order RK method. Chammud method is more preferred (according to Alshina).
        /// </summary>
        public static RungeKuttaSolver Butcher6 //ok
        {
            get
            {
                //http://www.mymathlib.com/c_source/diffeq/runge_kutta/runge_kutta_butcher.c
                double[] c = new double[] { 0.0, 1.0 / 3.0, 2.0 / 3.0, 1.0 / 3.0, 0.5, 0.5, 1.0 };
                double[] b = new double[] { 11.0 / 120.0, 0.0, 81.0 / 120.0, 81.0 / 120.0, -32.0 / 120.0, -32.0 / 120.0, 11.0 / 120.0 };
                double[,] a = new double[7, 7];
                a[1, 0] = 1.0 / 3.0;
                a[2, 1] = 2.0 / 3.0;
                a[3, 0] = 1.0 / 12.0; a[3, 1] = 1.0 / 3.0; a[3, 2] = -1.0 / 12.0;
                a[4, 0] = -1.0 / 16.0; a[4, 1] = 9.0 / 8.0; a[4, 2] = -3.0 / 16.0; a[4, 3] = -3.0 / 8.0;
                a[5, 1] = 9.0 / 8.0; a[5, 2] = -3.0 / 8.0; a[5, 3] = -0.75; a[5, 4] = 0.5;
                a[6, 0] = 9.0 / 44.0; a[6, 1] = -9.0 / 11.0; a[6, 2] = 63.0 / 44.0; a[6, 3] = 18.0 / 11.0; a[6, 4] = -16.0 / 11.0;

                return new RungeKuttaSolver(a, b, c);
            }
        }


        public static RungeKuttaSolver Hammud6 //ok
        {
            get
            {
                //Optimal first- to sixth-order accurate Runge-Kutta schemes
                //https://link.springer.com/article/10.1134%2FS0965542508030068?LI=true

                //1st Chammud order 6 Runge-Kutta scheme
                //http://www.peterstone.name/Maplepgs/Maple/nmthds/RKcoeff/Runge_Kutta_schemes/RK6/RKcoeff6b_3.pdf

                //http://keisan.casio.com/calculator
                const double s5 = 2.236067977499789696409; //=sqrt(5)
                const double o7 = 0.1428571428571428571429; //=1/7
                double[] c = new double[] { 0.0, 4.0 * o7, 5.0 * o7, 6.0 * o7, 0.5 - 0.1 * s5, 0.5 + s5 * 0.1, 1.0 };
                double[] b = new double[] { 1.0 / 12.0, 0.0, 0.0, 0.0, 5.0 / 12.0, 5.0 / 12.0, 1.0 / 12.0 };

                double[,] a = new double[7, 7];
                a[1, 0] = 4.0 * o7;
                a[2, 0] = 115.0 / 112.0; a[2, 1] = -5.0 / 16.0;
                a[3, 0] = 589.0 / 630.0; a[3, 1] = 5.0 / 18.0; a[3, 2] = -16.0 / 45.0;

                a[4, 0] = 229.0 / 1200.0 - 29.0 * s5 / 6000.0; a[4, 1] = 119.0 / 240.0 - 187.0 * s5 / 1200.0;
                a[4, 2] = -14.0 / 75.0 + 34.0 * s5 / 375.0; a[4, 3] = -3.0 * s5 / 100.0;

                a[5, 0] = 71.0 / 2400.0 - 587.0 * s5 / 12000.0; a[5, 1] = 187.0 / 480.0 - 391.0 * s5 / 2400.0;
                a[5, 2] = -38.0 / 75.0 + 26.0 * s5 / 375.0; a[5, 3] = 27.0 / 80.0 - 3.0 * s5 / 400.0;
                a[5, 4] = 0.25 + 0.25 * s5;

                a[6, 0] = -49.0 / 480.0 + 43.0 * s5 / 160.0; a[6, 1] = -425.0 / 96.0 + 51.0 * s5 / 32.0;
                a[6, 2] = 52.0 / 15.0 - 4.0 * s5 / 5.0; a[6, 3] = -27.0 / 16.0 + 3.0 * s5 / 16.0;
                a[6, 4] = 1.25 - 0.75 * s5; a[6, 5] = 2.5 - 0.5 * s5;

                return new RungeKuttaSolver(a, b, c);
            }
        }

        #region Adaptive methods

        //https://en.wikipedia.org/wiki/List_of_Runge%E2%80%93Kutta_methods
        public static RungeKuttaSolver DormandPrince4_5
        {
            get
            {
                double[] c = new double[] { 0.0, 0.2, 0.3, 0.8, 8.0 / 9.0, 1.0, 1.0 };
                double[] b = new double[] { 35.0 / 384.0, 0.0, 500.0 / 1113.0, 125.0 / 192.0, -2187.0 / 6784.0, 11.0 / 84.0, 0.0 };
                double[] b2 = new double[] { 5179.0 / 57600.0, 0.0, 7571.0 / 16695.0, 393.0 / 640.0, -92097.0 / 339200.0, 187.0 / 2100.0, 1.0 / 40.0 };

                double[,] a = new double[7, 7];
                a[1, 0] = 0.2;
                a[2, 0] = 3.0 / 40.0; a[2, 1] = 9.0 / 40.0;
                a[3, 0] = 44.0 / 45.0; a[3, 1] = -56.0 / 15.0; a[3, 2] = 32.0 / 9.0;
                a[4, 0] = 19372.0 / 6561.0; a[4, 1] = -25360.0 / 2187.0; a[4, 2] = 64448.0 / 6561.0; a[4, 3] = -212.0 / 729.0;
                a[5, 0] = 9017.0 / 3168.0; a[5, 1] = -355.0 / 33.0; a[5, 2] = 46732.0 / 5247.0; a[5, 3] = 49.0 / 176.0; a[5, 4] = -5103.0 / 18656.0; //O(h5)
                a[6, 0] = 35.0 / 384.0; a[6, 2] = 500.0 / 1113.0; a[6, 3] = 125.0 / 192.0; a[6, 4] = -2187.0 / 6784.0; a[6, 5] = 11.0 / 84.0; //O(h4)

                return new RungeKuttaSolver(a, b, c, b2, 4);
            }
        }

        //https://en.wikipedia.org/wiki/List_of_Runge%E2%80%93Kutta_methods
        public static RungeKuttaSolver HeunEuler //adaptive
        {
            get
            {
                double[] c = new double[] { 0.0, 1.0 };
                double[] b = new double[] { 0.5, 0.5 };
                double[] b2 = new double[] { 1.0, 0.0 };

                double[,] a = new double[2, 2];
                a[1, 0] = 1.0;

                return new RungeKuttaSolver(a, b, c, b2, 2);

            }

        }

        #endregion

        //tolerance is used only by adaptive methods where b2 is defined
        public double[,] Solve(
            Func<double, double, double> f,
            double x0, double y0,
            double h, double xn, double tolerance = 1e-15)
        {
            double[,] ret;
            int order = c.Length; // = b.Length
            double[] k = new double[order];

            double x = x0, y = y0;

            if (b2 == null) //not adaptive control
            {
                int intervals = (int)Math.Round((xn - x0) / h);
                ret = new double[2, intervals + 1];
                ret[0, 0] = x0;
                ret[1, 0] = y0;
                for (int iStep = 1; iStep <= intervals; iStep++)
                {
                    double sumbk = 0.0;
                    for (int i = 0; i < order; i++)
                    {
                        double sumk = 0.0;
                        for (int j = 0; j <= i - 1; j++)
                            sumk += a[i, j] * k[j];

                        k[i] = f(x + c[i] * h, y + h * sumk);
                        sumbk += b[i] * k[i];
                    }

                    ret[0, iStep] = x0 + iStep * h; //x
                    ret[1, iStep] = y + h * sumbk; //y

                    x = ret[0, iStep];
                    y = ret[1, iStep];
                }
            }
            else //with adaptive control
            {
                //check for a maximum value of steps?
                int points = 1;
                List<Tuple<double, double>> values = new List<Tuple<double, double>>();

                //add the initial values to the output array
                values.Add(new Tuple<double, double>(x0, y0));
                while (x < xn)
                {
                    //correct the step if the value is greater than the upper limit
                    if (x + h > xn) h = xn - x;

                    double sumbk = 0.0;
                    double sumbk2 = 0.0;
                    for (int i = 0; i < order; i++)
                    {
                        double sumk = 0.0;
                        for (int j = 0; j <= i - 1; j++)
                            sumk += a[i, j] * k[j];

                        k[i] = f(x + c[i] * h, y + h * sumk);
                        sumbk += b[i] * k[i];
                        sumbk2 += b2[i] * k[i];
                    }

                    double newx = x + h;
                    double newy = y + h * sumbk;
                    double newy2 = y + h * sumbk2;

                    double error = Math.Abs(newy - newy2); //O(h4) 
                                                           //as proved and explained in https://www.youtube.com/watch?v=Vs2usBZjUO8 (because it is of fourth order)
                                                           //and http://www.csun.edu/~lcaretto/me309/21-nsodeSystems.pdf

                    double scaleFactor = Math.Pow(h * tolerance / 2.0 / (xn - x0) / error, 1.0 / hOrder);
                    //change step for next iteration
                    if (scaleFactor >= 2)
                        h *= 2.0;
                    else if (scaleFactor < 1)
                        h /= 2.0;

                    if (scaleFactor >= 1)
                    //we update the values only if the scale factor is safe
                    //else we redo the calculation with smaller h
                    {
                        points++;
                        values.Add(new Tuple<double, double>(newx, newy));

                        x = newx;
                        y = newy;
                    }
                }

                //convert list to double array
                int size = points;
                ret = new double[2, points];
                for (int i = 0; i < points; i++)
                {
                    ret[0, i] = values[i].Item1;
                    ret[1, i] = values[i].Item2;
                }

            }


            return ret;
        } //ok


        public event EventHandler<SolverEventArgs> StepChanged;

        //tolerance is taken into account only in adaptive methods
        public double[,] Solve(
            Func<double, double[], double>[] f,
            double x0, double[] y0,
            double h, double xn, double tolerance = 1e-15)
        {
            {
                int ysCount = f.Length;
                double[,] ret = null;



                int order = c.Length; // = b.Length
                double[,] k = new double[order, f.Length];

                double x = x0;
                double[] y = y0;

                if (b2 == null) //not adaptive control
                {
                    int intervals = (int)Round((xn - x0) / h);
                    ret = new double[1 + ysCount, intervals + 1];

                    ret[0, 0] = x0;
                    for (int iFunc = 1; iFunc <= ysCount; iFunc++)
                        ret[iFunc, 0] = y0[iFunc - 1];

                    for (int iStep = 1; iStep <= intervals; iStep++)
                    {
                        double[] sumbk = new double[f.Length]; //one sum for each function
                        //calculate ks for all functions 
                        for (int i = 0; i < order; i++)
                        {
                            //first calculate the ys which are common for all k[j,:]
                            double[] yi = new double[ysCount];
                            for (int iFunc = 0; iFunc < ysCount; iFunc++)
                            {
                                //calculate k for each equation
                                double sumk = 0.0;
                                for (int j = 0; j <= i - 1; j++)
                                    sumk += a[i, j] * k[j, iFunc];

                                //add the necessary offset for all ys
                                yi[iFunc] = y[iFunc] + h * sumk;
                            }

                            //now calculate the k for each function
                            //k[i] for all functions
                            for (int iFunc = 0; iFunc < ysCount; iFunc++)
                            {
                                k[i, iFunc] = f[iFunc](x + c[i] * h, yi);
                                sumbk[iFunc] += b[i] * k[i, iFunc];
                            }
                        }

                        //copy to return array
                        ret[0, iStep] = x0 + iStep * h; //x
                        for (int iFunc = 1; iFunc <= ysCount; iFunc++)
                            ret[iFunc, iStep] = y[iFunc - 1] + h * sumbk[iFunc - 1]; //y

                        ////print to output
                        //for (int iFunc = 0; iFunc <= ysCount; iFunc++)
                        //    Debug.Write($"{ret[iFunc, iStep]}\t");
                        //Debug.Write("\r\n");

                        //Console.Write($"{ret[0, iStep]}\t");
                        //for (int iFunc = 1; iFunc <= ysCount; iFunc++)
                        //    Console.Write($"{ret[iFunc, iStep]:0.00E+00}\t");
                        //Console.Write("\r\n");

                        //Debug.WriteLine($"{ret[0, i]}\t{ret[1, i]}\t{ret[2, i]}\t{ret[3, i]}\t{values[4, i]}\t{values[5, i]}\t{values[5, i]}");


                        x = ret[0, iStep];
                        //copy back to y
                        for (int iFunc = 1; iFunc <= ysCount; iFunc++)
                            y[iFunc - 1] = ret[iFunc, iStep];

                        StepChanged?.Invoke(this, new SolverEventArgs(x, y, h));
                    }
                }
                else //with adaptive control (not yet)
                {
                    //check for a maximum value of steps?
                    int points = 1;
                    List<Tuple<double, double[]>> values = new List<Tuple<double, double[]>>();

                    //add the initial values to the output array
                    values.Add(new Tuple<double, double[]>(x0, y0));

                    while (x < xn)
                    {
                        //correct the step if the value is greater than the upper limit
                        if (x + h > xn) h = xn - x;

                        double[] sumbk = new double[f.Length]; //one sum for each function
                        //calculate ks for all functions 
                        for (int i = 0; i < order; i++)
                        {
                            //first calculate the ys which are common for all k[j,:]
                            double[] yi = new double[ysCount];
                            for (int iFunc = 0; iFunc < ysCount; iFunc++)
                            {
                                //calculate k for each equation
                                double sumk = 0.0;
                                for (int j = 0; j <= i - 1; j++)
                                    sumk += a[i, j] * k[j, iFunc];

                                //add the necessary offset for all ys
                                yi[iFunc] = y[iFunc] + h * sumk;
                            }

                            //now calculate the k for each function
                            //k[i] for all functions
                            for (int iFunc = 0; iFunc < ysCount; iFunc++)
                            {
                                k[i, iFunc] = f[iFunc](x + c[i] * h, yi);
                                sumbk[iFunc] += b[i] * k[i, iFunc];
                            }
                        }

                        //double newx = x + h;
                        //double newy = 

                        ////copy to return array
                        //ret[0, iStep] = x0 + iStep * h; //x
                        //for (int iFunc = 1; iFunc <= ysCount; iFunc++)
                        //    ret[iFunc, iStep] = y[iFunc - 1] + h * sumbk[iFunc - 1]; //y

                    }
                }

                return ret;
            }
        }

    }
}
