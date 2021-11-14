using Numerical.Polynomials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Console;

namespace Numerical
{
    public enum SolverStatus
    {
        None,
        InvalidInput,
        Converged,
        Diverged,
        ReachedMaximumIterations
    }

    public enum SolverToleranceType
    {
        RelativeError,
        DistanceFromZero
    }

    /// <summary>
    /// Single value result.
    /// </summary>
    public struct Result
    {
        public XY xy; //x is the input and y = f(x)
        public int Iterations;
        public SolverStatus SolverStatus;

        public Result(XY xy, int iterations, SolverStatus solverStatus)
        {
            this.xy = xy; this.Iterations = iterations; this.SolverStatus = solverStatus;
        }
    }


    public static class NonLinearSolver
    {
        //TODO: https://github.com/yoki/Optimization/blob/master/hybrid.f90 DOGLEG!

        #region Global solver variables

        public static double Tolerance { get; set; } = 1e-15;

        public static SolverToleranceType ToleranceType { get; set; } =
            SolverToleranceType.RelativeError;

        public static bool ShowIntermediateSteps { get; set; } = true;

        public static int MaxIterations { get; set; } = 1000;

        #endregion

        #region Solver template
        delegate XY internalSolver(XY xy);
        delegate bool areInputsValid(XY xy);

        static areInputsValid defaultValidCheck = xy => !double.IsNaN(xy.x) && !double.IsNaN(xy.y);


        /// <summary>
        /// This is a solve template which is common for all methods.
        /// </summary>
        /// <param name="solverFunction"></param>
        /// <param name="f"></param>
        /// <param name="x0"></param>
        /// <param name="debug"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxIterations"></param>
        /// <returns></returns>
        private static Result solve(
            areInputsValid validInputsFunction,
            internalSolver solverFunction,
            Func<double, double> f, double x0)
        {
            XY xfx = new XY(x0, f(x0));
            int iStep = 0;

            if (!validInputsFunction(xfx))
                return new Result(xfx, 0, SolverStatus.InvalidInput);

            try
            {
                for (iStep = 1; iStep <= MaxIterations; iStep++)
                {
                    XY xfxOld = xfx;
                    xfx = solverFunction(xfxOld);

                    double error = Math.Abs((xfx.x - xfxOld.x) / xfx.x);
                    if (ShowIntermediateSteps)
                        WriteLine($"Step #{iStep}, x:{xfx.x}, f(x):{xfx.y}, e:{error:0.000E+00}");

                    if (double.IsNaN(xfx.x)) //return the last valid value 
                        return new Result(xfxOld, iStep - 1, SolverStatus.Diverged);

                    if (ToleranceType == SolverToleranceType.RelativeError && error <= Tolerance ||
                        ToleranceType == SolverToleranceType.DistanceFromZero && Math.Abs(xfx.y) <= Tolerance)//||
                        return new Result(xfx, iStep, SolverStatus.Converged);
                }
                return new Result(xfx, MaxIterations, SolverStatus.ReachedMaximumIterations);
            }
            catch
            {
                return new Result(xfx, iStep, SolverStatus.Diverged);
            }
        }
        #endregion

        #region Open methods

        public static Result NewtonRaphson(
           Func<double, double> f, Func<double, double> df, double x0)
        {
            return solve(
                defaultValidCheck,
                (XY xfx) =>
                {
                    double dfx = df(xfx.x);
                    double x = xfx.x - xfx.y / dfx;
                    return new XY(x, f(x));
                }, f, x0);
        }

        public static Result NewtonRaphson(Polynomial p, double x0, int multiplicity = 1)
        {
            Polynomial dp = p.Derivative();
            //return NewtonRaphson(x=>p[x],x=>dp[x],x0);

            //this variant adds the multiplicity capability 
            return solve(
                defaultValidCheck,
                (XY xfx) =>
                {
                    //https://en.wikipedia.org/wiki/Newton%27s_method
                    //tested in x^10 -1 case and convergence is much faster when assumedMultiplicity is 1!
                    double dfx = dp[xfx.x];
                    double x = xfx.x - multiplicity * xfx.y / dfx;
                    return new XY(x, p[x]);
                }, x => p[x], x0);
        }

        /// <summary>
        /// Solves the equation x=g(x) (x=f(x)+x=g(x)) (Chapra p. 146).
        /// In order to converge |g'(x)|&lt;1 must occur.
        /// </summary>
        /// <returns></returns>
        public static Result FixedPointIteration(Func<double, double> f, double x0)
        {
            return solve(
                defaultValidCheck,
                (XY xfx) =>
                    {
                        double x = xfx.y + xfx.x;
                        return new XY(x, f(x));
                    },
                f, x0);
        }

        public static Result FixedPointIteration(Polynomial p, double x0) =>
            FixedPointIteration(x => p[x], x0);

        public static Result ModifiedSecant(
            Func<double, double> f, double x0, double delta = 0.01)
        {
            return solve(
                defaultValidCheck,
                (XY xfx) =>
                {
                    //double fx = f(x);
                    double x = xfx.x - delta * xfx.x * xfx.y / (f(xfx.x + delta * xfx.x) - xfx.y);
                    return new XY(x, f(x));
                },
                f, x0);
        }

        public static Result ModifiedSecant(Polynomial p, double x0, double delta = 0.01) =>
            ModifiedSecant(x => p[x], x0, delta);

        /// <summary>
        /// Secant method requires to initial values x-1 and x0. Does not need the derivative (Chapra p.158).
        /// </summary>
        /// <param name="f"></param>
        /// <param name="x0"></param>
        /// <param name="xm1"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxIterations"></param>
        /// <returns></returns>
        public static Result Secant(
            Func<double, double> f, double x0, double xm1)
        {
            double oldx = xm1, fx1 = f(xm1);
            return solve(
                xy => defaultValidCheck(xy) && defaultValidCheck(new XY(xm1, fx1)),
                (XY xfx) =>
                {
                    oldx = xfx.x;
                    double x = xfx.x - xfx.y * (xm1 - xfx.x) / (fx1 - xfx.y);
                    xm1 = oldx; //set next f(x-1)
                    fx1 = xfx.y; //set next x-1
                    return new XY(x, f(x));
                }, f, x0);
        }

        public static Result Secant(Polynomial p, double x0, double xm1) =>
            Secant(x => p[x], x0, xm1);

        #endregion

        #region Closed bracket methods
        public static Result Bisection(Func<double, double> f,
          double xl, double xu)
        {
            double fxl = f(xl);
            double fxu = f(xu);

            return solve(
                xy => defaultValidCheck(xy) //xl,fxl
                    && defaultValidCheck(new XY(xu, fxu)) && fxu * fxl <= 0,
                (XY xfx) =>
                {
                    double x = (xl + xu) / 2.0;
                    double fx = f(x);
                    if (fx * fxl > 0) //replace xl with x
                    {
                        xl = x;
                        fxl = fx;
                    }
                    else //replace xu with x
                    {
                        xu = x;
                        fxu = fx;
                    }
                    return new XY(x, fx);
                }, f, xl);
        }

        public static Result Bisection(Polynomial p, double xl, double xu) =>
            Bisection(x => p[x], xl, xu);

        public static Result FalsePosition(Func<double, double> f,
                double xl, double xu)
        {
            double fxl = f(xl);
            double fxu = f(xu);
            return solve(
                xy => defaultValidCheck(xy) //xl,fxl
                    && defaultValidCheck(new XY(xu, fxu)) && fxu * fxl <= 0,
                (XY xfx) =>
                {
                    double x = xu - fxu * (xl - xu) / (fxl - fxu); //this is the only line that differs from Bisection method
                    double fx = f(x);
                    if (fx * fxl > 0) //replace xl with x
                    {
                        xl = x;
                        fxl = fx;
                    }
                    else //replace xu with x
                    {
                        xu = x;
                        fxu = fx;
                    }
                    return new XY(x, fx);
                }, f, xl);
        }


        public static Result FalsePosition(Polynomial p, double xl, double xu) =>
            FalsePosition(x => p[x], xl, xu);


        public static Result ModifiedFalsePosition(Func<double, double> f,
                double xl, double xu)
        {
            int il = 0, iu = 0; //counters to track stagnant bounds
            double fxl = f(xl);
            double fxu = f(xu);
            return solve(
                xy => defaultValidCheck(xy) //xl,fxl
                    && defaultValidCheck(new XY(xu, fxu)) && fxu * fxl <= 0,
                (XY xfx) =>
                {
                    double x = xu - fxu * (xl - xu) / (fxl - fxu);
                    double fx = f(x);
                    if (fx * fxl > 0)
                    {
                        xl = x;
                        fxl = fx;
                        //the following two lines differ from the False Position method
                        il = 0;
                        if (++iu >= 2) fxu /= 2.0;
                    }
                    else
                    {
                        xu = x;
                        fxu = fx;
                        //the following two lines differ from the False Position method
                        iu = 0;
                        if (++il >= 2) fxl /= 2.0;
                    }
                    return new XY(x, fx);
                }, f, xl);
        }

        public static Result ModifiedFalsePosition(Polynomial p, double xl, double xu) =>
            ModifiedFalsePosition(x => p[x], xl, xu);

        /// <summary>
        /// Brent's algorithm is based on the algorithm described in https://en.wikipedia.org/wiki/Brent%27s_method. 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="xl"></param>
        /// <param name="xu"></param>
        /// <returns></returns>
        public static Result Brent(Func<double, double> f,
          double xl, double xu)
        {
            double fxl = f(xl);
            double fxu = f(xu);

            //https://en.wikipedia.org/wiki/Brent%27s_method
            if (Math.Abs(fxl) < Math.Abs(fxu)) //swap a and b
            {
                double tmp = xl; xl = xu; xu = tmp;
                tmp = fxl; fxl = fxu; fxu = tmp;
            }
            double c = xl,
                fc = fxl;

            double d = 0;
            bool useBisection = true;

            return solve(
                xy => defaultValidCheck(xy) //xl,fxl
                    && defaultValidCheck(new XY(xu, fxu)) && fxu * fxl <= 0,
                (XY xfx) =>
                {
                    double x;
                    if (fxl != fc && fxu != fc)
                    {
                        //inverse quadratic interpolation
                        x = xl * fxu * fc / (fxl - fxu) / (fxl - fc) +
                            xu * fxl * fc / (fxu - fxl) / (fxu - fc) +
                            c * fxl * fxu / (fc - fxl) / (fc - fxu);
                    }
                    else //secant method
                        x = xu - fxu * (xu - xl) / (fxu - fxl);

                    useBisection = !(x >= (3 * xl + xu) / 4.0 && x <= xu) ||  //condition 1
                        useBisection && Math.Abs(x - xu) >= Math.Abs(xu - c) / 2.0 ||//condition 2
                        !useBisection && Math.Abs(x - xu) >= Math.Abs(c - d) / 2.0 || //condition 3
                        useBisection && Math.Abs(xu - c) < double.Epsilon ||
                        !useBisection && Math.Abs(c - d) < double.Epsilon;

                    if (useBisection) //bisection method
                        x = (xl + xu) / 2.0;

                    double fx = f(x);
                    //d is assigned for the first time here; it won't be used above on the first iteration because mflag is set
                    d = c;
                    c = xu; fc = fxu;
                    if (fxl * fx < 0.0)
                    {
                        xu = x;
                        fxu = fx;
                    }
                    else
                    {
                        xl = x;
                        fxl = fx;
                    }

                    if (Math.Abs(fxl) < Math.Abs(fxu)) //swap a and b
                    {
                        double tmp = xl; xl = xu; xu = tmp;
                        tmp = fxl; fxl = fxu; fxu = tmp;
                    }

                    return new XY(x, fx);
                }, f, xl);
        }

        public static Result Brent(Polynomial p, double xl, double xu) =>
            Brent(x => p[x], xl, xu);


        #endregion

    }
}
