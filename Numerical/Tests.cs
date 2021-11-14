using Numerical.Polynomials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using static System.Math;
using static System.Console;
using System.Diagnostics;

namespace Numerical
{
    public class Tests
    {
        //shortcut to write to the console
        static Action<string> w = WriteLine;
        static void writeAsTitle(string title)
        {
            w(title);
            w(new string('-', title.Trim().Length));
        }

        #region Powell test

        static double[] fAerosol(double[] x)
        {
            double Mpl = 8.8002079167324871e-11;
            double Npl = 10000000000000.0;
            double Spl = 0.00015893908825321637;
            double s1 = 7.0685834705770342e-18;
            double m1 = 2.4740042147019616e-24;

            double a = x[0], d = x[1]; //a = 0.5, d = 2

            return new double[]
            {
                a / (a + 3) * (Math.Pow(d, a + 3) - 1.0) / (Math.Pow(d, a) - 1.0) - Mpl / Npl / m1,
                (a + 2) / (a + 3) * (Math.Pow(d, a + 3) - 1.0) / (Math.Pow(d, a + 2) - 1.0) - Mpl / Spl * s1 / m1
            };
        }

        static double[] fpowell(double[] x)
        {  //x1 = x2 = 1
            return new double[]
                {
                    10.0*(x[1]-x[0]*x[0]),
                    1.0-x[0]
                };
        }
        public static void TestPowell()
        {
            int info;
            double[,] j;
            double[] fvalout, xout =
            PowellHybrid.Solve(
                fun: fpowell, // Function to be solved
                x0: new double[] { 0.0,0.0 }, // Initial value
                xtol: 1e-6, // error torelance
                info: out info, //info for the solution
                fvalout: out fvalout, // f(xout)
                factor: 1.0, //initial value of delta
                jacobianStep: 1e-6, //Stepsize for the Jacobian
                display: true,
                maxFunCall: 2000, //max number of function calls
                noUpdate: 0,
                jacobianOut: out j);

            Console.WriteLine($"\r\nx1:{xout[0]} x2: {xout[1]}");
        }


        public static void TestPowell2()
        {
            int info;
            double[,] j;
            double[] fvalout, xout =
            PowellHybrid.Solve(
                fun: fAerosol, // Function to be solved
                x0: new double[] { 8.0, 3.0 }, // Initial value
                xtol: 1e-6, // error torelance
                info: out info, //info for the solution
                fvalout: out fvalout, // f(xout)
                factor: 1.0, //initial value of delta
                jacobianStep: 1e-6, //Stepsize for the Jacobian
                display: true,
                maxFunCall: 2000, //max number of function calls
                noUpdate: 0,
                jacobianOut: out j);

            Console.WriteLine($"\r\na:{xout[0]} d: {xout[1]}");
        }

        #endregion

        #region NonLinearSolver

        public static void TestPolynomialNewtonRaphson()
        {
            Func<double, double> f = t => t * t - 2.0;
            Func<double, double> df = t => 2 * t;

            writeAsTitle($"Newton-Raphson test");
            var solution = NonLinearSolver.NewtonRaphson(f, df, 1.6);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");


            writeAsTitle($"Newton-Raphson polynomial test");
            var x = Polynomial.x;
            Polynomial p = (x ^ 2) - 2.0;
            solution = p.NewtonRaphson(2.0);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");

            writeAsTitle($"Newton-Raphson polynomial multi-root test");
            p = (x ^ 10) - 1.0;
            solution = p.NewtonRaphson(0.5, 1);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");
        }

        public static void TestSolverMethods()
        {
            NonLinearSolver.ShowIntermediateSteps = true;
            NonLinearSolver.Tolerance = 1e-15;
            NonLinearSolver.ToleranceType = SolverToleranceType.DistanceFromZero;
            NonLinearSolver.MaxIterations = 100;

            Func<double, double> f = t => Exp(-t) - t;
            Func<double, double> df = t => -Exp(-t) - 1.0;
            //Func<double, double> g = x => Exp(-x);
            //solve x=g(x) see page 146, 147 Chapra
            writeAsTitle("OPEN METHODS");
            writeAsTitle($"Fixed-point iteration");
            var solution = NonLinearSolver.FixedPointIteration(f, 0.0);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");

            writeAsTitle($"Newton-Raphson");
            solution = NonLinearSolver.NewtonRaphson(f, df, 0.0);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");

            writeAsTitle($"Secant");
            solution = NonLinearSolver.Secant(f, 1.0, 0.0);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");

            writeAsTitle($"Modified Secant");
            solution = NonLinearSolver.ModifiedSecant(f, 1.0);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");

            //closed methods
            writeAsTitle("\r\nCLOSED METHODS");

            NonLinearSolver.Tolerance = 1e-4;
            writeAsTitle($"Bisection");
            solution = NonLinearSolver.Bisection(f, 0.0, 1.3);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");

            writeAsTitle($"False-Position");
            solution = NonLinearSolver.FalsePosition(f, 0.0, 1.3);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");


            writeAsTitle($"Modified False-Position");
            solution = NonLinearSolver.ModifiedFalsePosition(f, 0.0, 1.3);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");

            writeAsTitle($"Brent's");
            solution = NonLinearSolver.Brent(f, 0.0, 1.3);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");


            var x = Polynomial.x;
            f = t => Pow(t, 10.0) - 1.0;
            Polynomial p = (x ^ 10) - 1.0;
            writeAsTitle("\r\nPOLYNOMIAL TESTS");

            writeAsTitle($"Bisection (polynomial)");
            solution = NonLinearSolver.Bisection(p, 0.0, 1.3);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");

            writeAsTitle($"False-Position (polynomial)");
            solution = NonLinearSolver.FalsePosition(p, 0.0, 1.3);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");

            writeAsTitle($"Modified False-Position (polynomial)");
            solution = NonLinearSolver.ModifiedFalsePosition(p, 0.0, 1.3);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");

            writeAsTitle($"Brent's (polynomial)");
            solution = NonLinearSolver.Brent(p, 0.0, 1.3);
            w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");

            //writeAsTitle($"Brent's test #2"); //https://en.wikipedia.org/wiki/Brent%27s_method
            //solution = NonLinearSolver.Brent((x+3)*((x-1.0)^2), -4, 4.0/3.0);
            //w($"x: {solution.xy.x}, Iterations: {solution.Iterations}, Status: {solution.SolverStatus}\r\n");

        }

        public static void TestLagrangePolynomials()
        {
            //exp(x)
            LagrangeInterpolationPolynomial p = new LagrangeInterpolationPolynomial(new XY[] {
                new XY(0.0,Exp(0.0)),
                new XY(0.5,Exp(0.5)),
                new XY(1.0,Exp(1.0))});
            w($"Exp(0.25) = {Exp(0.25)}");
            w($"L(0.25) = {p[0.25]}");
            //alternative constructor using 2d array
            LagrangeInterpolationPolynomial p2 = new LagrangeInterpolationPolynomial(new double[,] {
                { 0.0,Exp(0.0) },
                { 0.5,Exp(0.5) },
                { 1.0,Exp(1.0) }
            });
            w($"L2(0.25) = {p2[0.25]}");

            //exp(x)  (exp(x) = d/dx(exp(x))
            HermiteInterpolationPolynomial h = new HermiteInterpolationPolynomial(
                new double[] { 0.0, 0.5, 1.0 }, //x
                new double[] { Exp(0.0), Exp(0.5), Exp(1.0) }, //y
                new double[] { Exp(0.0), Exp(0.5), Exp(1.0) }); //dy/dx
            //w($"Exp(0.25) = {Exp(0.25)}");
            w($"H(0.25) = {h[0.25]}");
            HermiteInterpolationPolynomial h2 = new HermiteInterpolationPolynomial(new double[,] {
                { 0.0, Exp(0.0), Exp(0.0)}, //x, y, dydx
                { 0.5, Exp(0.5), Exp(0.5)},
                { 1.0, Exp(1.0), Exp(1.0)}
            });
            w($"H2(0.25) = {h2[0.25]}");

        }

        #endregion

        #region Runge-Kutta


        #region System1
        //System1: z=y', y=cos(x²), x0=0, y0={1,0};
        static double dydx(double x, double[] y) => y[1]; //y contains y,z
        static double dzdx(double x, double[] y)
        {
            double x2 = x * x;
            return -4.0 * x2 * Cos(x2) - 2.0 * Sin(x2);
        }
        public static void SolveRKSystem1()
        {
            var values = RungeKuttaSolver.Butcher6.Solve(
                new Func<double, double[], double>[]
                {dydx,dzdx}, 0, new double[] { 1.0, 0.0 }, 0.01, 20.0, 1e-5);

            using (StreamWriter writer = new StreamWriter("d:\\t.txt"))
            {
                for (int i = 0; i < values.GetLength(1); i++)
                    writer.WriteLine($"{values[0, i]}\t{values[1, i]}\t{values[2, i]}");
            }
        }
        #endregion

        #region System2

        //System2: x1(t)=9e^t-8e^-t, x2(t)=9e^t-4e^-t, t=0, x10,x20 =[1,5]
        static double dxdt2(double t, double[] x) //x contains x1, x2
        {
            return -3 * x[0] + 4.0 * x[1]; //p.42 nikolidakis
        }
        static double dydt2(double t, double[] x)
        {
            return -2.0 * x[0] + 3.0 * x[1];
        }

        public static void SolveRKSystem2()
        {
            var values = RungeKuttaSolver.Butcher6.Solve(
                new Func<double, double[], double>[]
                {dxdt2,dydt2}, 0, new double[] { 1.0, 5.0 }, 0.01, 20.0, 1e-5);
            using (StreamWriter writer = new StreamWriter("d:\\t.txt"))
            {
                for (int i = 0; i < values.GetLength(1); i++)
                    writer.WriteLine($"{values[0, i]}\t{values[1, i]}\t{values[2, i]}");
            }
        }
        #endregion


        #endregion


        #region Matrix tests
        public static void MatrixTests()
        {
            double[,] a = new double[,]
                 {
                  { 2,3},
                  { 1,2 },
                  { 1,8 }//a
                 }, b = new double[,]
                 {
                  { 1,2},
                  { 3,4}
                 }; //b
            double[,] r = Matrix.matmul(a, b);
            WriteLine(Matrix.ToString(r));

            Matrix ma = new double[,] //implicit operator
                 {
                  { 2,3},
                  { 1,2 },
                  { 1,8 }//a
                 };
            Matrix mb = new double[,] //implicit operator
                 {
                  { 1,2},
                  { 3,4}
                 };
            Matrix mr = ma * mb;
            Console.WriteLine(mr);

            ma = new double[,] { { 2, 3 }, { 1, 4 } };
            WriteLine(ma ^ 5); //power
            Matrix A = (Matrix)ma.Clone(); //clone matrix
            WriteLine((A ^ 2) - 9 * A + 8);
            var I = Matrix.Eye(3);
            WriteLine((2000 * I).ToString("0.0", ", "));
            WriteLine((2000 * I).ToString());
        }

        #endregion
    }
}
