/*
 * ErlangDistribution.cs, 21.09.2006
 * 
 * 16.08.2006: Initial version
 * 21.09.2006: Adapted to change in base class (field "generator" declared private (formerly protected) 
 *               and made accessible through new protected property "Generator")
 * 
 */

using System;
using Paulus.Random;

namespace Paulus.Random
{
	/// <summary>
	/// Provides generation of erlang distributed random numbers.
	/// </summary>
	/// <remarks>
    /// The implementation of the <see cref="ErlangDistribution"/> type bases upon information presented on
    ///   <a href="http://en.wikipedia.org/wiki/Erlang_distribution">Wikipedia - Erlang distribution</a> and
    ///   <a href="http://www.xycoon.com/erlang_random.htm">Xycoon - Erlang Distribution</a>.
    /// </remarks>
	public class ErlangDistribution : Distribution
	{
		#region instance fields
		/// <summary>
		/// Gets or sets the parameter alpha which is used for generation of erlang distributed random numbers.
		/// </summary>
		/// <remarks>Call <see cref="IsValidAlpha"/> to determine whether a value is valid and therefor assignable.</remarks>
		public int Alpha
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
                }
        	}
		}

		/// <summary>
		/// Stores the parameter alpha which is used for generation of erlang distributed random numbers.
		/// </summary>
		private int alpha;
		
		/// <summary>
		/// Gets or sets the parameter lambda which is used for generation of erlang distributed random numbers.
		/// </summary>
        /// <remarks>Call <see cref="IsValidLambda"/> to determine whether a value is valid and therefor assignable.</remarks>
		public double Lambda
		{
			get
			{
				return this.lambda;
			}
			set
			{
                if (this.IsValidLambda(value))
                {
                    this.lambda = value;
                    this.theta = 1 / value ;
                    this.UpdateHelpers();
                }
        	}
		}

        private double theta;
        public double Theta
        {
            get
            {
                return this.theta;
            }
            set
            {
                if (this.IsValidLambda(1.0/value))
                {
                    this.theta = value;
                    this.lambda = 1 / value;
                    this.UpdateHelpers();
                }
            }
        }

		/// <summary>
		/// Stores the parameter lambda which is used for generation of erlang distributed random numbers.
		/// </summary>
		private double lambda;

        /// <summary>
        /// Stores an intermediate result for generation of erlang distributed random numbers.
        /// </summary>
        /// <remarks>
        /// Speeds up random number generation cause this value only depends on distribution parameters 
        ///   and therefor doesn't need to be recalculated in successive executions of <see cref="NextDouble"/>.
        /// </remarks>
        private double helper1;
        #endregion

		#region construction
		/// <summary>
        /// Initializes a new instance of the <see cref="ErlangDistribution"/> class, using a 
        ///   <see cref="StandardGenerator"/> as underlying random number generator.
		/// </summary>
        public ErlangDistribution()
            : this(new StandardGenerator())
		{
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="ErlangDistribution"/> class, using the specified 
        ///   <see cref="Generator"/> as underlying random number generator.
        /// </summary>
        /// <param name="generator">A <see cref="Generator"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public ErlangDistribution(Generator generator)
            : base(generator)
        {
            this.alpha = 1;
            this.lambda = 1.0;
            this.theta = 1.0; //alternative input (theta=1/lambda)
            this.UpdateHelpers();
        }
		#endregion
	
		#region instance methods
		/// <summary>
        /// Determines whether the specified value is valid for parameter <see cref="Alpha"/>.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <returns>
		/// <see langword="true"/> if value is greater than 0; otherwise, <see langword="false"/>.
		/// </returns>
		public bool IsValidAlpha(int value)
		{
			return value > 0;
		}
		
		/// <summary>
		/// Determines whether the specified value is valid for parameter <see cref="Lambda"/>.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <returns>
		/// <see langword="true"/> if value is greater than 0.0; otherwise, <see langword="false"/>.
		/// </returns>
        public bool IsValidLambda(double value)
		{
			return value > 0.0;
		}

        /// <summary>
        /// Updates the helper variables that store intermediate results for generation of erlang distributed random 
        ///   numbers.
        /// </summary>
        private void UpdateHelpers()
        {
            this.helper1 = -1.0 / this.lambda;
        }

        #endregion

		#region overridden Distribution members
        /// <summary>
		/// Gets the minimum possible value of erlang distributed random numbers.
		/// </summary>
		public override double Minimum
		{
			get
			{
				return 0.0;
			}
		}

		/// <summary>
		/// Gets the maximum possible value of erlang distributed random numbers.
		/// </summary>
        public override double Maximum
		{
			get
			{
				return double.MaxValue;
			}
		}

		/// <summary>
		/// Gets the mean value of erlang distributed random numbers.
		/// </summary>
        public override double Mean
		{
			get
			{
                return this.alpha / this.lambda;
			}
		}
		
		/// <summary>
		/// Gets the median of erlang distributed random numbers.
		/// </summary>
        public override double Median
		{
			get
			{
				return double.NaN;
			}
		}
		
		/// <summary>
		/// Gets the variance of erlang distributed random numbers.
		/// </summary>
        public override double Variance
		{
			get
			{
                return this.alpha / Math.Pow(this.lambda, 2.0);
			}
		}
		
		/// <summary>
		/// Gets the mode of erlang distributed random numbers.
		/// </summary>
        public override double[] Mode
		{
            get
            {
                return new double[] { (this.alpha - 1) / this.lambda };
            }
		}

        public override double Skewness
        {
            get
            {
                return 2.0/Math.Sqrt((double)alpha);
            }
        }

        public override double Kurtosis
        {
            get
            {
                return 6.0 / alpha ;
            }
        }
		
		/// <summary>
		/// Returns a erlang distributed floating point random number.
		/// </summary>
		/// <returns>A erlang distributed double-precision floating point number.</returns>
        public override double NextDouble()
		{
            double product = 1.0;
            for (int i = 0; i < this.alpha; i++)
            {
                double rnd=this.Generator.NextDouble();
                product *= rnd !=0 ? rnd: Generator.NextDouble();
            }

            // Subtract product from 1.0 to avoid Math.Log(0.0)
            return this.helper1 * Math.Log(product);
		}
		#endregion
	}
}