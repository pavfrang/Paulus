/*
 * ContinuousUniformDistribution.cs, 21.09.2006
 * 
 * 09.08.2006: Initial version
 * 21.09.2006: Adapted to change in base class (field "generator" declared private (formerly protected) 
 *               and made accessible through new protected property "Generator")
 * 
 */

using System;
using Paulus.Random;

namespace Paulus.Random
{
	/// <summary>
    /// Provides generation of continuous uniformly distributed random numbers.
	/// </summary>
    /// <remarks>
    /// The implementation of the <see cref="ContinuousUniformDistribution"/> type bases upon information presented on
    ///   <a href="http://en.wikipedia.org/wiki/Uniform_distribution_%28continuous%29">
    ///   Wikipedia - Uniform distribution (continuous)</a>.
    /// </remarks>
    public class ContinuousUniformDistribution : Distribution
    {
        #region instance fields
        /// <summary>
        /// Gets or sets the parameter alpha which is used for generation of uniformly distributed random numbers.
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
        /// Stores the parameter alpha which is used for generation of uniformly distributed random numbers.
        /// </summary>
        private double alpha;

        /// <summary>
        /// Gets or sets the parameter beta which is used for generation of uniformly distributed random numbers.
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
        /// Stores the parameter beta which is used for generation of uniformly distributed random numbers.
        /// </summary>
        private double beta;

        /// <summary>
        /// Stores an intermediate result for generation of uniformly distributed random numbers.
        /// </summary>
        /// <remarks>
        /// Speeds up random number generation cause this value only depends on distribution parameters 
        ///   and therefor doesn't need to be recalculated in successive executions of <see cref="NextDouble"/>.
        /// </remarks>
        private double helper1;
        #endregion

        #region construction
        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuousUniformDistribution"/> class, using a 
        ///   <see cref="StandardGenerator"/> as underlying random number generator. 
        /// </summary>
        public ContinuousUniformDistribution()
            : this(new StandardGenerator())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuousUniformDistribution"/> class, using the specified 
        ///   <see cref="Generator"/> as underlying random number generator.
        /// </summary>
        /// <param name="generator">A <see cref="Generator"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public ContinuousUniformDistribution(Generator generator)
            : base(generator)
        {
            this.alpha = 0.0;
            this.beta = 1.0;
            this.UpdateHelpers();
        }
        #endregion

        #region instance methods
        /// <summary>
        /// Determines whether the specified value is valid for parameter <see cref="Alpha"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// <see langword="true"/> if value is less than or equal to <see cref="Beta"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsValidAlpha(double value)
        {
            return (value <= this.beta);
        }

        /// <summary>
        /// Determines whether the specified value is valid for parameter <see cref="Beta"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// <see langword="true"/> if value is greater than or equal to <see cref="Alpha"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsValidBeta(double value)
        {
            return (value >= this.alpha);
        }
        
        /// <summary>
        /// Updates the helper variables that store intermediate results for generation of uniformly distributed random 
        ///   numbers.
        /// </summary>
        private void UpdateHelpers()
        {
            this.helper1 = this.beta - this.alpha;
        }
        #endregion

        #region overridden Distribution members
        /// <summary>
        /// Gets the minimum possible value of uniformly distributed random numbers.
        /// </summary>
        public override double Minimum
        {
            get
            {
                return this.alpha;
            }
        }

        /// <summary>
        /// Gets the maximum possible value of uniformly distributed random numbers.
        /// </summary>
        public override double Maximum
        {
            get
            {
                return this.beta;
            }
        }

        /// <summary>
        /// Gets the mean value of the uniformly distributed random numbers.
        /// </summary>
        public override double Mean
        {
            get
            {
                return this.alpha / 2.0 + this.beta / 2.0;
            }
        }

        /// <summary>
        /// Gets the median of uniformly distributed random numbers.
        /// </summary>
        public override double Median
        {
            get
            {
                return this.alpha / 2.0 + this.beta / 2.0;
            }
        }

        /// <summary>
        /// Gets the variance of uniformly distributed random numbers.
        /// </summary>
        public override double Variance
        {
            get
            {
                return Math.Pow(this.beta - this.alpha, 2.0) / 12.0;
            }
        }

        /// <summary>
        /// Gets the mode of the uniformly distributed random numbers.
        /// </summary>
        public override double[] Mode
        {
            get
            {
                return new double[] { };
            }
        }

        /// <summary>
        /// Returns a uniformly distributed floating point random number.
        /// </summary>
        /// <returns>A uniformly distributed double-precision floating point number.</returns>
        public override double NextDouble()
        {
            return this.alpha + this.Generator.NextDouble() * this.helper1;
        }
        #endregion
    }
}