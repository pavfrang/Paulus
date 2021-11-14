using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Drawing;

namespace Paulus.Win32.Simulator.Actions
{
    public class MouseClickSimulatorAction : MouseCombinationSimulatorAction
    {
        public MouseClickSimulatorAction(MouseButtons button = MouseButtons.Left, int delayBefore = 0, int delayAfter = 0)
            : this(button, MousePositionType.Relative, Point.Empty, delayBefore, delayAfter) { }

        public MouseClickSimulatorAction(MouseButtons button, MousePositionType positionType, Point position, int delayBefore = 0, int delayAfter = 0)
            : base(delayBefore, delayAfter, new MousePressSimulatorAction(button), new MouseReleaseSimulatorAction(button))
        {
            Button = button;
            ActionType = MouseActionType.Click;
        }

        static MouseClickSimulatorAction()
        {
            doubleClickSpeedInMs = getDoubleClickSpeedInMsFromRegistry();
        }

        static int doubleClickSpeedInMs;

        //is accessed from MultiClick/DoubleClickSimulatorAction
        internal static int getDoubleClickSpeedInMsFromRegistry()
        {
            RegistryKey CU = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            RegistryKey controlPanel = CU.OpenSubKey("Control Panel");
            RegistryKey mouse = controlPanel.OpenSubKey("Mouse");
            return int.Parse((string)mouse.GetValue("DoubleClickSpeed"));
        }

        public bool IsCloseEnoughAndCapturesSameWindowForDoubleClick(MouseClickSimulatorAction secondClick)
        {
            return hWnd == secondClick.hWnd &&
                Math.Abs((secondClick.ActionTime.Value - ActionTime.Value).TotalMilliseconds) <= doubleClickSpeedInMs;
        }

        public MouseDoubleClickSimulatorAction GetMouseDoubleClickSimulatorActionWithTheSameProperties()
        {
            MouseDoubleClickSimulatorAction dblClick = new MouseDoubleClickSimulatorAction(Button);
            dblClick.TimeElapsedSinceStart = TimeElapsedSinceStart;
            dblClick.ActionTime = ActionTime;
            dblClick.hWnd = hWnd;
            dblClick.Position = Position;
            dblClick.PositionType = MousePositionType.AbsoluteScreen;

            return dblClick;
        }

        public MouseMultiClickSimulatorAction GetMouseMultiClickSimulatorActionWithTheSameProperties(int clicksCount)
        {
            MouseMultiClickSimulatorAction multiClick = new MouseMultiClickSimulatorAction(Button, clicksCount);
            multiClick.TimeElapsedSinceStart = TimeElapsedSinceStart;
            multiClick.ActionTime = ActionTime;
            multiClick.hWnd = hWnd;
            multiClick.Position = Position;
            multiClick.PositionType = MousePositionType.AbsoluteScreen;

            return multiClick;
        }
    }
}
