using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Numerical
{
    public static class Integration
    {

        /// <summary>
        /// Integrates based on Trapezoid rule (Chapra p. 613)
        /// </summary>
        /// <param name="f"></param>
        /// <param name="intervals">Equals to points + 1</param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double IntegrateTrapezoid(Func<double, double> f, int intervals, double a, double b)
        {
            double sum = f(a) + f(b);
            double h = (b - a) / intervals;
            for (int i = 1; i < intervals; i++)
                sum += 2 * f(a + i * h);
            return sum * h / 2.0;
        }

        public static Func<Func<double, double>, double, double, double>
            NewtonCotesDefault = NewtonCotesClosed7;

        /// <summary>
        /// Default integration function for functions F(t,x). t must be the last parameter
        /// </summary>
        public static Func<Func<double, double, double>, double, double, double, double>
            NewtonCotesDefaultT = NewtonCotesClosed7T;

        #region Newton-Cotes Closed

        /// <summary>
        /// 6 points (5 intervals) Newton-Cotes (h^7 truncation error).
        /// (http://mathworld.wolfram.com/Newton-CotesFormulas.html)
        /// </summary>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double NewtonCotesClosed6(Func<double, double> f, double a, double b)
        {
            double h = (b - a) / 5;
            return (b - a) * (19.0 * (f(a) + f(b)) + 75.0 * (f(a + h) + f(a + 4.0 * h)) +
                50.0 * (f(a + 2.0 * h) + f(a + 3.0 * h))) / 288.0;
        }

        /// <summary>
        /// 7 points (6 intervals) Newton-Cotes (h^9 truncation error).
        /// (http://mathworld.wolfram.com/Newton-CotesFormulas.html)
        /// </summary>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double NewtonCotesClosed7(Func<double, double> f, double a, double b)
        {
            double h = (b - a) / 6.0;
            double ret = h * (41.0 * (f(a) + f(b)) + 216.0 * (f(a + h) + f(a + 5.0 * h)) +
                27.0 * (f(a + 2.0 * h) + f(a + 4.0 * h)) + 272.0 * f(a + 3.0 * h)) / 140.0;

            //if(!double.IsNaN(ret))
          //  if (double.IsNaN(ret)) Debugger.Break();

            Debug.WriteLine($"a = {a}, b = {b}, ret = {ret}");


            return ret;
        }

        //integerate f(t,x)  - x in [a,b]

        /// <summary>
        /// Integrate f(t,x) over x in the range [a,b]. t must be given.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double NewtonCotesClosed7T(Func<double, double, double> f, double a, double b, double t) //f(t,x)
        {
            double h = (b - a) / 6.0;
            return h * (41.0 * (f(t, a) + f(t, b)) + 216.0 * (f(t, a + h) + f(t, a + 5.0 * h)) +
                27.0 * (f(t, a + 2.0 * h) + f(t, a + 4.0 * h)) + 272.0 * f(t, a + 3.0 * h)) / 140.0;
        }

        /// <summary>
        /// 9 points (8 intervals) Newton-Cotes (h^11 truncation error).
        /// </summary>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double NewtonCotesClosed9(Func<double, double> f, double a, double b)
        {
            const double c = 4.0 / 14175.0;
            double h = (b - a) / 8.0;
            return h * c * (989.0 * (f(a) + f(b)) + 5888.0 * (f(a + h) + f(a + 7.0 * h)) - 928.0 * (f(a + 2.0 * h) + f(a + 6.0 * h))
                + 10496.0 * (f(a + 3.0 * h) + f(a + 5.0 * h)) - 4540.0 * f(a + 4.0 * h));
        }

        /// <summary>
        /// 11 points (10 intervals) Newton-Cotes (h^13 truncation error).
        /// </summary>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double NewtonCotesClosed11(Func<double, double> f, double a, double b)
        {
            const double c = 5.0 / 299376.0;
            double h = (b - a) / 10.0;
            return h * c * (16067.0 * (f(a) + f(b)) + 106300.0 * (f(a + h) + f(a + 9.0 * h)) - 48525.0 * (f(a + 2.0 * h) + f(a + 8.0 * h)) +
                272400.0 * (f(a + 3.0 * h) + f(a + 7.0 * h)) - 260550.0 * (f(a + 4.0 * h) + f(a + 6.0 * h)) + 427368.0 * f(a + 5.0 * h));
        }
        #endregion

        #region Newton-Cotes Open
        /// <summary>
        /// 6 points (5 intervals+2) Newton-Cotes open (h^7 truncation error).
        /// </summary>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>  
        public static double NewtonCotesOpen6(Func<double, double> f, double a, double b)
        {
            const double c = 7.0 / 1440.0;
            double h = (b - a) / 7.0;
            return h * c * (611.0 * (f(a + h) + f(a + 6.0 * h)) - 453.0 * (f(a + 2.0 * h) + f(a + 5.0 * h)) +
                562.0 * (f(a + 3.0 * h) + f(a + 4.0 * h)));
        }

        /// <summary>
        /// 7 points (6 intervals+2) Newton-Cotes open (h^9 truncation error).
        /// </summary>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns> 
        public static double NewtonCotesOpen7(Func<double, double> f, double a, double b)
        {
            const double c = 8.0 / 945.0;
            double h = (b - a) / 8.0;
            return h * c * (460.0 * (f(a + h) + f(a + 7.0 * h)) - 954.0 * (f(a + 2.0 * h) + f(a + 6.0 * h)) +
                2196.0 * (f(a + 3.0 * h) + f(a + 5.0 * h)) - 2459.0 * f(a + 4.0 * h));
        }




        #endregion

        #region Gauss Hermite

        public static Func<Func<double, double>, double>
            //     GaussHermiteDefault = GaussHermite20;
            GaussHermiteDefault = GaussHermite5;

        public static Func<Func<double, double, double>, double, double>
            GaussHermiteDefaultT = GaussHermite5T;

        //http://dlmf.nist.gov/3.5#viii
        //http://keisan.casio.com/exec/system/1281195844

        private static double[,] gaussHermite5 = new double[,] {
            { 0.0,0.945308720482942 }, //the first column is the xk and the second is the wk
            { 0.958572464613819, 0.393619323152241},
            { 0.202018287045609e1,0.199532420590459e-1 }
        };

        private static double[,] gaussHermite10 = new double[,] {
            { 0.342901327223704609,0.610862633735325799}, //the first column is the xk and the second is the wk
            { 0.103661082978951365e1, 0.240138611082314686},
            { 0.175668364929988177e1, 0.338743944554810631e-1 },
            { 0.253273167423278980e1,0.134364574678123269e-2},
            { 0.343615911883773760e1,0.764043285523262063e-5}
        };

        private static double[,] gaussHermite15 = new double[,] {
            {0.0 ,0.564100308726417532853 },
            {0.565069583255575748526,0.412028687498898627026},
            {0.113611558521092066632e1 ,0.158488915795935746884},
            {0.171999257518648893242e1 ,0.307800338725460822287e-1},

            { 0.232573248617385774545e1,0.277806884291277589608e-2},
            { 0.296716692790560324849e1,0.100004441232499868127e-3},
            { 0.366995037340445253473e1,0.105911554771106663578e-5},
            { 0.449999070730939155366e1,0.152247580425351702016e-8}
        };

        private static double[,] gaussHermite20 = new double[,]
        {
            {0.245340708300901249904 ,0.46224366960061008965},
            {0.737473728545394358706 ,0.28667550536283412972},
            {0.123407621539532300789e1,0.109017206020023320014},
            {0.173853771211658620678e1 ,0.248105208874636108822e-1},
            {0.225497400208927552308e1 ,0.324377334223786183218e-2},

            {0.278880605842813048053e1 ,0.228338636016353967257e-3},
            {0.334785456738321632691e1 ,0.780255647853206369415e-5},
            {0.394476404011562521038e1 , 0.108606937076928169400e-6},
            {0.460368244955074427308e1 ,0.439934099227318055363e-9},
            {0.538748089001123286202e1 ,0.222939364553415129252e-12}
        };


        /// <summary>
        /// Integrate f(t,x) over x (t is a constant value).
        /// </summary>
        /// <param name="f"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double GaussHermite5T(Func<double, double, double> f, double t)
        {
            double s = gaussHermite5[0, 1] /*w*/ * f(t, 0.0);
            for (int i = 1; i <= 2; i++)
            {
                double xi = gaussHermite5[i, 0];
                s += gaussHermite5[i, 1] /*w*/ * (f(t, xi) + f(t, -xi));
            }
            return s;

        }

        public static double GaussHermite5(Func<double, double> f)
        {
            double s = gaussHermite5[0, 1] /*w*/ * f(0.0);
            for (int i = 1; i <= 2; i++)
            {
                double xi = gaussHermite5[i, 0];
                s += gaussHermite5[i, 1] /*w*/ * (f(xi) + f(-xi));
            }
            return s;
        }

        public static double GaussHermite10(Func<double, double> f)
        {
            double s = 0.0;
            for (int i = 0; i <= 4; i++)
            {
                double xi = gaussHermite10[i, 0];
                s += gaussHermite10[i, 1] /*w*/ * (f(xi) + f(-xi));
            }
            return s;
        }

        public static double GaussHermite15(Func<double, double> f)
        {
            double s = gaussHermite15[0, 1] /*w*/ * f(0.0);
            for (int i = 1; i <= 7; i++)
            {
                double xi = gaussHermite15[i, 0];
                s += gaussHermite15[i, 1] /*w*/ * (f(xi) + f(-xi));
            }
            return s;

            // return GaussHermiteGeneric(f, gaussHermite15);
        }

        public static double GaussHermite20(Func<double, double> f)
        {
            double s = 0.0;
            for (int i = 0; i <= 9; i++)
            {
                double xi = gaussHermite20[i, 0];
                s += gaussHermite20[i, 1] /*w*/ * (f(xi) + f(-xi));
            }
            return s;

            // return GaussHermiteGeneric(f, gaussHermite20);
        }

        public static double GaussHermiteGeneric(
            Func<double, double> f, double[,] abscissasWeights)
        {
            double s = 0.0;
            int startIndex = 0;
            int pointsCount = abscissasWeights.GetLength(0);

            if (abscissasWeights[0, 0] == 0.0) //(xi==0 when odd number of points)
            {
                s = abscissasWeights[0, 1] /*w*/ * f(0.0);
                startIndex = 1;
            }

            for (int i = startIndex; i < pointsCount; i++)
            {
                double xi = abscissasWeights[i, 0];
                s += abscissasWeights[i, 1] /*w*/ * (f(xi) + f(-xi));
            }
            return s;
        }


        #endregion

    }
}
