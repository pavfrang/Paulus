using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Paulus.Win32.Simulator.Actions
{
    public class MouseMultiClickSimulatorAction : MouseCombinationSimulatorAction
    {
        public MouseMultiClickSimulatorAction(MouseButtons button, int clicksCount, int delayBefore = 0, int delayAfter = 0) 
            : this(button, clicksCount, MousePositionType.Relative, Point.Empty, delayBefore, delayAfter) { }
    

        public MouseMultiClickSimulatorAction(MouseButtons button, int clicksCount,MousePositionType positionType, Point position,  int delayBefore=0, int delayAfter=0)
            :base(delayBefore,delayAfter,null)
        {
            _actions = new List<MouseSimulatorAction>();

            for (int iClick = 0; iClick < clicksCount; iClick++)
                _actions.Add(new MouseClickSimulatorAction(button));

            Button = button;

            ActionType = MouseActionType.MultiClick;
            PositionType = positionType;
            Position = position;
        }

        public static int GetDoubleClickSpeedInMsFromRegistry()
        {
            return MouseClickSimulatorAction.getDoubleClickSpeedInMsFromRegistry();
        }
    }
}
