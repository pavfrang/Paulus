/*
 * 
 * BetaDistribution.cs, 27.03.2007
 * 
 * 09.08.2006: Initial version
 * 16.08.2006: Renamed fields storing GammaDistribution objects and declared them private explicitely
 * 27.03.2007: Overridden the now virtual base class method Reset to explicitely reset the underlying 
 *				 GammaDistributions of the BetaDistribution
 * 
 */

using System;

namespace Paulus.Random
{
    /// <summary>
    /// Provides generation of beta distributed random numbers.
    /// </summary>
    /// <remarks>
    /// The implementation of the <see cref="BetaDistribution"/> type bases upon information presented on
    ///   <a href="http://en.wikipedia.org/wiki/Beta_distribution">Wikipedia - Beta distribution</a> and
    ///   <a href="http://www.xycoon.com/beta_randomnumbers.htm">Xycoon - Beta Distribution</a>.
    /// </remarks>
    public class BetaDistribution : Distribution
    {
        #region instance fields
        /// <summary>
        /// Gets or sets the parameter alpha which is used for generation of beta distributed random numbers.
        /// </summary>
        /// <remarks>Call <see cref="IsValidAlpha"/> to determine whether a value is valid and therefor assignable.</remarks>
        public double Alpha
        {
            get
            {
                return this.alpha;
            }
            set
            {
                if (this.IsValidAlpha(value))
                {
                    this.alpha = value;
                    this.UpdateHelpers();
                }
            }
        }

        /// <summary>
        /// Stores the parameter alpha which is used for generation of beta distributed random numbers.
        /// </summary>
        private double alpha;

        /// <summary>
        /// Gets or sets the parameter beta which is used for generation of beta distributed random numbers.
        /// </summary>
        /// <remarks>Call <see cref="IsValidBeta"/> to determine whether a value is valid and therefor assignable.</remarks>
        public double Beta
        {
            get
            {
                return this.beta;
            }
            set
            {
                if (this.IsValidBeta(value))
                {
                    this.beta = value;
                    this.UpdateHelpers();
                }
            }
        }

        /// <summary>
        /// Stores the parameter beta which is used for generation of beta distributed random numbers.
        /// </summary>
        private double beta;

        /// <summary>
        /// Stores a <see cref="GammaDistribution"/> object used for generation of beta distributed random numbers
        ///   and configured with parameter <see cref="alpha"/>.
        /// </summary>
        private GammaDistribution gammaDistributionAlpha;

        /// <summary>
        /// Stores a <see cref="GammaDistribution"/> object used for generation of beta distributed random numbers
        ///   and configured with parameter <see cref="beta"/>.
        /// </summary>
        private GammaDistribution gammaDistributionBeta;
        #endregion

        #region construction
        /// <summary>
        /// Initializes a new instance of the <see cref="BetaDistribution"/> class, using a 
        ///   <see cref="StandardGenerator"/> as underlying random number generator.
        /// </summary>
        public BetaDistribution()
            : this(new StandardGenerator())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BetaDistribution"/> class, using the specified 
        ///   <see cref="Generator"/> as underlying random number generator.
        /// </summary>
        /// <param name="generator">A <see cref="Generator"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public BetaDistribution(Generator generator)
            : base(generator)
        {
            this.alpha = 1.0;
            this.beta = 1.0;
            this.gammaDistributionAlpha = new GammaDistribution(generator);
            this.gammaDistributionAlpha.Theta = 1.0;
            this.gammaDistributionBeta = new GammaDistribution(generator);
            this.gammaDistributionBeta.Theta = 1.0;
            this.UpdateHelpers();
        }
        #endregion

        #region instance methods
        /// <summary>
        /// Determines whether the specified value is valid for parameter <see cref="Alpha"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// <see langword="true"/> if value is greater than 0.0; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsValidAlpha(double value)
        {
            return value > 0.0;
        }

        /// <summary>
        /// Determines whether the specified value is valid for parameter <see cref="Beta"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// <see langword="true"/> if value is greater than 0.0; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsValidBeta(double value)
        {
            return value > 0.0;
        }

        /// <summary>
        /// Updates the helper variables that store intermediate results for generation of beta distributed random 
        ///   numbers.
        /// </summary>
        private void UpdateHelpers()
        {
            this.gammaDistributionAlpha.Alpha = this.alpha;
            this.gammaDistributionBeta.Alpha = this.beta;
        }
        #endregion

        #region overridden IDistribution members
        /// <summary>
        /// Resets the beta distribution, so that it produces the same random number sequence again.
        /// </summary>
        /// <returns>
        /// <see langword="true"/>, if the beta distribution was reset; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Reset()
        {
            bool result = base.Reset();
            if (result)
            {
                result = this.gammaDistributionAlpha.Reset();
                if (result)
                {
                    result = this.gammaDistributionBeta.Reset();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the minimum possible value of beta distributed random numbers.
        /// </summary>
        public override double Minimum
        {
            get
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Gets the maximum possible value of beta distributed random numbers.
        /// </summary>
        public override double Maximum
        {
            get
            {
                return 1.0;
            }
        }

        /// <summary>
        /// Gets the mean value of beta distributed random numbers.
        /// </summary>
        public override double Mean
        {
            get
            {
                return this.alpha / (this.alpha + this.beta);
            }
        }

        /// <summary>
        /// Gets the median of beta distributed random numbers.
        /// </summary>
        public override double Median
        {
            get
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Gets the variance of beta distributed random numbers.
        /// </summary>
        public override double Variance
        {
            get
            {
                return (this.alpha * this.beta) / (Math.Pow(this.alpha + this.beta, 2.0) * (this.alpha + this.beta + 1.0));
            }
        }

        /// <summary>
        /// Gets the mode of beta distributed random numbers.
        /// </summary>
        public override double[] Mode
        {
            get
            {
                if ((this.alpha > 1) && (this.beta > 1))
                {
                    return new double[] { (this.alpha - 1.0) / (this.alpha + this.beta - 2.0) };
                }
                else if ((this.alpha < 1) && (this.beta < 1))
                {
                    return new double[] { 0.0, 1.0 };
                }
                else if (((this.alpha < 1) && (this.beta >= 1)) || ((this.alpha == 1) && (this.beta > 1)))
                {
                    return new double[] { 0.0 };
                }
                else if (((this.alpha >= 1) && (this.beta < 1)) || ((this.alpha > 1) && (this.beta == 1)))
                {
                    return new double[] { 1.0 };
                }
                else
                {
                    return new double[] { };
                }
            }
        }

        public override double Skewness
        {
            get
            {
                return 2.0 * (beta - alpha) * Math.Sqrt(alpha + beta + 1.0) /
                    (alpha + beta + 2.0) / Math.Sqrt(alpha * beta);
            }
        }

        public override double Kurtosis
        {
            get
            {
                double a2 = alpha * alpha;
                double a3 = a2 * alpha;
                return 6.0 * (a3 - a2*(2.0*beta-1.0)+ beta * beta * (beta +1.0) -2.0 *alpha*beta*(beta+2.0))/
                alpha/beta/(alpha+beta+2.0)/(alpha+beta+3.0);
            }
        }



        /// <summary>
        /// Returns a beta distributed floating point random number.
        /// </summary>
        /// <returns>A beta distributed double-precision floating point number.</returns>
        public override double NextDouble()
        {
            double x = this.gammaDistributionAlpha.NextDouble();

            return x / (x + this.gammaDistributionBeta.NextDouble());
        }
        #endregion
    }
}