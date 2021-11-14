using System;
using System.Runtime.InteropServices;

namespace Paulus.Algebra
{
    /// <summary>
    /// Alternative definition of the plane using a normal vector and a point that belongs to the plane.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Plane
    {
        public Vector V;
        public Point3D P;

        public Plane(Vector v, Point3D p)
        {
            this.V = v; this.P = p;
        }

        /// <summary>
        /// Plane constructor defined from three points in CCW manner.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        public Plane(Point3D p1, Point3D p2, Point3D p3)
        {
            P = p1; //any of the three points
            Vector v1 = p2 - p1;
            Vector v2 = p3 - p1;
            V = !(v1 ^ v2);
        }

        public static Plane operator -(Plane plane)
        {
            return new Plane(-plane.V, plane.P);
        }

        public override string ToString()
        {
            return string.Format("N {0} P {1}", V.ToString(), P.ToString());
        }
    }

    /// <summary>
    /// Represents a plane that is defined by the equation Ax+By+Cz+D=0
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Plane2
    {
        public double A, B, C, D;

        public Plane2(double a, double b, double c, double d)
        {
            this.A = a; this.B = b; this.C = c; this.D = d;
        }

        public override string ToString()
        {
            return string.Format("{0}x+{1}y+{2}z+{3}=0", A, B, C, D);
        }
    }




}