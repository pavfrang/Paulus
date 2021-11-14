using System;
using System.Runtime.InteropServices;

namespace Paulus.Algebra
{
    public enum RotationType : int
    {
        CounterClockwise, Clockwise,
        //CCW = CounterClockwise, CW = Clockwise //aliases
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector
    {
        public double X, Y, Z;

        #region Constructors
        public Vector(double x, double y) //: this(x, y, 0)
        {
            X = x; Y = y; Z = 0D;
        }

        public Vector(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public static implicit operator Vector(double[] arr)
        {
            Vector retv; retv.X = arr[0]; retv.Y = arr[1]; retv.Z = arr[2];
            return retv;
        }
        #endregion

        #region Operator overloading
        public static bool operator ==(Vector v1, Vector v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
        }

        public static bool operator !=(Vector v1, Vector v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            //return this == (Vector)obj;
            Vector v2 = (Vector)obj;
            return X == v2.X && Y == v2.Y && Z == v2.Z;
        }

        public static Vector operator +(Vector v)
        {
            return v;
        }

        public static Vector operator -(Vector v)
        {
            Vector ret; ret.X = -v.X; ret.Y = -v.Y; ret.Z = -v.Z;
            return ret;
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            Vector ret = v1;
            ret.X += v2.X; ret.Y += v2.Y; ret.Z += v2.Z;
            return ret;
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            Vector ret = v1;
            ret.X -= v2.X; ret.Y -= v2.Y; ret.Z -= v2.Z;
            return ret;
        }

        public static Vector operator *(double a, Vector v)
        {
            Vector ret = v;
            ret.X *= a; ret.Y *= a; ret.Z *= a;
            return ret;
        }

        public static Vector operator *(Vector v, double a)
        {
            Vector ret = v;
            ret.X *= a; ret.Y *= a; ret.Z *= a;
            return ret;
        }

        public static Vector operator *(Vector v1, Vector v2)
        {
            Vector ret = v1;
            ret.X *= v2.X; ret.Y *= v2.Y; ret.Z *= v2.Z;
            return ret;
        }

        public static Vector operator /(Vector v, double a)
        {
            Vector ret = v;
            ret.X /= a; ret.Y /= a; ret.Z /= a;
            return ret;
        }

        public static Vector operator /(Vector v1, Vector v2)
        {
            Vector ret = v1;
            ret.X /= v2.X; ret.Y /= v2.Y; ret.Z /= v2.Z;
            return ret;
        }

        /// <summary>
        /// Returns the cross product of two vectors.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector operator ^(Vector v1, Vector v2)
        {
            Vector ret;
            ret.X = v1.Y * v2.Z - v2.Y * v1.Z;
            ret.Y = -v1.X * v2.Z + v2.X * v1.Z;
            ret.Z = v1.X * v2.Y - v2.X * v1.Y;
            return ret;
        }

        /// <summary>
        /// Returns the normalized vector.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector operator !(Vector v)
        {
            double val = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            val = Math.Sqrt(val);

            Vector ret = v;
            ret.X /= val; ret.Y /= val; ret.Z /= val;
            return ret;
        }

        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double operator &(Vector v1, Vector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        //projects v1 on v2 and returns the result
        public static Vector operator %(Vector v1, Vector v2)
        {
            double vl = v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z;
            if (vl > 0)
            {
                double dotDivideValue = (v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z) / vl;
                Vector ret = v2;
                ret.X *= dotDivideValue;
                ret.Y *= dotDivideValue;
                ret.Z *= dotDivideValue;
                return ret;
                //return (v1 & v2) / vl * v2;
            }
            else
                return v1;
        }
        #endregion

        #region Vector functions

        /// <summary>
        /// Returns the length of the vector v.
        /// </summary>
        /// <param name="v">The vector whose length is calculated.</param>
        /// <returns></returns>
        public static double Length(Vector v)
        {
            double val = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            return Math.Sqrt(val);
        }

        //returns a rotated vector around u
        //(avoid any call to other functions to increase speed)
        public static Vector Rotate(Vector v, Vector u, double angle, RotationType rotationType)
        {
            Vector v2; //= v % u; //component of v on u
            //the following block calculates v % u
            double uLengthSquared = u.X * u.X + u.Y * u.Y + u.Z * u.Z;
            if (uLengthSquared > 0)
            {
                double dotDivideValue = (v.X * u.X + v.Y * u.Y + v.Z * u.Z) / uLengthSquared;
                v2 = u;
                v2.X *= dotDivideValue;
                v2.Y *= dotDivideValue;
                v2.Z *= dotDivideValue;
                //v2= (v & u) / vl * u;
            }
            else
                v2 = v;

            Vector vu = v; //= v - v2; //vertical to the u component of v
            vu.X -= v2.X; vu.Y -= v2.Y; vu.Z -= v2.Z;

            //find the unit vectors for the plane that u defines
            double lengthSquared = vu.X * vu.X + vu.Y * vu.Y + vu.Z * vu.Z; //=Value(vu)
            if (lengthSquared > 0D) //if there is a vertical to u component
            {
                Vector ex = vu; // = !vu;
                double vuLength = Math.Sqrt(lengthSquared);
                ex.X /= vuLength; ex.Y /= vuLength; ex.Z /= vuLength;

                Vector nu = u; // = !u;
                double nuLength = Math.Sqrt(uLengthSquared);
                nu.X /= nuLength; nu.Y /= nuLength; nu.Z /= nuLength;

                Vector ey;// = nu ^ ex;
                ey.X = nu.Y * ex.Z - ex.Y * nu.Z;
                ey.Y = -nu.X * ex.Z + ex.X * nu.Z;
                ey.Z = nu.X * ex.Y - ex.X * nu.Y;

                if (rotationType == RotationType.Clockwise) angle = -angle;
                //return vuLength * (System.Math.Cos(angle) * ex + System.Math.Sin(angle) * ey) + v2;
                Vector ret = v2;
                double vuLengthCos = vuLength * Math.Cos(angle);
                double vuLengthSin = vuLength * Math.Sin(angle);
                ret.X += vuLengthCos * ex.X + vuLengthSin * ey.X;
                ret.Y += vuLengthCos * ex.Y + vuLengthSin * ey.Y;
                ret.Z += vuLengthCos * ex.Z + vuLengthSin * ey.Z;
                return ret;
            }
            else //no rotation occurs (v and u are parallel)
            {
                return v;
            }
        }

        //public static double operator <(Vector v1, Vector v2)
        //{
        //    return Angle(v1, v2);
        //}

        //public static double operator >(Vector v1, Vector v2)
        //{
        //    return Angle(v2, v1);
        //}

        //returns angle in the range [0,pi]

        public static double Angle(Vector v1, Vector v2)
        {
            double vl1 = v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z;
            if (vl1 == 0D) return 0D;
            double vl2 = v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z;
            if (vl2 == 0D) return 0D;
            //calculate v1&v2/ (|v1|*|v2|) (returns the cosine of the angle)
            vl2 = (v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z) / Math.Sqrt(vl1) / Math.Sqrt(vl2);
            if (vl2 <= -1D) return Constants.PI;
            else if (vl2 >= 1D) return 0.0;
            else return Math.Acos(vl2);
        }

        //u must have a component in the direction of the v1^v2;
        //returns an angle in the [0,2pi) range
        public static double Angle2(Vector v1, Vector v2, Vector u, RotationType rotationType)
        {
            double angle; // = Angle(v1, v2);
            double vl1 = v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z;
            if (vl1 == 0D)
                angle = 0D;
            else
            {
                double vl2 = v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z;
                if (vl2 == 0D)
                    angle = 0D;
                //calculate v1&v2/ (|v1|*|v2|) (returns the cosine of the angle)
                else
                {
                    vl2 = (v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z) / Math.Sqrt(vl1) / Math.Sqrt(vl2);
                    if (vl2 <= -1D)
                        angle = Constants.PI;
                    else if (vl2 >= 1D) angle = 0.0;
                    else angle = Math.Acos(vl2);
                }
            }

            //u & (v1^v2)
            double vl = u.X * (v1.Y * v2.Z - v2.Y * v1.Z) + u.Y * (-v1.X * v2.Z + v2.X * v1.Z) + u.Z * (v1.X * v2.Y - v2.X * v1.Y);
            if (vl >= 0D && rotationType == RotationType.Clockwise || vl <= 0D && rotationType == RotationType.CounterClockwise)
                return Constants.PIMULT2 - angle;
            else return angle;
        }

        public static Vector Round(Vector v, int decimals)
        {
            Vector ret;
            ret.X = Math.Round(v.X, decimals);
            ret.Y = Math.Round(v.Y, decimals);
            ret.Z = Math.Round(v.Z, decimals);
            return ret;
        }
        #endregion

        #region Basic vectors (calculated only once)
        public static readonly Vector UnitX = new Vector(1.0, 0.0);
        public static readonly Vector UnitY = new Vector(0.0, 1.0);
        public static readonly Vector UnitZ = new Vector(0.0, 0.0, 1.0);
        public static readonly Vector Zero = new Vector();
        public static readonly Vector MinusUnitX = new Vector(-1.0, 0.0);
        public static readonly Vector MinusUnitY = new Vector(0.0, -1.0);
        public static readonly Vector MinusUnitZ = new Vector(0.0, 0.0, -1.0);
        #endregion

        public override string ToString()
        {
            return string.Format("[{0} {1} {2}]", X, Y, Z);
        }

    }

}