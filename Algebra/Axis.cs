using System.Runtime.InteropServices;

namespace Paulus.Algebra
{
    public enum AxisType
    {
        AxisX, AxisY, AxisZ
    }   
    
    /// <summary>
    /// An axis is defined by a point that belongs to the axis and a vector that is parallel to the axis.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Axis
    {
        public Vector Vector;
        public Point3D Point;

        public Axis(Vector v, Point3D p)
        {
            this.Vector = v; this.Point = p;
        }

        public static implicit operator Axis(Vector v)
        {
            Axis ax; ax.Vector = v; ax.Point = Point3D.Empty;
            return ax;
        }

        public override bool Equals(object obj)
        {
            Axis other = (Axis)obj;
            return Vector == other.Vector && Point == other.Point;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Axis axis1, Axis axis2)
        {
            return axis1.Vector == axis2.Vector && axis1.Point == axis2.Point;
        }

        public static bool operator !=(Axis axis1, Axis axis2)
        {
            return axis1.Vector != axis2.Vector || axis1.Point != axis2.Point;
        }

        public static readonly Axis AxisX = new Axis(Vector.UnitX, Point3D.Empty);
        public static readonly Axis AxisY = new Axis(Vector.UnitY, Point3D.Empty);
        public static readonly Axis AxisZ = new Axis(Vector.UnitZ, Point3D.Empty);

        public override string ToString()
        {
            return string.Format("V {0} P {1}", Vector.ToString(), Point.ToString());
        }

    }

}
