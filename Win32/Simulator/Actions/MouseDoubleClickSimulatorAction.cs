using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Paulus.Win32.Simulator.Actions
{
    public class MouseDoubleClickSimulatorAction : MouseMultiClickSimulatorAction
    {
        public MouseDoubleClickSimulatorAction(MouseButtons button, MousePositionType positionType, Point position, int delayBefore = 0, int delayAfter = 0)
            : base(button, 2, positionType, position, delayBefore, delayAfter)
        {
            ActionType = MouseActionType.DoubleClick;
        }

        public MouseDoubleClickSimulatorAction(MouseButtons button, int delayBefore = 0, int delayAfter = 0)
            : this(button, MousePositionType.Relative, Point.Empty, delayBefore, delayAfter) { }

        public static double GetDoubleClickSpeedInMs()
        {
            return MouseClickSimulatorAction.getDoubleClickSpeedInMsFromRegistry();
        }
    }
}
