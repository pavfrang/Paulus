using Paulus.Win32.Simulator.Actions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Paulus.Win32.Simulator
{

    /// <summary>
    /// Operations that simulate mouse events. (v1.3)
    /// </summary>
    public static class MouseSimulator
    {
        public static bool ButtonDown(MouseButtons button)
        {
            return new MousePressSimulatorAction(button).Send();
        }

        public static bool ButtonUp(MouseButtons button)
        {
            return new MouseReleaseSimulatorAction(button).Send();
        }

        public static bool Wheel(WheelButton button, int wheelClicks)
        {
            return new MouseWheelSimulatorAction(button, wheelClicks).Send();
        }

        public static bool Move(MousePositionType positionType, Point position)
        {
            return new MouseMoveSimulatorAction(positionType, position).Send();
        }

        public static bool Move(MousePositionType positionType, int x, int y)
        {
            return new MouseMoveSimulatorAction(positionType, new Point(x, y)).Send();
        }

        public static bool MouseCombination(params MouseSimulatorAction[] mouseEvents)
        {
            return new MouseCombinationSimulatorAction(mouseEvents).Send();
            //return SimulatorAction.Send(mouseEvents);
        }

        public static bool MouseCombination(IEnumerable<MouseSimulatorAction> mouseEvents)
        {
            return new MouseCombinationSimulatorAction(mouseEvents).Send();
            //return SimulatorAction.Send(mouseEvents);
        }

        public static bool Click(MouseButtons button = MouseButtons.Left)
        {
            return new MouseClickSimulatorAction(button).Send();
        }

        public static bool DoubleClick(MouseButtons button = MouseButtons.Left)
        {
            return new MouseDoubleClickSimulatorAction(button).Send();
        }

        public static bool MultiClick(MouseButtons button, int clicksCount)
        {
            return new MouseMultiClickSimulatorAction(button, clicksCount).Send();
        }  
    }

}
