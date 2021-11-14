using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Paulus.Win32.Simulator.Actions
{
    public class MouseMoveSimulatorAction : MouseSimulatorAction
    {
        public MouseMoveSimulatorAction(MousePositionType positionType, Point position, int delayBefore=0, int delayAfter=0)
            :base(MouseActionType.Move,positionType,position,delayBefore,delayAfter)
        {
            if (!(new MousePositionType[] { MousePositionType.AbsoluteScreen, MousePositionType.Relative, MousePositionType.AbsoluteVirtual }).Contains(positionType))
                throw new ArgumentException("Please specify a move type from the following values MoveAction.Absolute, MoveAction.AbsoluteVirtualDesktop, MoveAction.Relative.");

            WheelClicks = 0;
            WheelButton = WheelButton.None;
            Button = MouseButtons.None;
        }

        public override bool Send()
        {
            if (PositionType == MousePositionType.AbsoluteScreen)
                Cursor.Position = Position;
            else if (PositionType == MousePositionType.Relative)
                Cursor.Position.Offset(Position);
            else
                return base.Send();

            return true;
        }

        //public MouseMoveSimulatorAction(MouseMoveType moveType, int x, int y) :
        //    this(moveType, new System.Drawing.Point(x, y)) { }

        //public override string ToString()
        //{
        //    if(TimeElapsedSinceStart.HasValue)
        //        return string.Format("Mouse Move, Position: {{X={0},Y={1}}}, Time: {2:0.000}",Dx,Dy,TimeElapsedSinceStart.Value.TotalSeconds);
        //    else
        //        return string.Format("Mouse Move, Position: {{X={0},Y={1}}}",Dx,Dy);
        //}
    }
}
