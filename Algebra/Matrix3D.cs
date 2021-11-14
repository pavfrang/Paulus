using System;
using System.Runtime.InteropServices;

namespace Paulus.Algebra
{
    //The elements are put in column order as they exist in memory in VB, Fortran AND in use for OpenGL
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix3D
    {
        public double m11, m21, m31, m41, //column specific just like in vb and fortran!
            m12, m22, m32, m42,
            m13, m23, m33, m43,
            m14, m24, m34, m44;

        public static void Identity(Matrix3D matrix)
        {
            matrix.m11 = 1.0; matrix.m12 = 0.0; matrix.m13 = 0.0;
            matrix.m21 = 0.0; matrix.m22 = 1.0; matrix.m23 = 0.0;
            matrix.m31 = 0.0; matrix.m32 = 0.0; matrix.m33 = 1.0;
        }
    }

}
