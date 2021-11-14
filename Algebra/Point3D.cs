using System;
using System.Runtime.InteropServices;

namespace Paulus.Algebra
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Point3D
    {
        public double X, Y, Z;

        #region Constructors
        public Point3D(double x, double y) //: this(x, y, 0)
        {
            X = x; Y = y; Z = 0D;
        }

        public Point3D(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public static implicit operator Point3D(double[] arr)
        {
            Point3D ret;
            ret.X = arr[0]; ret.Y = arr[1]; ret.Z = arr[2];
            return ret;
        }

        public static Point3D[] FromDoubleArray(double[,] arr)
        {
            Point3D[] points = new Point3D[arr.GetUpperBound(0) + 1];
            for (int i = 0; i <= arr.GetUpperBound(0); i++)
            {
                points[i].X = arr[i, 0];
                points[i].Y = arr[i, 1];
                points[i].Z = arr[i, 2];
            }
            return points;

        }
        #endregion

        #region Point functions
        public static bool operator ==(Point3D p1, Point3D p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z;
        }

        public static bool operator !=(Point3D p1, Point3D p2)
        {
            return p1.X != p2.X || p1.Y != p2.Y || p1.Z != p2.Z;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            //return this == (Point)obj;
            Point3D p2 = (Point3D)obj;
            return X == p2.X && Y == p2.Y && Z == p2.Z;
        }

        public static Point3D operator +(Point3D p)
        {
            return p;
        }

        public static Point3D operator -(Point3D p)
        {
            Point3D ret;
            ret.X = -p.X; ret.Y = -p.Y; ret.Z = -p.Z;
            return ret;
        }

        public static Point3D operator +(Point3D p, Vector v)
        {
            Point3D ret; ret.X = p.X + v.X; ret.Y = p.Y + v.Y; ret.Z = p.Z + v.Z;
            return ret;
        }

        public static Point3D operator -(Point3D p, Vector v)
        {
            Point3D ret; ret.X = p.X - v.X; ret.Y = p.Y - v.Y; ret.Z = p.Z - v.Z;
            return ret;
        }

        //returns the vector p2-p1 returns the vector 12
        public static Vector operator -(Point3D p2, Point3D p1)
        {
            Vector ret;
            ret.X = p2.X - p1.X; ret.Y = p2.Y - p1.Y; ret.Z = p2.Z - p1.Z;
            return ret;
        }

        public static double Distance(Point3D p1, Point3D p2)
        {
            double dx = p2.X - p1.X, dy = p2.Y - p1.Y, dz = p2.Z - p1.Z;
            double val = dx * dx + dy * dy + dz * dz;
            return Math.Sqrt(val);
        }

        /// <summary>
        /// Returns the distance of the point p from the origin.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double Distance(Point3D p) //euclidean distance
        {
            double val = p.X * p.X + p.Y * p.Y + p.Z * p.Z;
            return Math.Sqrt(val);
        }

        public static double ManhattanDistance(Point3D p1, Point3D p2)
        {
            double dx = p1.X >= p2.X ? p1.X - p2.X : p2.X - p1.X;
            double dy = p1.Y >= p2.Y ? p1.Y - p2.Y : p2.Y - p1.Y;
            //ok dz practically not needed
            return dx + dy;
        }

        public static Point3D RotateAroundAxis(Point3D p, Axis a, double angle, RotationType rotationType)
        {
            return a.Point + Vector.Rotate(p - a.Point, a.Vector, angle, rotationType);
        }
        #endregion

        public static readonly Point3D Empty = new Point3D();

        public override string ToString()
        {
            return string.Format("[{0} {1} {2}]", X, Y, Z);
        }

        public static Point3D Round(Point3D p, int decimals)
        {
            Point3D ret;
            ret.X = Math.Round(p.X, decimals);
            ret.Y = Math.Round(p.Y, decimals);
            ret.Z = Math.Round(p.Z, decimals);
            return ret;
        }
    }


    //public interface ICanAdd
    //{
    //    static ICanAdd operator +(ICanAdd t1,ICanAdd t2);
    //}

    //public struct tPoint3D<T>
    //{
    //    public T X, Y, Z;

    //    public static readonly tPoint3D<T> Empty = new tPoint3D<T>();

    //    public tPoint3D(T x, T y) //: this(x, y, 0)
    //    {
    //        X = x; Y = y; Z = default(T);
    //    }

    //    public tPoint3D(T x, T y, T z)
    //    {
    //        X = x; Y = y; Z = z;
    //    }

    //}

}