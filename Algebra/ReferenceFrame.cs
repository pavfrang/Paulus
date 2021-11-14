using System;
using System.Runtime.InteropServices;

namespace Paulus.Algebra
{
    public enum PlaneType
    {
        XY, YZ, ZX
    }

    public enum SystemType
    {
        Cartesian, Cylindrical, Spherical, Other
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct ReferenceFrame
    {
        public Vector UnitX, UnitY, UnitZ;
        public Point3D Start;

        public ReferenceFrame(Vector unitX, Vector unitY, Vector unitZ, Point3D start)
        {
            this.UnitX = unitX; this.UnitY = unitY; this.UnitZ = unitZ; this.Start = start;
        }

        public ReferenceFrame(Vector tilt, Vector unitX, Point3D start)
        // : this(unitX, tilt ^ unitX, tilt, start)  //avoid calling another constructor
        {
            this.Start = start; this.UnitX = unitX; this.UnitZ = tilt;
            //this.UnitY = tilt ^ unitX;
            this.UnitY.X = tilt.Y * unitX.Z - unitX.Y * tilt.Z;
            this.UnitY.Y = -tilt.X * unitX.Z + unitX.X * tilt.Z;
            this.UnitY.Z = tilt.X * unitX.Y - unitX.X * tilt.Y;
        }

        public ReferenceFrame(Vector v1, Vector v2, PlaneType planeType, Point3D start)
        {
            //the third vector is v1^v2 (cross product)
            switch (planeType)
            {
                case PlaneType.XY:
                    this.UnitX = v1; this.UnitY = v2; this.Start = start;
                    this.UnitZ.X = v1.Y * v2.Z - v2.Y * v1.Z;
                    this.UnitZ.Y = -v1.X * v2.Z + v2.X * v1.Z;
                    this.UnitZ.Z = v1.X * v2.Y - v2.X * v1.Y; break;
                case PlaneType.YZ:
                    this.UnitY = v1; this.UnitZ = v2; this.Start = start;
                    this.UnitX.X = v1.Y * v2.Z - v2.Y * v1.Z;
                    this.UnitX.Y = -v1.X * v2.Z + v2.X * v1.Z;
                    this.UnitX.Z = v1.X * v2.Y - v2.X * v1.Y; break;
                case PlaneType.ZX:
                default:
                    this.UnitX = v2; this.UnitZ = v1; this.Start = start;
                    this.UnitY.X = v1.Y * v2.Z - v2.Y * v1.Z;
                    this.UnitY.Y = -v1.X * v2.Z + v2.X * v1.Z;
                    this.UnitY.Z = v1.X * v2.Y - v2.X * v1.Y; break;
            }
        }

        public static ReferenceFrame operator +(ReferenceFrame s, Vector v)
        {
            ReferenceFrame ret = s;
            //ret.Start += v;
            ret.Start.X += v.X;
            ret.Start.Y += v.Y;
            ret.Start.Z += v.Z;
            return ret;
        }

        public static ReferenceFrame operator -(ReferenceFrame s, Vector v)
        {
            ReferenceFrame ret = s;
            //ret.Start -= v;
            ret.Start.X -= v.X;
            ret.Start.Y -= v.Y;
            ret.Start.Z -= v.Z;
            return ret;
        }

        public static ReferenceFrame RotateAroundAxis(ReferenceFrame s, Axis a, double angle, RotationType rotationType)
        {
            return new ReferenceFrame(Vector.Rotate(s.UnitX, a.Vector, angle, rotationType),
                Vector.Rotate(s.UnitY, a.Vector, angle, rotationType),
                Vector.Rotate(s.UnitZ, a.Vector, angle, rotationType),
                Point3D.RotateAroundAxis(s.Start, a, angle, rotationType));
        }

        public static bool IsValid(ReferenceFrame s, double tolerance)
        {
            return
                !( //if one of the following occurs then the system is not valid 
                //all frame vectors must be of value 1
                (Math.Abs(Vector.Length(s.UnitX) - 1.0) > tolerance ||
                Math.Abs(Vector.Length(s.UnitY) - 1.0) > tolerance ||
                Math.Abs(Vector.Length(s.UnitZ) - 1.0) > tolerance ||
                //check for a right-hand reference frame
                Vector.Length((s.UnitX ^ s.UnitY) - s.UnitZ) > tolerance ||
                Vector.Length((s.UnitY ^ s.UnitZ) - s.UnitX) > tolerance)
                );
        }

        /// <summary>
        /// Retrieve default cartesian system.
        /// </summary>
        /// <returns></returns>
        public static readonly ReferenceFrame Default = new ReferenceFrame(Vector.UnitX, Vector.UnitY, Vector.UnitZ, Point3D.Empty);

        public override string ToString()
        {
            return string.Format("P {0} Vx {1} Vy {2} Vz {3}", Start, UnitX, UnitY, UnitZ);
        }
    }

}