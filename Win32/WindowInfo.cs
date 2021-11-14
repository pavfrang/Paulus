using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Paulus.Win32
{
    public enum SearchType
    {
        ByClassName,
        ByCaption
    }

    public class WindowInfo
    {
        private WindowInfo() { }

        public static WindowInfo Create(IntPtr hWnd, WindowInfo parent)
        {
            if (hWnd == IntPtr.Zero) hWnd = User32.GetDesktopWindow();

            if (WindowInfoCache.ContainsKey(hWnd)) return WindowInfoCache[hWnd];

            WindowInfo newWindowInfo = new WindowInfo();
            newWindowInfo._handle= hWnd;


            if (parent != null)
                newWindowInfo._parent = parent;
            else
            {
                //try to retrieve the parent if not defined here
                IntPtr parenthWnd = User32.GetParent(hWnd);
                if (parenthWnd != IntPtr.Zero) newWindowInfo._parent = WindowInfo.Create(parenthWnd, null);
            }

            WindowInfoCache.Add(hWnd, newWindowInfo);

            return newWindowInfo;
        }

        static WindowInfo()
        {
            WindowInfoCache = new Dictionary<IntPtr, WindowInfo>();
        }

        public static Dictionary<IntPtr, WindowInfo> WindowInfoCache;
        public static void ClearWindowInfoCache() { WindowInfoCache.Clear(); }


        #region Get special windows
        public static WindowInfo GetDesktopWindowInfo()
        {
            IntPtr desktopHandle = User32.GetDesktopWindow();
            return WindowInfo.Create(desktopHandle, null);
        }

        /// <summary>The GetForegroundWindow function returns a handle to the foreground window.</summary>
        public static WindowInfo GetForegroundWindowInfo()
        {
            return WindowInfo.Create(User32.GetForegroundWindow(), GetDesktopWindowInfo());
        }

        public static WindowInfo GetWindowFromPoint(Point point)
        {
            IntPtr hWnd = User32.WindowFromPoint((POINT)point);
            return WindowInfo.Create(hWnd, null);
        }

        public static WindowInfo GetWindowAtCurrentCursorPosition()
        {
            POINT pt;
            User32.GetCursorPos(out pt);
            IntPtr hWnd = User32.WindowFromPoint(pt);
            return WindowInfo.Create(hWnd, null);
        }

        public static List<WindowInfo> FindWindows(string textToBeFound, bool isPartialSearch = false, SearchType searchType = SearchType.ByClassName)
        {
            return GetDesktopWindowInfo().FindSpecificChildWindows(textToBeFound, isPartialSearch, searchType);
        }

        public static List<WindowInfo> FindWindows(string textLevel1, string textLevel2, bool isPartialSearch = false, SearchType searchType = SearchType.ByClassName)
        {
            List<WindowInfo> windowsLevel1 = GetDesktopWindowInfo().FindSpecificChildWindows(textLevel1, isPartialSearch, searchType);
            List<WindowInfo> windowsLevel2 = new List<WindowInfo>();
            foreach (WindowInfo windowLevel1 in windowsLevel1)
                windowsLevel2.AddRange(windowLevel1.FindSpecificChildWindows(textLevel2, isPartialSearch, searchType));
            return windowsLevel2;
        }

        public static List<WindowInfo> FindWindows(bool isPartialSearch = false, SearchType searchType = SearchType.ByClassName, params string[] textPerLevel)
        {
            int levelsCount = textPerLevel.Length;
            if (levelsCount == 1)
                return FindWindows(textPerLevel[0], isPartialSearch, searchType);
            else
            {
                Dictionary<int, List<WindowInfo>> windowsPerLevel = new Dictionary<int, List<WindowInfo>>();
                windowsPerLevel.Add(0, FindWindows(textPerLevel[0], isPartialSearch, searchType));
                for (int i = 1; i < textPerLevel.Length; i++)
                {
                    windowsPerLevel.Add(i, new List<WindowInfo>());
                    foreach (WindowInfo w in windowsPerLevel[i - 1])
                        windowsPerLevel[i].AddRange(w.FindSpecificChildWindows(textPerLevel[i], isPartialSearch, searchType));
                }
                return windowsPerLevel[levelsCount - 1];
            }
        }

        #endregion

        #region Handle
        private IntPtr _handle;
        /// <summary>
        /// Returns the window handle. This is set at the constructor.
        /// </summary>
        public IntPtr Handle
        {
            get { return _handle; }
        }
        #endregion

        #region Parent


        private WindowInfo _parent;
        /// <summary>
        /// Returns the parent Window. This is set at the constructor. 
        /// </summary>
        public WindowInfo Parent
        {
            get { return _parent; }

        }
        #endregion

        #region ClassName
        [DllImport("User32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        private static string GetClassName(IntPtr hWnd)
        {
            StringBuilder windowClass = new StringBuilder(128);
            GetClassName(hWnd, windowClass, 128);
            return windowClass.ToString();
        }

        public string ClassName
        {
            get { return GetClassName(_handle); }
        }
        #endregion

        #region Caption/WindowText
 
        private static string GetWindowText(IntPtr hWnd)
        {
            StringBuilder text = new StringBuilder(128);
            User32.GetWindowText(hWnd, text, 128);

            //if (hWnd.ToInt32()==0x770EDC) System.Diagnostics.Debugger.Break();
            if (text.Length == 0)
            {
                //attempt by SendMessage
                int length = 65536;
                text = new StringBuilder(length);
                User32.SendMessage(hWnd, (uint)User32.WM.GETTEXT, length, text);
            }
            return text.ToString();
        }

        /// <summary>
        /// Returns the caption of the window. The caption is updated each time the UpdateProperties function is called.
        /// </summary>
        public string Text
        {
            get
            {
                return GetWindowText(_handle);
            }

            set
            {
                int len = value.Length;
                StringBuilder sb = new StringBuilder(value);
                IntPtr result = User32.SendMessage(_handle, (uint)User32.WM.SETTEXT, len, sb);
                //if (result.ToInt32() == 1) _text = value;
            }
        }
        #endregion

        #region Process handle

        /// <summary>
        /// Returns the process handle that owns the window. The process handle is updated each time the UpdateProperties function is called.
        /// </summary>
        public IntPtr ProcessHandle
        {
            get
            {
                IntPtr _processHandle = IntPtr.Zero;
                User32.GetWindowThreadProcessId(_handle, ref _processHandle);
                return _processHandle;
            }
        }
        #endregion


        private bool EnumChildWindowsCallback(IntPtr hWnd, ref IntPtr lParam)
        {
            WindowInfo window = Create(hWnd, this);

            #region Search criteria (better use FindWindowEx)
            bool found;
            switch (childrenSearchInfo.SearchType)
            {
                case SearchType.ByClassName:
                    found = childrenSearchInfo.IsPartialSearch ? window.ClassName.Contains(childrenSearchInfo.TextToBeFound) :
                        window.ClassName == childrenSearchInfo.TextToBeFound;
                    break;
                default:
                case SearchType.ByCaption:
                    found = childrenSearchInfo.IsPartialSearch ? window.Text.Contains(childrenSearchInfo.TextToBeFound) : window.Text == childrenSearchInfo.TextToBeFound;
                    break;
            }

            if (found)
            {
                _childWindowInfos.Add(window);
            }
            #endregion

            lParam = hWnd;
            return true;
        }

        private List<WindowInfo> _childWindowInfos;
        public List<WindowInfo> ChildWindowInfos
        {
            get { return _childWindowInfos; }
        }

        private SearchInfo childrenSearchInfo;

        public List<WindowInfo> FindSpecificChildWindows(string textToBeFound, bool isPartialSearch = false, SearchType searchType = SearchType.ByClassName)
        {
            _childWindowInfos = new List<WindowInfo>();

            #region Alternative full text search (works)
            //if (!isPartialSearch) //this is case insensitive full search
            //{
            //    IntPtr foundPtr = IntPtr.Zero;
            //    if (searchType == SearchType.ByClassName)
            //    {
            //        while (true)
            //        {
            //            foundPtr = FindWindowEx(_handle, foundPtr, textToBeFound, IntPtr.Zero);
            //            if (foundPtr == IntPtr.Zero) break;
            //            WindowInfo info = new WindowInfo(foundPtr, this);
            //            _childWindowInfos.Add(info);
            //            Console.WriteLine(info.ToString());
            //        }
            //    }
            //    else if (searchType == SearchType.ByCaption)
            //    {
            //        while (true)
            //        {
            //            foundPtr = FindWindowEx(_handle, foundPtr, IntPtr.Zero, textToBeFound);
            //            if (foundPtr == IntPtr.Zero) break;
            //            WindowInfo info = new WindowInfo(foundPtr, this);
            //            _childWindowInfos.Add(info);
            //            Console.WriteLine(info.ToString());
            //        }

            //    }
            //}

            //else
            //{
            #endregion

            childrenSearchInfo = new SearchInfo();
            childrenSearchInfo.TextToBeFound = textToBeFound;
            childrenSearchInfo.IsPartialSearch = isPartialSearch;
            childrenSearchInfo.SearchType = searchType;

            IntPtr lParam = IntPtr.Zero;
            User32.EnumChildWindows(_handle, EnumChildWindowsCallback, ref lParam);
            //}

            return _childWindowInfos;
        }

        public override string ToString()
        {
            return string.Format("Handle: {0:X8}, Classname: \"{1}\", Text: \"{2}\"", _handle.ToInt32(), ClassName, Text);
        }

        private struct SearchInfo
        {
            public string TextToBeFound;
            public bool IsPartialSearch;
            public SearchType SearchType;
        }

        #region EnumWindows

        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, ref int lParam);

        //private static bool EnumWindowsCallback(int hWnd, ref int lParam)
        //{
        //    WindowInfo window = new WindowInfo(hWnd, null);
        //    lParam = hWnd;
        //    return true;
        //}

        //private static SearchInfo searchInfo;

        //public static List<WindowInfo> FindWindows(string textToBeFound, bool isPartialSearch = false, SearchType searchType = SearchType.ByClassName)
        //{
        //    searchInfo = new SearchInfo();
        //    searchInfo.WindowInfos = new List<WindowInfo>();
        //    searchInfo.TextToBeFound = textToBeFound;
        //    searchInfo.IsPartialSearch = isPartialSearch;
        //    searchInfo.SearchType = searchType;

        //    int lParam = 0;
        //    EnumWindows(EnumWindowsCallback, ref lParam);

        //    return searchInfo.WindowInfos;
        //}
        #endregion


    }


}
