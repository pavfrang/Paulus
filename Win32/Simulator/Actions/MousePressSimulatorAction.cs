using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Paulus.Win32.Simulator.Actions
{
    public class MousePressSimulatorAction : MouseSimulatorAction
    {
        public MousePressSimulatorAction(MouseButtons button, int delayBefore = 0, int delayAfter = 0)
            : this(button, MousePositionType.Relative, Point.Empty, delayBefore, delayAfter) { }

        public MousePressSimulatorAction(MouseButtons button, MousePositionType positionType, Point position, int delayBefore = 0, int delayAfter = 0)
            : base(MouseActionType.Press, positionType, position, delayBefore, delayAfter)
        {
            Button = button;
            WheelButton = WheelButton.None;
            WheelClicks = 0;
        }
    }
}

