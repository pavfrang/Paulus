using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using Microsoft.Office.Interop.Excel;
using Paulus.Win32;

namespace Paulus.Excel
{
    /// <summary>
    /// The structure is used in the <see cref="ExcelExtensions.OpenExcel(out ExcelInfo,ChangeSettingsMode,bool)"/> and <see cref="ExcelExtensions.QuitOrRestoreSettings(Application,ref ExcelInfo)"/> static functions to store basic settings of the application that will change for optimization.
    /// </summary>
    public struct ExcelInfo
    {
        /// <summary>
        /// The value is set to true when a new instance has been created, by the OpenExcel function.
        /// </summary>
        public bool IsNewInstance;
        /// <summary>
        /// The DisplayAlerts value of the Excel instance before it changes for optimizing reasons.
        /// </summary>
        public bool DisplayAlertsOld;
        /// <summary>
        /// The ScreenUpdating value of the Excel instance before it changes for optimizing reasons.
        /// </summary>
        public bool ScreenUpdatingOld;
        /// <summary>
        /// The Visible value of the Excel instance before it changes for optimizing reasons.
        /// </summary>
        public bool VisibleOld;
        /// <summary>
        /// If true then all information inside the structure is not valid. This should be set to true if the application has exited.
        /// </summary>
        public bool IsInvalid;
        ///// <summary>
        ///// The value is set to true when the settings of the instance have been changed. This is set to true if the optimizeForSpeed is true at the OpenExcel function.
        ///// </summary>
        //public bool SettingsHaveChanged;
        /// <summary>
        /// The change settings mode. See <see cref="ChangeSettingsMode"/>.
        /// </summary>
        public ChangeSettingsMode ChangeSettingsMode;

        //public ExcelInfo(bool isNewInstance)
        //{
        //    IsNewInstance = isNewInstance;
        //    SettingsHaveChanged = DisplayAlertsOld = ScreenUpdatingOld = VisibleOld = IsInvalid = false;
        //}
    }

    /// <summary>
    /// This mode is used when opening an application, like Excel and defines whether or not settings should be changed based on needs for speed, debugging..
    /// </summary>
    public enum ChangeSettingsMode
    {
        /// <summary>
        /// Do not change the settings of the application after it is open.
        /// </summary>
        DontChangeSettings,
        /// <summary>
        /// Change settings of the application to ease debugging.
        /// </summary>
        ChangeSettingsForDebug,
        /// <summary>
        /// Change settings of the application to optimize for speed.
        /// </summary>
        ChangeSettingsForSpeed,
        /// <summary>
        /// Change settings of the application to optimize for speed except for the Visible property.
        /// </summary>
        ChangeSettingsForSpeedExceptForVisible
    }

    /// <summary>
    /// Contains the basic Excel contraints for all Excel editions.
    /// </summary>
    public enum ExcelConstraints
    {
        /// <summary>
        /// The maximum row in Office 2000,XP,2003.
        /// </summary>
        MaxRow2003 = 65536,
        /// <summary>
        /// The maximum row in Office 2007,2010.
        /// </summary>
        MaxRow2007 = 1048576,
        /// <summary>
        /// The maximum column in Office 2000,XP,2003.
        /// </summary>
        MaxColumn2003 = 256,
        /// <summary>
        /// The maximum column in Office 2007,2010.
        /// </summary>
        MaxColumn2007 = 16384,
        /// <summary>
        /// The maximum number points per data series in a 2D chart in Office 2000,XP,2003.
        /// </summary>
        MaxPointsInDataSeries2DChart2003 = 32000,
        /// <summary>
        /// The maximum number points per data series in a 3D chart in Office 2000,XP,2003.
        /// </summary>
        MaxPointsInDataSeries3DChart2003 = 4000,
        /// <summary>
        /// The maximum number points in a chart in Office 2000,XP,2003.
        /// </summary>
        MaxPointsInAllDataSeries2003 = 256000,
        /// <summary>
        /// The maximum header length.
        /// </summary>
        ColumnWidth = 255
    }

    /// <summary>
    /// Contains extension functions to ease the use of 
    /// </summary>
    public static class ExcelExtensions
    {
        #region Excel application

        /// <summary>
        /// Opens an existing or a new Excel application window. Optionally changes the application settings to optimize for speed and stores the application settings that are used for optimization to an info structure.
        /// </summary>
        /// <param name="info">Contains the settings of the Excel application before the speed optimization. It is used if optimizeForSpeed is true and an existing instance of the application windows is used.</param>
        /// <param name="changeSettingsMode">Sets whether settings of the application should be changed. See <see cref="ChangeSettingsMode"/> .</param>
        /// <param name="getExistingInstance">If set to true, it attempts to get the first existing instance of the <see cref="Application"/>. If it is false and an existing instance is not found, then a new instance is created too.</param>
        /// <returns>The Excel Application object or null.</returns>
        /// <example>
        ///ExcelInfo info;
        ///Application app = ExcelExt.OpenExcel(out info);
        /// </example>
        /// <seealso cref="ExcelExtensions.QuitOrRestoreSettings(Application,ref ExcelInfo)"/>
        public static Application OpenExcel(out ExcelInfo info,
            ChangeSettingsMode changeSettingsMode = ChangeSettingsMode.ChangeSettingsForSpeed, bool getExistingInstance = true)
        {
            info = new ExcelInfo();

            Application xlApp = null;

            try
            {
                if (getExistingInstance)
                {
                    info.IsNewInstance = false;
                    xlApp = (Application)global::System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
                }
                else
                {
                    info.IsNewInstance = true;
                    xlApp = new Application();
                }
            }
            catch (global::System.Runtime.InteropServices.COMException) //COMException (object was not found)
            {
                info.IsNewInstance = true;
                xlApp = new Application();
            }
            finally
            {
                xlApp.ChangeSettings(ref info, changeSettingsMode);
            }

            return xlApp;
        }

        public static Application OpenExcel(
            string workbookPathToCheckItsParentApplication,
            out ExcelInfo info, out Workbook wb,
            ChangeSettingsMode changeSettingsMode = ChangeSettingsMode.DontChangeSettings,
            bool getExistingInstanceIfFoundAndWorkbookIsNotOpen = true)
        {
            if (File.Exists(workbookPathToCheckItsParentApplication))
            {
                Application app = GetExcelInstanceThatContainsWorkbook(workbookPathToCheckItsParentApplication);
                if (app != null)
                {
                    info = new ExcelInfo(); info.IsNewInstance = false;
                    app.ChangeSettings(ref info, changeSettingsMode);
                    app.Workbooks.TryGetWorkbookByFullPath(workbookPathToCheckItsParentApplication, out wb);
                    return app;
                }
                Application newInstance = OpenExcel(out info, changeSettingsMode, getExistingInstanceIfFoundAndWorkbookIsNotOpen);
                wb = newInstance.Workbooks.Open(workbookPathToCheckItsParentApplication);
                return newInstance;
            }
            else //it's better not to be very generic and do a specific job (but good!)
            {
                throw new FileNotFoundException("The file cannot be found.",workbookPathToCheckItsParentApplication);
               
                //Application newInstance = OpenExcel(out info, changeSettingsMode, getExistingInstanceIfFoundAndWorkbookIsNotOpen);
                //wb = null; //file does not exist
                //return newInstance;
            }
        }

        public static void CreateCopyAndOpenExcelAndWorkbook(string filePath, string copyPath,
   out Application app, out ExcelInfo info, out Workbook wb, ChangeSettingsMode changeSettingsMode = ChangeSettingsMode.ChangeSettingsForSpeed, bool getExistingInstanceIfFoundAndWorkbookIsNotOpen = true)
        {
            app = ExcelExtensions.OpenExcel(copyPath, out info, out wb, changeSettingsMode, getExistingInstanceIfFoundAndWorkbookIsNotOpen);

            if (wb == null) //check if the workbook is open
            {
                File.Copy(filePath, copyPath, true);
                wb = app.Workbooks.Open(copyPath);
            }
        }

        /// <summary>
        /// Changes the settings to the current Excel Application. Note that the info.IsNewInstance remains the same.
        /// </summary>
        /// <param name="xlApp"></param>
        /// <param name="changeSettingsMode"></param>
        /// <param name="info"></param>
        public static void ChangeSettings(this Application xlApp, ref ExcelInfo info, ChangeSettingsMode changeSettingsMode = ChangeSettingsMode.ChangeSettingsForSpeedExceptForVisible)
        {
            //store old settings
            info.ChangeSettingsMode = changeSettingsMode;
            info.ScreenUpdatingOld = xlApp.ScreenUpdating;
            info.DisplayAlertsOld = xlApp.DisplayAlerts;
            info.VisibleOld = xlApp.Visible;

            switch (info.ChangeSettingsMode)
            {
                case ChangeSettingsMode.ChangeSettingsForSpeedExceptForVisible:
                    xlApp.ScreenUpdating = xlApp.DisplayAlerts = false; break;
                case ChangeSettingsMode.ChangeSettingsForSpeed:
                    xlApp.ScreenUpdating = xlApp.DisplayAlerts = xlApp.Visible = false; break;
                case ChangeSettingsMode.ChangeSettingsForDebug:
                    xlApp.ScreenUpdating = xlApp.DisplayAlerts = xlApp.Visible = true; break;
            }
        }

        /// <summary>
        /// Quits the Excel application if the instance is created from inside the current application, or restores the Excel settings to its previous values. Note that the Quit method works only if the instance is created inside the current application.
        /// The info structure is set by <see cref="ExcelExtensions.OpenExcel(out ExcelInfo,ChangeSettingsMode,bool)"/>. 
        /// </summary>
        /// <param name="xlApp">The Excel instance that is to be restored or to quit.</param>
        /// <param name="info">The ExcelInfo structure that contains information on how the Excel instance has been created and initialized.</param>
        /// <example>
        /// app.QuitOrRestoreSettings(ref info);
        /// </example>
        /// <seealso cref="RestoreSettings(Application,ref ExcelInfo)"/>
        public static void QuitOrRestoreSettings(this Application xlApp, ref ExcelInfo info)
        {
            if (!info.IsNewInstance)
                xlApp.RestoreSettings(ref info);
            else //quit is not allowed if the instance is not new
                xlApp.Quit();

            info.IsInvalid = true;
        }

        /// <summary>
        /// Restores the settings of the Excel application based on the info. The info structure is set by <see cref="ExcelExtensions.OpenExcel(out ExcelInfo,ChangeSettingsMode,bool)"/>.
        /// The info.SettingsHaveChanged is NOT taken into account if the ignoreSettingsHaveChangedFlag is true.
        /// </summary>
        /// <param name="xlApp">The Excel instance that is to be restored.</param>
        /// <param name="info">The ExcelInfo structure that contains information on how the Excel instance has been created and initialized.</param>
        /// <param name="ignoreSettingsHaveChangedFlag">If true then the SettingsHaveChanged flag is ignored and old values are always restored.</param>
        /// <example>
        /// xlApp.RestoreSettings(ref info);
        /// </example>
        public static void RestoreSettings(this Application xlApp, ref ExcelInfo info, bool ignoreSettingsHaveChangedFlag = false)
        {
            if (info.ChangeSettingsMode != ChangeSettingsMode.DontChangeSettings || ignoreSettingsHaveChangedFlag)
            {
                xlApp.ScreenUpdating = info.ScreenUpdatingOld;
                xlApp.DisplayAlerts = info.DisplayAlertsOld;
                xlApp.Visible = info.VisibleOld;
                info.ChangeSettingsMode = ChangeSettingsMode.DontChangeSettings;//reset flags
            }
        }
        #endregion

        #region Get Excel instances
        [DllImport("Oleacc.dll")]
        private static extern int AccessibleObjectFromWindow(IntPtr hWnd, uint dwObjectID, byte[] riid,
            ref Microsoft.Office.Interop.Excel.Window ptr);

        /// <summary>
        /// Kills all Excel processes that contain no workbooks.
        /// </summary>
        public static void KillEmptyExcelInstances()
        {
            List<Process> psToKill = new List<Process>();

            List<WindowInfo> xlMains = WindowInfo.FindWindows("XLMAIN");

            foreach (WindowInfo xlMain in xlMains)
                if (xlMain.FindSpecificChildWindows("XLDESK").Count == 0)
                    psToKill.Add(Process.GetProcessById(xlMain.ProcessHandle.ToInt32()));

            foreach (Process p in psToKill) p.Kill();


            //Process[] psToKill = Process.GetProcessesByName("EXCEL");
            //foreach (Process p in ps)
            //    if (p.MainWindowHandle.ToInt32() == 0) p.Kill();
        }

        public static void KillAllExcelInstances()
        {
            //#1 way
            //Process[] psToKill = Process.GetProcessesByName("EXCEL");

            List<Process> psToKill = new List<Process>();
            List<WindowInfo> xlMains = WindowInfo.FindWindows("XLMAIN");
            foreach (WindowInfo xlMain in xlMains) psToKill.Add(Process.GetProcessById(xlMain.ProcessHandle.ToInt32()));

            foreach (Process p in psToKill) p.Kill();
        }

        public static void KillAllExcelInstancesExceptFor(string workbookPath)
        {
            KillEmptyExcelInstances();

            List<Application> excelInstances = GetNonEmptyExcelInstances();
            foreach (Application excelInstance in excelInstances)
                if (!excelInstance.Workbooks.ContainsByFullPath(workbookPath)) excelInstance.Quit(); //check it
        }

        private static Application GetExcelInstanceFromWorkbookInfo(WindowInfo workbookWindowInfo)
        {
            const uint OBJID_NATIVEOM = 0xFFFFFFF0; //constant to return the object model pointer
            Guid IID_IDispatch = new Guid("{00020400-0000-0000-C000-000000000046}");
            Window ptr = null;
            int hr = AccessibleObjectFromWindow(workbookWindowInfo.Handle, OBJID_NATIVEOM, IID_IDispatch.ToByteArray(), ref ptr);
            return hr >= 0 ? ptr.Application : null;
        }

        private static List<Application> GetExcelInstancesFromWorkbookHandles(List<WindowInfo> workbookWindowInfos)
        {
            List<Application> excelInstances = new List<Application>();

            foreach (WindowInfo workbookInfo in workbookWindowInfos)
            {
                Application app = GetExcelInstanceFromWorkbookInfo(workbookInfo);
                if (app != null && !excelInstances.Contains(app)) excelInstances.Add(app);
            }
            return excelInstances;
        }

        //retrieves Excel instances that have at least one workbook
        public static List<Application> GetNonEmptyExcelInstances()
        {
            List<WindowInfo> workbookWindowInfos = WindowInfo.FindWindows(false, SearchType.ByClassName, "XLMAIN", "XLDESK", "EXCEL7");

            return GetExcelInstancesFromWorkbookHandles(workbookWindowInfos);
        }

        public static Application GetExcelInstanceThatContainsWorkbook(string fullPath)
        {
            List<Application> excelInstances = GetNonEmptyExcelInstances();
            foreach (Application excelInstance in excelInstances)
                if (excelInstance.Workbooks.ContainsByFullPath(fullPath)) return excelInstance;
            return null;
        }

        public static T GetOneValue<T>(this Application excel, string workbookPath, string worksheet, string address)
        {
            Workbook wb = excel.Workbooks.Open(workbookPath);
            Worksheet sh = wb.Worksheets[worksheet];
            T value= sh.Range[address].Value ?? default(T);
            wb.Close(false);
            return value;
        }
        #endregion

        #region Workbooks

        /// <summary>
        /// Returns true if the workbook exists.
        /// </summary>
        /// <param name="workbooks">The workbooks collection.</param>
        /// <param name="workbookName">The name of the workbook to be checked for existence.</param>
        /// <param name="ignoreCase">If true then the case is ignored.</param>
        /// <returns>true if the workbook exists.</returns>
        public static bool Contains(this Workbooks workbooks, string workbookName, bool ignoreCase = true)
        {
            foreach (Workbook workbook in workbooks)
            {
                bool found = ignoreCase ? workbook.Name.ToLower() == workbookName.ToLower() : workbook.Name == workbookName;
                if (found)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the workbook exists. Also returns the workbook if found.
        /// </summary>
        /// <param name="workbooks">The workbooks collection.</param>
        /// <param name="workbookName">The name of the workbook to be checked for existence.</param>
        /// <param name="workbook">The workbook to be returned.</param>
        /// <param name="ignoreCase">If true then the case is ignored.</param>
        /// <returns>true if the workbook exists.</returns>
        public static bool TryGetWorkbook(this Workbooks workbooks, string workbookName, out Workbook workbook, bool ignoreCase = true)
        {
            foreach (Workbook wb in workbooks)
            {
                bool found = ignoreCase ? wb.Name.ToLower() == workbookName.ToLower() : wb.Name == workbookName;
                if (found)
                {
                    workbook = wb;
                    return true;
                }
            }
            workbook = null;
            return false;
        }

        /// <summary>
        /// Returns true if the workbook exists by specifying the full path of the workbook.
        /// </summary>
        /// <param name="workbooks">The workbooks collection.</param>
        /// <param name="fullPath">The full path of the workbook to be checked for existence.</param>
        /// <returns>true if the workbook exists.</returns>
        public static bool ContainsByFullPath(this Workbooks workbooks, string fullPath)
        {
            foreach (Workbook wb in workbooks)
                if (wb.FullName.ToLower() == fullPath.ToLower())
                    return true; //Book1 etc. is returned for unsaved workbooks
            return false;
        }

        /// <summary>
        /// Returns true if the workbook exists by specifying the full path of the workbook. Also returns the workbook if found.
        /// </summary>
        /// <param name="workbooks">The workbooks collection.</param>
        /// <param name="fullPath">The full path of the workbook to be checked for existence.</param>
        /// <param name="workbook">The workbook to be returned.</param>
        /// <returns>true if the workbook exists.</returns>
        public static bool TryGetWorkbookByFullPath(this Workbooks workbooks, string fullPath, out  Workbook workbook)
        {
            foreach (Workbook wb in workbooks)
                if (wb.FullName.ToLower() == fullPath.ToLower())
                {
                    workbook = wb;
                    return true; //Book1 etc. is returned for unsaved workbooks
                }
            workbook = null;
            return false;
        }

        /// <summary>
        /// Opens a workbook if it exists or creates a new workbook if it does not exist.
        /// </summary>
        /// <param name="workbooks">The workbooks collection.</param>
        /// <param name="fullPath">The full path of the workbook to be checked for existence.</param>
        /// <returns>The new or existent workbook depending on if the workbook already exists.</returns>
        public static Workbook OpenOrCreateNew(this Workbooks workbooks, string fullPath)
        {
            if (File.Exists(fullPath))
                return workbooks.Open(fullPath);
            else
            {
                Workbook newWb = workbooks.Add();
                newWb.SaveAs(fullPath);
                return newWb;
            }
        }

        /// <summary>
        /// Returns a generic list of the workbooks.
        /// </summary>
        /// <param name="workbooks">The workbooks collection.</param>
        /// <returns>A generic list of the workbooks.</returns>
        public static List<Workbook> ToList(this Workbooks workbooks)
        {
            List<Workbook> list = new List<Workbook>();
            foreach (Workbook wb in workbooks)
                list.Add(wb);
            return list;
        }

        /// <summary>
        /// Returns an array of the workbooks.
        /// </summary>
        /// <param name="workbooks">The workbooks collection.</param>
        /// <returns>An array of the workbooks.</returns>
        public static Workbook[] ToArray(this Workbooks workbooks)
        {
            return workbooks.ToList().ToArray();
        }

        public static void RemoveAllVBACode(this Workbook wb)
        {
            Microsoft.Vbe.Interop.VBProject proj = wb.VBProject;
            foreach (Microsoft.Vbe.Interop.VBComponent comp in proj.VBComponents)
            {
                if (comp.Type == Microsoft.Vbe.Interop.vbext_ComponentType.vbext_ct_Document)
                {
                    Microsoft.Vbe.Interop.CodeModule module = comp.CodeModule;
                    module.DeleteLines(1, module.CountOfLines);
                }
                else
                    proj.VBComponents.Remove(comp);
            }
        }

        //useful to clear selections
        public static void ResetSelection(this Workbook wb)
        {
            if (wb.Worksheets.Count > 0)
            {
                Worksheet sheet=wb.Worksheets[1]; //the first worksheet has the index 1
                sheet.Select();
                sheet.Range["a1"].Select();
            }
        }
        #endregion

    }
}
