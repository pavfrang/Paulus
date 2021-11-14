using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Numerical.Matrix;
using static Numerical.Math2;

namespace Numerical
{
    public static class PowellHybrid
    {
        //returns xout
        public static double[] Solve(
            Func<double[], double[]> fun, double[] x0,
            out int info, //information on output
            out double[] fvalout,
            out double[,] jacobianOut,
            double xtol = 0.0001, //relative tolerance of x
            double jacobianStep = -1, //relative step for derivative
            bool display = false, //controls display on iteration
            int maxFunCall = -1, //max number of function calls
            double factor = 100.0, //initial value of delta
            int noUpdate = 0, //jacobian recalculation info
            double deltaSpeed = 0.25, //how fast delta should decrease,
            double fxtolerance = 1e-11
            )
        {
            //initialize values
            //counters
            int iterationsCount = 0;
            int functionCount = 0;
            int goodJacobian = 0;
            int badJacobian = 0;
            int slowJacobian = 0;
            int slowFunction = 0;
            info = 0;
            int n = x0.Length;

            //set default values for optional inputs
            if (maxFunCall == -1) maxFunCall = n * 100;
            if (jacobianStep == -1) jacobianStep = xtol * 0.1;

            //Jacobian and function values
            double[] xbest = x0.ToArray();
            double[] fvalbest = fun(xbest);
            functionCount++;
            double[,] J = GetJacobian(fun, xbest, jacobianStep, fvalbest);
            functionCount++;
            double[,] Q, R;
            QrFactorization(J, out Q, out R);
            double[] Qtfval = matmul(transpose(Q), fvalbest);

            //calculate normalization matrix Psi
            //Inverse of normalization matrix  
            double[,] psiInv = new double[n, n];
            double[] psiDiag = new double[n];
            for (int i = 0; i < n; i++)
            {
                //normalization factor is R[i,i] unless R[i,i]=0
                double temp = R[i, i] != 0.0 ? Math.Abs(R[i, i]) : 1.0;
                psiInv[i, i] = 1.0 / temp;
                psiDiag[i] = temp;
            }
            //calculate initial value of delta
            double delta = factor * norm(mult(psiDiag, xbest));
            if (delta == 0.0) delta = 1.0;

            //check initial guess is good or not
            if (norm(fvalbest) == 0.0) info = 1;

            //display first line (not implemented)
            if (display)
            {
                Console.WriteLine("FsolveHybrid:");
                Console.WriteLine("       Norm      Actual  Trust-Region     Step  Jacobian   Direction");
                Console.WriteLine(" iter  f(x)    Reduction    Size          Size  Recalculate Type");
                Console.WriteLine($"   0 {norm(fvalbest):F5})");
            }

            //main loop
            while (true)
            {
                iterationsCount++;

                //old values are values at the start of the iteration
                double[] fvalold = (double[])fvalbest.Clone();
                double[] xold = (double[])xbest.Clone();

                //calculate the best direction
                double[] p;
                int directionFlag;
                Dogleg(out p, Q, matmul(R, psiInv), delta, Qtfval, out directionFlag);
                p = matmul(psiInv, p);

                //update the trust region
                double[] fvalnew = fun(add(xold, p));
                functionCount++;
                double[] fvalpredicted = add(fvalbest, matmul(Q, matmul(R, p)));
                double deltaOld = delta;
                double actualReduction, reductionRatio;
                UpdateDelta(ref delta, ref goodJacobian, ref badJacobian, out actualReduction, out reductionRatio,
                    fvalold, fvalnew, fvalpredicted, mult(psiDiag, p), deltaSpeed);

                //get the best value so far
                if (norm(fvalnew) < norm(fvalold) && reductionRatio > 0.0001)
                {
                    xbest = add(xold, p);
                    fvalbest = (double[])fvalnew.Clone();
                }

                //*** Check convergence ***
                //Successful Convergence
                //if (delta < xtol * norm(mult(psiDiag, xbest)) || norm(fvalbest) ==0.0) info = 1;
                if (delta < xtol * norm(mult(psiDiag, xbest)) || norm(fvalbest)<fxtolerance) info = 1;

                //too many function calls
                if (functionCount > maxFunCall)
                    info = 2;

                //tolerance is too small
                if (delta < 100.0 * epsilon * norm(mult(psiDiag, xbest))) info = 3;

                //not successful based on jacobian
                if (actualReduction > 0.1) slowJacobian = 0;
                if (slowJacobian == 5) info = 4;
                //if jacobian is recalculated every time, we do not perform this test
                if (noUpdate == 1) slowJacobian = 0;

                ////not successful based on function value
                //slowFunction++;
                //if (actualReduction > 0.01) slowFunction = 0;
                //if (slowFunction == 10) info = 5;

                //update jacobian
                double pnorm = norm(p);
#pragma warning disable CS0219 // Variable is assigned but its value is never used
                int updateJacobian = 0;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
                if (badJacobian == 2 || pnorm == 0.0 || noUpdate == 1)
                {
                    //calculate jacobian using finite differences
                    J = GetJacobian(fun, xbest, jacobianStep, fvalbest);
                    functionCount += n;
                    QrFactorization(J, out Q, out R);
                    Qtfval = matmul(transpose(Q), fvalbest);

                    //recalculate normalization matrix Psi
                    for (int i = 0; i < n; i++)
                    {
                        //normalization factor is R[i,i] unless R[i,i]=0
                        double temp = R[i, i] != 0.0 ? Math.Abs(R[i, i]) : 1.0;
                        psiInv[i, i] = Math.Min(psiInv[i, i], 1.0 / temp);
                        psiDiag[i] = 1 / psiInv[i, i];

                    }

                    //take care of counters
                    badJacobian = 0;
                    slowJacobian++;
                    updateJacobian = 1;
                }
                else if (reductionRatio > 0.0001)
                {
                    //broyden's rank 1 update
                    QrUpdate(ref Q, ref R, subtract(fvalnew, fvalpredicted), mult(p, 1.0 / (pnorm * pnorm)));
                    Qtfval = matmul(transpose(Q), fvalbest);
                }

                //display iteration 
                if (display)
                {
                    string sJacobian = updateJacobian == 1 ? " Yes  " : "      ";

                    string sDirection;
                    switch (directionFlag)
                    {
                        case 1:
                            sDirection = "Newton"; break;
                        case 2:
                            sDirection = "Cauchy"; break;
                        case 3:
                            sDirection = "Combination"; break;
                        default:
                            sDirection = "Unknown"; break;

                    }
                    Console.WriteLine($"{iterationsCount:###0} {norm(fvalbest):0.0####E+00} {100.0 * actualReduction:0.0####E+00} {deltaOld:0.0####E+00} {norm(mult(psiDiag, p))::0.0####E+00} {sJacobian} {sDirection}");
                }

                //exit check
                if (info != 0) break;
            }

            //prepare outputs
            double[] xout = (double[])xbest.Clone();
            fvalout = (double[])fvalbest.Clone();
            jacobianOut = (double[,])J;
            return xout;
        }

        static void UpdateDelta(ref double delta, ref int goodJacobian, ref int badJacobian,
            out double actualReduction, out double reductionRatio, double[] oldfval, double[] newfval,
            double[] predictedfval, double[] p, double deltaSpeed)
        {
            //update trust region delta

            //scaled reduction for predicted fval
            double predictedReduction = 1.0 - pow(norm(predictedfval) / norm(oldfval), 2);
            actualReduction = 1.0 - pow(norm(newfval) / norm(oldfval), 2);

            double pnorm = norm(p);
            //calculate the ratio of actual to predicted reduction
            reductionRatio = 0.0;
            //special case if PredictedReduction = 0
            if (predictedReduction != 0.0)
                reductionRatio = actualReduction / predictedReduction;

            if (reductionRatio < 0.1)
            {
                //prediction was not good, shrink trust region
                delta *= deltaSpeed;
                badJacobian++;
                goodJacobian = 0;
            }
            else
            {
                badJacobian = 0;
                goodJacobian++;
                if (goodJacobian > 1 || reductionRatio > 0.5)
                    //prediction was fair. expand trust region
                    delta = Math.Max(delta, 2.0 * pnorm);
                else if (Math.Abs(1.0 - reductionRatio) < 0.1)
                    //prediction was very good (the ratio is close to one)
                    //expand trust region
                    delta = 2.0 * pnorm;
            }
        }

        const double epsilon = 2.220446049250313e-016; //the same epsilon value with fortran (for double precision)
        static void Dogleg(out double[] p, double[,] Q, double[,] R, double delta, double[] Qtf, out int flag)
        {
            //find linear combination of newton direction and steepest descent direction

            //flag : it indicates the type of the p (optional)
            //flag = 1  : newton direction
            //flag = 2  : Steepest descent direction
            //flag = 3  : Linear combination of both

            //number of variables
            int n = R.GetLength(0);//p.Length;
            //calculate newton direction

            //prepare a small value in case diagonal element of R is zero
            double temp = epsilon * abs(diag(R)).Max();
            double[] nu = new double[n];

            if (R[n - 1, n - 1] != 0.0)
                nu[n - 1] = -1.0 * Qtf[n - 1] / R[n - 1, n - 1]; //normal case
            else
                nu[n - 1] = -1.0 * Qtf[n - 1] / temp; //special value

            //solve backwards
            for (int i = n - 2; i >= 0; i--)
            {
                double dotproductRnu = 0.0;
                for (int j = i + 1; j < n; j++)
                    dotproductRnu += R[i, j] * nu[j];

                if (R[i, i] == 0.0)
                    //special value
                    nu[i] = (-1.0 * Qtf[i] - dotproductRnu) / temp;
                else
                    nu[i] = (-1.0 * Qtf[i] - dotproductRnu) / R[i, i];
            }

            double nunorm = norm(nu);
            int tempFlag;
            if (nunorm < delta) //Newton direction
            {
                p = nu.ToArray();
                tempFlag = 1;
            }
            else //Newton direction not accepted
            {
                double[] g = opposite(matmul(transpose(R), Qtf));
                double gnorm = norm(g);
                double Jgnorm = norm(matmul(Q, matmul(R, g)));
                if (Jgnorm == 0.0) // special attention if steepest direction is zero
                {
                    p = mult(nu, delta / nunorm);
                    //flag=3;
                    tempFlag = 3; //??
                }
                //accept steepest descent direction
                else if (gnorm * gnorm * gnorm / (Jgnorm * Jgnorm) > delta) //not well explained?
                {
                    p = mult(g, delta / gnorm);
                    tempFlag = 2;
                }
                else //linear combination of both
                {
                    // calculate the weight of each direction
                    double mu = gnorm * gnorm / (Jgnorm * Jgnorm);
                    double[] mug = mult(mu, g);
                    double mugnorm = norm(mug);

                    double theta = (delta * delta - mugnorm * mugnorm) / (dotProduct(mug, subtract(nu, mug)) +
                        Math.Sqrt(pow(dotProduct(nu, mug) - delta * delta, 2) + (nunorm * nunorm - delta * delta) *
                        (delta*delta - mugnorm * mugnorm)));

                    p = add(mult((1.0 - theta) * mu, g), mult(theta, nu));
                    tempFlag = 3;
                }
            }
            flag = tempFlag;
        }

        static void QrFactorization(double[,] A, out double[,] Q, out double[,] R)
        {
            //Calculate QR factorizaton using Householder transformation. 
            //It finds orthogonal matrix Q and upper triangular R such that
            //A  = Q * [R; ZeroMatrix]
            //A: m by n (m>=n) input matrix for the QR factorization to be computed 
            //Q: m by m output orthogonal matrix 
            //R: m by n output upper triangular matrix. 

            int m = A.GetLength(0); //A rows
            int n = A.GetLength(1); //A columns

            //check size of the outputs
            //Q = new double[m,m];
            //R = new double[m,n];
            //int mQ1 = Q.GetLength(0); //Q rows
            //int mQ2 = Q.GetLength(1); //Q columns
            //int mR = R.GetLength(0); //R rows
            //int nR = R.GetLength(1); //R columns

            //if (n != nR || m != mQ1 || m != mQ2 || m != mR)
            //    throw new InvalidOperationException("QRfactorization : output matrix dimensions do not match with inputs");
            if (m < n)
                throw new InvalidOperationException("QRfactorization : number of rows must be equal or greater than number of columns");

            //main loop
            R = (double[,])A.Clone();
            Q = Matrix.eye(m, m);
            double[] u = new double[m];
            for (int i = 0; i < n; i++)
            {
                //check if all elements are already zero
                double normR = 0.0;
                for (int j = i; j < m; j++)
                {
                    u[j] = R[j, i];
                    normR += R[j, i] * R[j, i];
                }
                normR = Math.Sqrt(normR);
                u[i] -= normR;
                if (norm(u) == 0.0)
                    continue; //????

                //no need for nth column if m == n
                if (m == n && i == n - 1) break;

                //P is identity at the left top part
                double[,] P = Matrix.eye(m, m);

                //right bottom (m-i+1) by (m-i+1) part of P contains numbers
                double[,] eye = Matrix.eye(m - i, m - i);
                double[] uim = new double[m - i];
                for (int j = 0; j < uim.Length; j++)
                    uim[j] = u[j + i];
                double normuim = norm(uim);
                double[,] uimOuter = Matrix.outer(uim, uim);
                double normuim2 = normuim * normuim;
                for (int i2 = i; i2 < m; i2++)
                    for (int j2 = i; j2 < m; j2++)
                        P[i2, j2] = eye[i2 - i, j2 - i] - 2.0 * uimOuter[i2 - i, j2 - i] / normuim2;
                R = Matrix.matmul(P, R); //eliminate column i
                Q = Matrix.matmul(Q, P); //update Q

            }
        }

        static void QrUpdate(ref double[,] Q, ref double[,] R, double[] u, double[] v)
        {
            int N = u.Length;
            //double[] w = new double[N];
            //double[,] Qt = new double[N, N];
            double[,] Qt = transpose(Q);
            double[] w = matmul(Qt, u);

            double wnorm = norm(w);

            //make w to unit vector
            for (int i = N - 1; i >= 1; i--)
            {
                double c, s, t;
                //calculate cos and sin
                if (w[i - 1] == 0.0)
                {
                    c = 0.0; s = 1.0;
                }
                else
                {
                    t = w[i] / w[i - 1];
                    c = 1.0 / Math.Sqrt(t * t + 1.0);
                    s = t * c;
                }

                ApplyGivens(ref R, c, s, i - 1, i);
                ApplyGivens(ref Qt, c, s, i - 1, i);
                w[i - 1] = c * w[i - 1] + s * w[i];
                w[i] = c * w[i] - s * w[i - 1];
            }

            //update R
            for (int j = 0; j < N; j++)
                R[0, j] += w[0] * v[j];

            //transform upper Hessenberg matrix R to upper triangular matrix
            //H in the documentation is currently R
            for (int i = 0; i <= N - 2; i++)
            {
                double c, s, t;
                if (R[i, i] == 0.0)
                {
                    c = 0.0; s = 1.0;
                }
                else
                {
                    t = R[i + 1, i] / R[i, i];
                    c = 1.0 / Math.Sqrt(t * t + 1.0);
                    s = t * c;
                }
                ApplyGivens(ref R, c, s, i, i + 1);
                ApplyGivens(ref Qt, c, s, i, i + 1);
            }

            Q = Matrix.transpose(Qt);
        }


        static void ApplyGivens(ref double[,] A, double c2, double s2, int i2, int j2)
        {
            //store original input
            int N = A.GetLength(1); //get number of columns
            double[] ai = new double[N], aj = new double[N];
            for (int j = 0; j < N; j++)
            {
                ai[j] = A[i2, j];
                aj[j] = A[j2, j];
            }

            //only row i and row j changes
            for (int j = 0; j < N; j++)
            {
                A[i2, j] += (c2 - 1.0) * ai[j] + s2 * aj[j];
                //A[j2, j] += -s2 * ai[j] + (c2 - 1.0) * aj[j];
                A[j2, j] = A[j2, j] - s2 * ai[j] + (c2 - 1.0) * aj[j];
            }
        }

        static double[,] GetJacobian(Func<double[], double[]> fun, double[] x0, double epsilon, double[] fval)
        {
            //number of variables
            int m = x0.Length;
            //number of equations
            int n = fval.Length;

            double[,] jac = new double[n, m];

            for (int j = 0; j < m; j++)
            {
                //special treatment if x0 = 0
                double xdx = x0[j] == 0.0 ? 0.001 : x0[j] * (1.0 + epsilon);

                double[] xtemp = x0.ToArray();
                xtemp[j] = xdx;
                double[] fval1 = fun(xtemp); //evaluate function at xtemp
                for (int i = 0; i < n; i++)
                    jac[i, j] = (fval1[i] - fval[i]) / (xdx - x0[j]);
            }

            return jac;
        }

    }
}
