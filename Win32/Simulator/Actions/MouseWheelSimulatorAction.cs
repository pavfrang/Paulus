using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Paulus.Win32.Simulator.Actions
{
    public class MouseWheelSimulatorAction : MouseSimulatorAction
    {
        public MouseWheelSimulatorAction(WheelButton wheelButton, int wheelClicks, int delayBefore = 0, int delayAfter = 0)
            : this(wheelButton, wheelClicks, MousePositionType.Relative, Point.Empty, delayBefore, delayAfter) { }

        public MouseWheelSimulatorAction(WheelButton wheelButton, int wheelClicks, MousePositionType positionType, Point position, int delayBefore = 0, int delayAfter = 0)
            :base(MouseActionType.Wheel,positionType,position,delayBefore,delayAfter)
        {
            if (wheelButton != WheelButton.Horizontal && wheelButton != WheelButton.Vertical)
                throw new ArgumentException("Please specify a wheel button from the following values: WheelButton.Horizontal, WheelButton.Vertical.");

            Button = MouseButtons.None;
            WheelClicks = wheelClicks;
            WheelButton = wheelButton;
        }

        //public override string ToString()
        //{
        //    if (TimeElapsedSinceStart.HasValue)
        //        return string.Format("Mouse Wheel, Wheel: {0}, Position: {{X={1},Y={2}}}, Time: {3:0.000}", WheelButton, Dx, Dy, TimeElapsedSinceStart.Value.TotalSeconds);
        //    else
        //        return string.Format("Mouse Wheel, Wheel: {0}, Position: {{X={1},Y={2}}}", WheelButton, Dx, Dy);
        //}
    }
}
