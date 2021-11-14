using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Paulus.Win32.Simulator.Actions
{
    public class MouseReleaseSimulatorAction : MouseSimulatorAction
    {
        public MouseReleaseSimulatorAction(MouseButtons button, int delayBefore = 0, int delayAfter = 0)
            :this(button,MousePositionType.Relative,Point.Empty,delayBefore,delayAfter){ }

        public MouseReleaseSimulatorAction(MouseButtons button,MousePositionType positionType,Point position,int delayBefore=0, int delayAfter=0)
            :base(MouseActionType.Release,positionType,position,delayBefore,delayAfter)
        {
            Button = button;
            WheelButton = WheelButton.None;
            WheelClicks = 0;
        }

        public MouseClickSimulatorAction GetMouseClickSimulatorActionWithTheSameProperties()
        {
            MouseClickSimulatorAction click = new MouseClickSimulatorAction(Button);
            click.TimeElapsedSinceStart = TimeElapsedSinceStart;
            click.ActionTime = ActionTime;
            click.hWnd = hWnd;
            click.Position = Position;
            click.PositionType = PositionType;

            return click;
        }
    }
}
