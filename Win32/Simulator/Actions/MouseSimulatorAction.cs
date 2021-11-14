using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Paulus.Win32;

using System.Xml;

namespace Paulus.Win32.Simulator.Actions
{
    public enum WheelButton
    {
        None = 0,
        Vertical = 0x2000000,
        Horizontal = 0x4000000
    }

    public enum MouseActionType
    {
        Press,
        Release,
        Wheel,
        Move,
        Click,
        DoubleClick,
        MultiClick,
        Combination
        //DoubleClick //used for monitoring
    }

    public enum MousePositionType
    {
        Relative,
        /// <summary>
        /// Coordinates are relative to the top left corner of the primary screen.
        /// </summary>
        AbsoluteScreen,
        /// <summary>
        /// Coordinates are relative to the top left of the top left screen, so that coordinates are always positive numbers.
        /// </summary>
        AbsoluteVirtual //coordinates relative
    }

    public abstract class MouseSimulatorAction : SimulatorAction
    {
        #region Constructors

        public MouseSimulatorAction(MouseActionType actionType, MousePositionType positionType, Point position, int delayBefore=0, int delayAfter=0) {
            ActionType = actionType;
            PositionType = positionType;
            Position = position;
            DelayBefore = delayBefore;
            DelayAfter = delayAfter;
        }

        #endregion

        #region Main properties
        public MouseButtons Button { get; set; } //Left,Right, Middle, X1, X2
        public MouseActionType ActionType { get; set; } //Press or Release
        public MousePositionType PositionType { get; set; }
        //protected Point position;
        public Point Position { get { return new Point(Dx, Dy); } set { Dx = value.X; Dy = value.Y; } }
        protected int Dx, Dy;

        public WheelButton WheelButton {get;set;}
        public int WheelClicks { get; set; }

        public override string ToString()
        {
            return string.Format("<mouse action=\"{0}\" button=\"{1}\" movetype=\"{2}\" X=\"{3}\" Y=\"{4}\" wheel=\"{5}\" wheel_clicks=\"{6}\" delay_before=\"{7}\" delay_after=\"{8}\" />",
                ActionType, Button, PositionType, Position.X, Position.Y, WheelButton, WheelClicks,DelayBefore,DelayAfter);
        }

        public static MouseSimulatorAction Parse(XmlElement d)
        {
            #region Old text parsing
            ////List<string> tokens=s.GetTokensBetweenCharacters('"','"');
            //Dictionary<string, string> d = s.GetTokensDictionaryBetweenCharacters('"', '"', new string[] {
            //"action","button","movetype","X","Y","wheel","wheel_clicks","delay_before","delay_after"});
            //MouseActionType action = (MouseActionType)Enum.Parse(typeof(MouseActionType), d["action"]);
            //MouseButtons button = (MouseButtons)Enum.Parse(typeof(MouseButtons), d["button"]);
            //MousePositionType positionType = (MousePositionType)Enum.Parse(typeof(MousePositionType), d["movetype"]);
            //int Dx = int.Parse(d["X"]);
            //int Dy = int.Parse(d["Y"]);
            //Point position = new Point(Dx, Dy);
            //WheelButton wheelButton = (WheelButton)Enum.Parse(typeof(WheelButton), d["wheel"]);
            //int wheelClicks = int.Parse(d["wheel_clicks"]);
            //int delayBefore = int.Parse(d["delay_before"]);
            //int delayAfter = int.Parse(d["delay_after"]);
            #endregion

            MouseActionType action = (MouseActionType)Enum.Parse(typeof(MouseActionType), d.Attributes["action"].Value);
            MouseButtons button = (MouseButtons)Enum.Parse(typeof(MouseButtons), d.Attributes["button"].Value);
            MousePositionType positionType = (MousePositionType)Enum.Parse(typeof(MousePositionType), d.Attributes["movetype"].Value);
            int Dx = int.Parse(d.Attributes["X"].Value);
            int Dy = int.Parse(d.Attributes["Y"].Value);
            Point position = new Point(Dx, Dy);
            WheelButton wheelButton = (WheelButton)Enum.Parse(typeof(WheelButton), d.Attributes["wheel"].Value);
            int wheelClicks = int.Parse(d.Attributes["wheel_clicks"].Value);
            int delayBefore = int.Parse(d.Attributes["delay_before"].Value);
            int delayAfter = int.Parse(d.Attributes["delay_after"].Value);

            switch (action)
            {
                case MouseActionType.Move:
                    return new MouseMoveSimulatorAction(positionType, position, delayBefore, delayAfter);
                case MouseActionType.Press:
                    return new MousePressSimulatorAction(button, positionType, position, delayBefore, delayAfter);
                case MouseActionType.Release:
                    return new MouseReleaseSimulatorAction(button, positionType, position, delayBefore, delayAfter);
                case MouseActionType.Wheel:
                    return new MouseWheelSimulatorAction(wheelButton, wheelClicks, delayBefore, delayAfter);
                //case MouseActionType.Click:
                //    return new MouseClickSimulatorAction(button, positionType, position, delayBefore, delayAfter);
                default:
                    return null;
            }
        }

        public static MouseSimulatorAction Parse(string s)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(s);
            return Parse(doc.DocumentElement);
        }

        /// <summary>
        /// Created at 10-01-2014.
        /// </summary>
        /// <param name="mouseAction"></param>
        /// <returns></returns>
        public static explicit operator MouseEventArgs(MouseSimulatorAction mouseAction)
        {
            int clicks=0;
            if (mouseAction is MouseMultiClickSimulatorAction)
                clicks = (mouseAction as MouseMultiClickSimulatorAction).Actions.Count;
            else if (mouseAction is MouseDoubleClickSimulatorAction)
                clicks = 2;
            else if (mouseAction is MouseClickSimulatorAction)
                clicks = 1;

            //only the vertical wheel may be used here
            return new MouseEventArgs(mouseAction.Button, clicks, mouseAction.Dx, mouseAction.Dy, mouseAction.WheelClicks * User32.WHEEL_DELTA);
        }
        #endregion

        protected override User32.INPUT ToInputAPI()
        {
            User32.INPUT input = new User32.INPUT();
            User32.MOUSEINPUT mi = new User32.MOUSEINPUT();

            mi.mouseData = 0;
            mi.dwFlags = 0;

            #region Left, Right, Middle
            if (Button.HasFlag(MouseButtons.Left))
                if (ActionType == MouseActionType.Press) //we allow only one action here
                    mi.dwFlags |= User32.MOUSEEVENTF.LEFTDOWN;
                else
                    mi.dwFlags |= User32.MOUSEEVENTF.LEFTUP;

            if (Button.HasFlag(MouseButtons.Right))
                if (ActionType == MouseActionType.Press)
                    mi.dwFlags |= User32.MOUSEEVENTF.RIGHTDOWN;
                else
                    mi.dwFlags |= User32.MOUSEEVENTF.RIGHTUP;

            if (Button.HasFlag(MouseButtons.Middle))
                if (ActionType == MouseActionType.Press)
                    mi.dwFlags |= User32.MOUSEEVENTF.MIDDLEDOWN;
                else
                    mi.dwFlags |= User32.MOUSEEVENTF.MIDDLEUP;
            #endregion

            #region XButton1, XButton2, WheelButton
            if (Button.HasFlag(MouseButtons.XButton1))
            {
                if (ActionType == MouseActionType.Press)
                    mi.dwFlags |= User32.MOUSEEVENTF.XDOWN;
                else
                    mi.dwFlags |= User32.MOUSEEVENTF.XUP;
                mi.mouseData = (int)User32.MouseEventDataXButtons.XBUTTON1;
            }
            else if (Button == MouseButtons.XButton2)
            {
                if (ActionType == MouseActionType.Press)
                    mi.dwFlags |= User32.MOUSEEVENTF.XDOWN;
                else
                    mi.dwFlags |= User32.MOUSEEVENTF.XUP;
                mi.mouseData = (int)User32.MouseEventDataXButtons.XBUTTON2;
            }
            else if (WheelButton == WheelButton.Vertical)
            {
                mi.dwFlags |= User32.MOUSEEVENTF.WHEEL;
                mi.mouseData = WheelClicks * User32.WHEEL_DELTA;
            }
            else if (WheelButton == WheelButton.Horizontal)
            {
                mi.dwFlags |= User32.MOUSEEVENTF.HWHEEL;
                mi.mouseData = WheelClicks * User32.WHEEL_DELTA;
            }
            #endregion

            #region Position
            mi.dx = Dx;
            mi.dy = Dy;
            if (PositionType == MousePositionType.Relative)
                mi.dwFlags |= User32.MOUSEEVENTF.MOVE;
            else if (PositionType == MousePositionType.AbsoluteScreen)
            {
                mi.dwFlags |= User32.MOUSEEVENTF.ABSOLUTE | User32.MOUSEEVENTF.MOVE;
                mi.dx *= 65535 / User32.GetSystemMetrics(User32.SystemMetric.SM_CXSCREEN); // Screen.PrimaryScreen.Bounds.Width;
                mi.dy *= 65535 / User32.GetSystemMetrics(User32.SystemMetric.SM_CYSCREEN);  //Screen.PrimaryScreen.Bounds.Height;
            }
            else if (PositionType == MousePositionType.AbsoluteVirtual)
            {
                mi.dwFlags |= User32.MOUSEEVENTF.ABSOLUTE | User32.MOUSEEVENTF.VIRTUALDESK | User32.MOUSEEVENTF.MOVE;
                mi.dx *= 65535 / User32.GetSystemMetrics(User32.SystemMetric.SM_CXVIRTUALSCREEN);
                mi.dy *= 65535 / User32.GetSystemMetrics(User32.SystemMetric.SM_CYVIRTUALSCREEN);
            }
            #endregion

            mi.time = 0u;
            mi.dwExtraInfo = UIntPtr.Zero;
            input.type = User32.InputType.INPUT_MOUSE;
            input.U.mi = mi;
            return input;
        }
    }
}
