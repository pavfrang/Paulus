using Paulus.Win32;
using Paulus.Win32.Simulator.Actions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Paulus.Win32.Simulator
{
    public enum ClickTrackerBehavior
    {
        DontTrack,
        Clicks,
        ClicksAndDoubleClicks,
        /// <summary>
        /// Track MultiClicks prevents DoubleClicks from tracking.
        /// </summary>
        ClicksAndMultiClicks
    }

    public class MouseRecorder : IDisposable
    {
        public MouseRecorder()
        {
            FillUpDownWindowMessagesList(); // Enum.GetValues(typeof(MouseClickRelatedAction)).Cast<MouseClickRelatedAction>().ToList();
            _clickTrackerBehavior = ClickTrackerBehavior.ClicksAndDoubleClicks;
            _multiClicksCountToTrack = 3;
        }

        //private enum MouseClickRelatedAction : uint
        //{
        //    LeftButtonDown = User32.WM.LBUTTONDOWN,
        //    LeftButtonUp = User32.WM.LBUTTONUP,
        //    RightButtonDown = User32.WM.RBUTTONDOWN,
        //    RightButtonUp = User32.WM.RBUTTONUP,
        //    MiddleButtonDown = User32.WM.MBUTTONDOWN,
        //    MiddleButtonUp = User32.WM.MBUTTONUP,
        //    XButtonDown = User32.WM.XBUTTONDOWN,
        //    XButtonUp = User32.WM.XBUTTONUP,
        //    //LeftButtonDoubleClick = User32.WM.LBUTTONDBLCLK,
        //    //RightButtonDoubleClick = User32.WM.RBUTTONDBLCLK,
        //    //MiddleButtonDoubleClick = User32.WM.MBUTTONDBLCLK,
        //    //XButtonDoubleClick = User32.WM.XBUTTONDBLCLK,
        //    //NonClientLeftButtonDoubleClick = User32.WM.NCLBUTTONDBLCLK,
        //    //NonClientLeftButtonDown = User32.WM.NCLBUTTONDOWN,
        //    //NonClientLeftButtonUp = User32.WM.NCLBUTTONUP,
        //    //NonClientRightButtonDoubleClick = User32.WM.NCRBUTTONDBLCLK,
        //    //NonClientRightButtonDown = User32.WM.NCRBUTTONDOWN,
        //    //NonClientRightButtonUp = User32.WM.NCRBUTTONUP,
        //    //NonClientMiddleButtonDoubleClick = User32.WM.NCMBUTTONDBLCLK,
        //    //NonClientMiddleButtonDown = User32.WM.NCMBUTTONDOWN,
        //    //NonClientMiddleButtonUp = User32.WM.NCMBUTTONUP,
        //    //NonClientXButtonDoubleClick = User32.WM.NCXBUTTONDBLCLK,
        //    //NonClientXButtonDown = User32.WM.NCXBUTTONDOWN,
        //    //NonClientXButtonUp = User32.WM.NCXBUTTONUP
        //}

        #region Events
        public event EventHandler RecordingStarted;
        protected void OnTrackingStarted()
        {
            EventHandler handler = RecordingStarted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler RecordingFinished;
        protected void OnTrackingFinished()
        {
            EventHandler handler = RecordingFinished;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Used to track cursor motion. Note that the mouse button is not tracked, so the Button property of the mouseAction is not set.
        /// </summary>
        public event EventHandler<MouseSimulatorActionEventArgs> MouseMoveTracked;
        protected void OnMouseMoveTracked(MouseSimulatorAction mouseAction)
        {
            var handler = MouseMoveTracked;
            if (handler != null) handler(this, new MouseSimulatorActionEventArgs(mouseAction));
        }

        public event EventHandler<MouseSimulatorActionEventArgs> MouseWheelTracked;
        protected void OnMouseWheelTracked(MouseSimulatorAction mouseAction)
        {
            var handler = MouseWheelTracked;
            if (handler != null) handler(this, new MouseSimulatorActionEventArgs(mouseAction));
        }

        public event EventHandler<MouseSimulatorActionEventArgs> MousePressTracked;
        private MousePressSimulatorAction lastMousePressAction;
        protected void OnMousePressTracked(MouseSimulatorAction mouseAction)
        {
            lastMousePressAction = (MousePressSimulatorAction)mouseAction;

            var handler = MousePressTracked;
            if (handler != null) handler(this, new MouseSimulatorActionEventArgs(mouseAction));
        }

        public event EventHandler<MouseSimulatorActionEventArgs> MouseReleaseTracked;
        protected void OnMouseReleaseTracked(MouseSimulatorAction mouseAction)
        {
            var handler = MouseReleaseTracked;
            if (handler != null) handler(this, new MouseSimulatorActionEventArgs(mouseAction));
        }

        public event EventHandler<MouseSimulatorActionEventArgs> MouseClickTracked;
        protected void OnMouseClickTracked(MouseSimulatorAction mouseAction)
        {
            var handler = MouseClickTracked;
            if (handler != null) handler(this, new MouseSimulatorActionEventArgs(mouseAction));
        }

        public event EventHandler<MouseSimulatorActionEventArgs> MouseDoubleClickTracked;
        protected void OnMouseDoubleClickTracked(MouseSimulatorAction mouseAction)
        {
            var handler = MouseDoubleClickTracked;
            if (handler != null) handler(this, new MouseSimulatorActionEventArgs(mouseAction));
        }

        public event EventHandler<MouseSimulatorActionEventArgs> MouseMultiClickTracked;
        protected void OnMouseMultiClickTracked(MouseSimulatorAction mouseAction)
        {
            var handler = MouseMultiClickTracked;
            if (handler != null) handler(this, new MouseSimulatorActionEventArgs(mouseAction));
        }

        /// <summary>
        /// Should be used if all tracked mouse actions are needed.
        /// </summary>
        public event EventHandler<MouseSimulatorActionEventArgs> MouseActionTracked;
        protected void OnMouseActionTracked(MouseSimulatorAction mouseAction)
        {
            _recordedMouseActions.Add(mouseAction);

            var handler = MouseActionTracked;
            if (handler != null) handler(this, new MouseSimulatorActionEventArgs(mouseAction));
        }

        #endregion

        #region Tracking
        private List<MouseSimulatorAction> _recordedMouseActions;
        public List<MouseSimulatorAction> RecordedMouseActions
        {
            get { return _recordedMouseActions; }
            set { _recordedMouseActions = value; }
        }
        /// <summary>
        /// Installs low level mouse hook. This hook raises events every time a mouse event occured. Returns true if the tracking is successful.
        /// </summary>
        public bool StartRecording()
        {
            //release all previously tracked values
            if (_recordedMouseActions == null) _recordedMouseActions = new List<MouseSimulatorAction>();
            else _recordedMouseActions.Clear();

            //attempt to hook
            //uint hInstance =(uint)System.Threading.Thread.CurrentThread.ManagedThreadId;
            //Marshal.GetHINSTANCE(System.Reflection.Assembly.GetCallingAssembly().GetModules()[0]);
            callbackDelegate = new User32.HookProc(MouseHookCallback);
            ptrMouseHook = User32.SetWindowsHookEx(User32.HookType.WH_MOUSE_LL, callbackDelegate, IntPtr.Zero, 0u);

            //hook installed successfully    
            if (ptrMouseHook != IntPtr.Zero)
            {
                firstActionTicks = GetTickCount();
                firstActionTime = DateTime.Now;
                OnTrackingStarted();
            }

            return ptrMouseHook != IntPtr.Zero;
        }

        /// <summary>
        /// Uninstalls the low level mouse hook. Hooks events are stopped from being raised.
        /// </summary>
        public bool StopRecording()
        {
            bool succeeded = User32.UnhookWindowsHookEx(ptrMouseHook);
            if (succeeded)
            {
                ptrMouseHook = IntPtr.Zero;
                callbackDelegate = null;
                OnTrackingFinished();
            }
            return succeeded;
        }

        #region Hook internals
        private IntPtr ptrMouseHook;
        private User32.HookProc callbackDelegate;

        //needed to check against valid values only (initialized at contructor level and used inside the hooked function)
        private List<User32.WM> upDownWindowMessages;
        private void FillUpDownWindowMessagesList()
        {
            upDownWindowMessages = new User32.WM[]{
                    User32.WM.LBUTTONDOWN, User32.WM.LBUTTONUP,
                    User32.WM.RBUTTONDOWN, User32.WM.RBUTTONUP,
                    User32.WM.MBUTTONDOWN,User32.WM.MBUTTONUP,
                    User32.WM.XBUTTONDOWN,User32.WM.XBUTTONUP}.ToList();
        }

        //time related
        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();
        private uint firstActionTicks;
        private DateTime firstActionTime;
        private MouseSimulatorAction lastActionRecorded;

        //MSLLHOOKSTRUCT hook = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
        private IntPtr MouseHookCallback(int nCode, User32.WM wParam, ref User32.MSLLHOOKSTRUCT hook)
        {
            if (nCode == User32.HC_ACTION)
            {
                bool validUpOrDownMessage = upDownWindowMessages.Contains(wParam);

                if (wParam == User32.WM.MOUSEMOVE || wParam == User32.WM.NCMOUSEMOVE ||
                    wParam == User32.WM.MOUSEHWHEEL || wParam == User32.WM.MOUSEWHEEL || validUpOrDownMessage)
                {
                    MouseSimulatorAction simpleAction = null;

                    if (wParam == User32.WM.MOUSEMOVE || wParam == User32.WM.NCMOUSEMOVE)
                        simpleAction = new MouseMoveSimulatorAction(MousePositionType.AbsoluteScreen, hook.pt);
                    else if (validUpOrDownMessage)
                        simpleAction = getMousePressOrReleaseAction(wParam, hook);
                    else if (wParam == User32.WM.MOUSEWHEEL || wParam == User32.WM.MOUSEHWHEEL)
                    {
                        WheelButton wheel = wParam == User32.WM.MOUSEWHEEL ? WheelButton.Vertical : WheelButton.Horizontal;
                        simpleAction = new MouseWheelSimulatorAction(wheel, hook.mouseDataHigh / User32.WHEEL_DELTA);
                        simpleAction.Position = Point.Empty;
                        simpleAction.PositionType = MousePositionType.Relative;
                    }

                    simpleAction.PositionType = MousePositionType.AbsoluteScreen;
                    simpleAction.Position = hook.pt;

                    simpleAction.TimeElapsedSinceStart = new TimeSpan(10000 * (hook.time - firstActionTicks));
                    simpleAction.ActionTime = firstActionTime + simpleAction.TimeElapsedSinceStart;
                    if (lastActionRecorded != null) simpleAction.DelayBefore = (int)(simpleAction.ActionTime.Value - lastActionRecorded.ActionTime.Value).TotalMilliseconds;

                    if (!(simpleAction is MouseMoveSimulatorAction)) simpleAction.hWnd = User32.WindowFromPoint(hook.pt);

                    raiseEventsForTrackedAction(simpleAction);

                    lastActionRecorded = simpleAction;
                }
            }

            //calls the next hook chain
            return User32.CallNextHookEx(ptrMouseHook, nCode, wParam, ref hook);
        }

        private void raiseEventsForTrackedAction(MouseSimulatorAction action)
        {
            if (action is MousePressSimulatorAction)
                OnMousePressTracked(action);
            else if (action is MouseReleaseSimulatorAction)
                OnMouseReleaseTracked(action);
            else if (action is MouseMoveSimulatorAction)
                OnMouseMoveTracked(action);
            else if (action is MouseWheelSimulatorAction)
                OnMouseWheelTracked(action);

            OnMouseActionTracked(action);

            //check for clicks, double clicks, multi clicks based on the multiclicktrackerbehavior
            if (action is MouseReleaseSimulatorAction &&
                _clickTrackerBehavior != ClickTrackerBehavior.DontTrack)
                checkAndRaiseEventForClicks((MouseReleaseSimulatorAction)action);
        }

        private MouseSimulatorAction getMousePressOrReleaseAction(User32.WM wParam, User32.MSLLHOOKSTRUCT hook)
        {
            short highWord = hook.mouseDataHigh;

            MouseButtons b;
            switch (wParam)
            {
                default:
                case User32.WM.LBUTTONDOWN:
                case User32.WM.LBUTTONUP:
                    //case User32.WM.LBUTTONDBLCLK:
                    //case User32.WM.NCLBUTTONDBLCLK:
                    //case User32.WM.NCLBUTTONDOWN:
                    //case User32.WM.NCLBUTTONUP:
                    b = MouseButtons.Left; break;
                case User32.WM.RBUTTONDOWN:
                case User32.WM.RBUTTONUP:
                    //case User32.WM.RBUTTONDBLCLK:
                    //case User32.WM.NCRBUTTONDBLCLK:
                    //case User32.WM.NCRBUTTONDOWN:
                    //case User32.WM.NCRBUTTONUP:
                    b = MouseButtons.Right; break;
                case User32.WM.MBUTTONDOWN:
                case User32.WM.MBUTTONUP:
                    //case User32.WM.MBUTTONDBLCLK:
                    //case User32.WM.NCMBUTTONDBLCLK:
                    //case User32.WM.NCMBUTTONDOWN:
                    //case User32.WM.NCMBUTTONUP:
                    b = MouseButtons.Middle; break;
                case User32.WM.XBUTTONDOWN:
                case User32.WM.XBUTTONUP:
                    //case User32.WM.XBUTTONDBLCLK:
                    //case User32.WM.NCXBUTTONDBLCLK:
                    //case User32.WM.NCXBUTTONDOWN:
                    //case User32.WM.NCXBUTTONUP:
                    User32.MouseEventDataXButtons xButton =
                        (User32.MouseEventDataXButtons)(highWord);
                    b = xButton == User32.MouseEventDataXButtons.XBUTTON1 ? MouseButtons.XButton1 : MouseButtons.XButton2; break;
            }

            MouseActionType a;
            switch (wParam)
            {
                //case User32.WM.LBUTTONDBLCLK:
                //case User32.WM.RBUTTONDBLCLK:
                //case User32.WM.MBUTTONDBLCLK:
                //case User32.WM.XBUTTONDBLCLK:
                //case User32.WM.NCLBUTTONDBLCLK:
                //case User32.WM.NCRBUTTONDBLCLK:
                //case User32.WM.NCMBUTTONDBLCLK:
                //case User32.WM.NCXBUTTONDBLCLK:
                //    a = MouseActions.DoubleClick; break;
                case User32.WM.LBUTTONDOWN:
                case User32.WM.RBUTTONDOWN:
                case User32.WM.MBUTTONDOWN:
                case User32.WM.XBUTTONDOWN:
                //case User32.WM.NCLBUTTONDOWN:
                //case User32.WM.NCRBUTTONDOWN:
                //case User32.WM.NCMBUTTONDOWN:
                //case User32.WM.NCXBUTTONDOWN:
                default:
                    a = MouseActionType.Press; break;
                case User32.WM.LBUTTONUP:
                case User32.WM.RBUTTONUP:
                case User32.WM.MBUTTONUP:
                case User32.WM.XBUTTONUP:
                    //case User32.WM.NCLBUTTONUP:
                    //case User32.WM.NCRBUTTONUP:
                    //case User32.WM.NCMBUTTONUP:
                    //case User32.WM.NCXBUTTONUP:
                    a = MouseActionType.Release; break;
            }

            MouseSimulatorAction action = a == MouseActionType.Press ?
                new MousePressSimulatorAction(b) :
                (MouseSimulatorAction)new MouseReleaseSimulatorAction(b);
            action.PositionType = MousePositionType.Relative;
            action.Position = Point.Empty;

            return action;
        }

        #region Click tracker
        private MouseClickSimulatorAction lastClickAction;
        private ClickTrackerBehavior _clickTrackerBehavior;
        public ClickTrackerBehavior ClickTrackerBehavior { get { return _clickTrackerBehavior; } set { _clickTrackerBehavior = value; } }

        private int _multiClicksCountToTrack, consecutiveClicks;
        public int MultiClicksCountToTrack { get { return _multiClicksCountToTrack; } set { _multiClicksCountToTrack = value; } }

        private void raiseEventsForMultiClick(MouseClickSimulatorAction currentClick)
        {
            if (consecutiveClicks == 2)
            {
                MouseDoubleClickSimulatorAction dblClick = currentClick.GetMouseDoubleClickSimulatorActionWithTheSameProperties();
                OnMouseDoubleClickTracked(dblClick);
                OnMouseActionTracked(dblClick);
            }
            else
            {
                MouseMultiClickSimulatorAction multiClick = currentClick.GetMouseMultiClickSimulatorActionWithTheSameProperties(consecutiveClicks);
                OnMouseMultiClickTracked(multiClick);
                OnMouseActionTracked(multiClick);
            }

            lastClickAction = null;
            consecutiveClicks = 1;
        }

        private void raiseEventsForDoubleClick(MouseClickSimulatorAction currentClick)
        {
            MouseDoubleClickSimulatorAction dblClick = currentClick.GetMouseDoubleClickSimulatorActionWithTheSameProperties();
            OnMouseDoubleClickTracked(dblClick);
            OnMouseActionTracked(dblClick);
            lastClickAction = null;
        }

        private bool checkAndRaiseEventForClicks(MouseReleaseSimulatorAction currentRelease)
        {
            //if press and release are for the same window then raise a click event too
            if (lastMousePressAction != null) //this is set at the OnMousePressTracked function
                if (lastMousePressAction.hWnd == currentRelease.hWnd && currentRelease.hWnd != IntPtr.Zero)
                {
                    MouseClickSimulatorAction click = currentRelease.GetMouseClickSimulatorActionWithTheSameProperties();
                    OnMouseClickTracked(click);
                    OnMouseActionTracked(click);

                    if (_clickTrackerBehavior == ClickTrackerBehavior.ClicksAndDoubleClicks ||
                        _clickTrackerBehavior == ClickTrackerBehavior.ClicksAndMultiClicks)
                    {
                        bool raisedEvent = checkAndRaiseEventForDoubleOrMoreClicks(click);

                        //if a double click occurs then set lastClickAction to null to avoid triple click tracking
                        //so that 4 clicks correspond to 2 double clicks

                        if (!raisedEvent) lastClickAction = click;
                    }
                    return true;
                }
            return false;
        }

        private bool checkAndRaiseEventForDoubleOrMoreClicks(MouseClickSimulatorAction currentClick)
        {
            if (lastClickAction != null && lastClickAction.IsCloseEnoughAndCapturesSameWindowForDoubleClick(currentClick))
            {
                if (_clickTrackerBehavior == ClickTrackerBehavior.ClicksAndDoubleClicks)
                {
                    raiseEventsForDoubleClick(currentClick);
                    return true;
                }
                else //ClicksAndMultiClicks
                {
                    consecutiveClicks++;
                    if (consecutiveClicks == _multiClicksCountToTrack)
                    {
                        raiseEventsForMultiClick(currentClick);
                        return true;
                    }
                }

            }
            else if (_clickTrackerBehavior == ClickTrackerBehavior.ClicksAndMultiClicks)
                consecutiveClicks = 1; //reset consecutiveClicks

            return false;
        }

        #endregion

        #endregion

        #endregion


        public void Save(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("<?xml version=\"1.0\"?>\r\n<actions>");
                foreach (MouseSimulatorAction a in _recordedMouseActions)
                    if (!(a is MouseCombinationSimulatorAction))
                        writer.WriteLine(a.ToString());
                writer.WriteLine("</actions>");
            }
        }

        public void Load(string path, bool appendToExistingActions = false)
        {
            if (_recordedMouseActions == null) _recordedMouseActions = new List<MouseSimulatorAction>();

            if (!appendToExistingActions) _recordedMouseActions.Clear();

            XmlDocument doc = new XmlDocument();doc.Load(path);

            XmlElement a = doc.DocumentElement;
            XmlNodeList actions = a.GetElementsByTagName("mouse");
            foreach (XmlElement mouseAction in actions)
                _recordedMouseActions.Add(MouseSimulatorAction.Parse(mouseAction));

            #region old simple text reader
            //using (StreamReader reader = new StreamReader(path))
            //    while (!reader.EndOfStream)
            //        _recordedMouseActions.Add(MouseSimulatorAction.Parse(reader.ReadLine()));
            #endregion
        }

        //should have a pause?
        private BackgroundWorker player;

        public void Play()
        {
            player = new BackgroundWorker();
            player.DoWork += new DoWorkEventHandler(player_DoWork);
            player.RunWorkerCompleted += new RunWorkerCompletedEventHandler(player_RunWorkerCompleted);

            player.RunWorkerAsync();

        }

        public event EventHandler PlayFinished;
        protected void OnPlayFinished()
        {
            EventHandler handler = PlayFinished;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        void player_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnPlayFinished();
        }

        void player_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < _recordedMouseActions.Count; i++)
            {

                MouseSimulatorAction a = _recordedMouseActions[i];
                if (a is MouseCombinationSimulatorAction) continue;

                if (a.DelayBefore > 0) Thread.Sleep(a.DelayBefore);
                a.Send();
                if (a.DelayAfter > 0) Thread.Sleep(a.DelayAfter);
            }

        }

        ~MouseRecorder()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (ptrMouseHook != IntPtr.Zero) StopRecording();
        }

    }
}
