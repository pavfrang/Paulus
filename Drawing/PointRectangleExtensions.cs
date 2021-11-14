using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Paulus.Drawing
{
    public static class PointRectangleExtensions
    {
        static PointRectangleExtensions()
        {
            rnd = new Random();
        }

        static Random rnd;

        public static PointF GetRandomPoint(float minX, float maxX, float minY, float maxY)
        {
            PointF newPoint;
            lock(rnd)
                newPoint = new PointF( //this is a copy of the GetRandomPoint function
                    (float)(rnd.NextDouble() * (maxX - minX) + minX), (float)(rnd.NextDouble() * (maxY - minY) + minY));

            return newPoint;
        }

        public static PointF GetRandomPoint(this RectangleF restrictedArea)
        {
            float minX, maxX, minY, maxY;
            if (restrictedArea.Left < restrictedArea.Right)
            {
                minX = restrictedArea.Left; maxX = restrictedArea.Right;
            }
            else
            {
                minX = restrictedArea.Right; maxX = restrictedArea.Left;
            }

            if (restrictedArea.Bottom < restrictedArea.Top)
            {
                minY = restrictedArea.Bottom; maxY = restrictedArea.Top;
            }
            else
            {
                minY = restrictedArea.Top; maxY = restrictedArea.Bottom;
            }
            
            PointF newPoint;
            lock(rnd)
            newPoint= new PointF( //this is a copy of the GetRandomPoint function
                (float)(rnd.NextDouble() * (maxX - minX) + minX), (float)(rnd.NextDouble() * (maxY - minY) + minY)); 

            return newPoint;

        }

    }
}
