using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Paulus.Win32.Simulator
{
    public enum ClickType
    {
        Single = 1, Double = 2
    }
    
    public class SimpleMouseOperation : ICloneable
    {
        public SimpleMouseOperation(string description, Point position, MouseButtons button, ClickType clickType, int delay)
        {
            Position = position; Button = button; ClickType = clickType; Delay = delay; Description = description;
        }

        #region Properties
        public Point Position{get;set;}
        public MouseButtons Button{get;set;}
        public ClickType ClickType{get;set;}
        public int Delay{get;set;}
        public string Description{get;set;}

        public override string ToString()
        {
            return string.Format("{0} {1}", Description, Position);
        }
        #endregion

        public static explicit operator SimpleMouseOperation(string mouseOperationLine)
        {
            //x,y,button,clicktype,delay,description
            string[] values = mouseOperationLine.Split(',');
            return new SimpleMouseOperation(values[5],
                new Point(int.Parse(values[0]), int.Parse(values[1])),
                values[2].ToLower() == "left" ? MouseButtons.Left : MouseButtons.Right,
                values[3].ToLower() == "single" ? ClickType.Single : ClickType.Double,
                int.Parse(values[4]));
        }

        public static IEnumerable<SimpleMouseOperation> GetSimpleMouseOperations(IEnumerable<string> mouseOperationLines)
        {
            foreach (string mouseOperationLine in mouseOperationLines)
                yield return (SimpleMouseOperation)mouseOperationLine;
        }

        public object Clone()
        {
            return new SimpleMouseOperation(Description, Position, Button, ClickType, Delay);
        }
    }

}
