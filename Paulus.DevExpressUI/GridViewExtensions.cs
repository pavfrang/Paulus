using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Paulus.UI
{
    public static class GridViewExtensions
    {
        public static void InitializeStaticGridView(this GridView gridView, bool allowSort = false, bool allowFilter = false)
        {
            gridView.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
            gridView.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
            gridView.OptionsCustomization.AllowColumnMoving = false;
            gridView.OptionsCustomization.AllowFilter = allowFilter;
            gridView.OptionsCustomization.AllowGroup = false;
            gridView.OptionsCustomization.AllowQuickHideColumns = false;
            gridView.OptionsCustomization.AllowSort = allowSort;
            gridView.OptionsFilter.AllowColumnMRUFilterList = false;
            gridView.OptionsFilter.AllowFilterEditor = false;
            gridView.OptionsFilter.AllowFilterIncrementalSearch = false;
            gridView.OptionsFilter.AllowMRUFilterList = false;
            gridView.OptionsFilter.AllowMultiSelectInCheckedFilterPopup = false;
            gridView.OptionsMenu.EnableFooterMenu = false;
            gridView.OptionsMenu.EnableGroupPanelMenu = false;
            gridView.OptionsView.ColumnAutoWidth = false;
            gridView.OptionsView.ShowGroupExpandCollapseButtons = false;
            gridView.OptionsView.ShowGroupPanel = false;

            gridView.OptionsMenu.EnableColumnMenu = false;

            //allow multiline headers
            gridView.OptionsView.AllowHtmlDrawHeaders = true;
            gridView.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
        }

        /// <summary>
        /// Clears existing columns, assigns the tablee and centers the column headers.
        /// </summary>
        /// <param name="gridView"></param>
        /// <param name="table"></param>
        public static void LoadTableAndPrepareColumns(this GridView gridView, DataTable table)
        {
            //the settings should be loaded
            gridView.Columns.Clear();
            gridView.GridControl.DataSource = table;
            gridView.PopulateColumns(table);
            //center all column headers and set their header to bold
            //gridView.CenterColumnHeaders();

        }

        public static void CenterColumnHeaders(this GridView gridView)
        {
            foreach (GridColumn column in gridView.Columns)
            {
                column.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                column.AppearanceHeader.Font = new Font(column.AppearanceHeader.Font, FontStyle.Bold);
            }
        }

        public static void BestFitColumnsWithMargin(this GridView gridView, int margin = 10, bool useDoubleMarginForComboboxEdits = false)
        {
            foreach (GridColumn column in gridView.Columns)
            {
                if (column.ColumnEdit != null && column.ColumnEdit is RepositoryItemComboBox && useDoubleMarginForComboboxEdits)
                    column.Width = column.GetBestWidth() + 2 * margin;
                else
                    column.Width = column.GetBestWidth() + margin;
            }
        }

        public static int GetMaximumWidth(string[] texts, Font font, int decimals)
        {
            return
                texts.Max(t => TextRenderer.MeasureText(t, font).Width);
        }


        private static HashSet<GridView> gridViewsWithKeyDownEventHandled = new HashSet<GridView>();
        /// <summary>
        /// The function will track the KeyDown event. If the Down or Up keys are pressed then the selected index of the combo is changed.
        /// The default behavior is to change the cell focus after pressing the Down or Up keys (which is overriden with the use of this method). 
        /// </summary>
        /// <param name="gridView"></param>
        /// <param name="comboBox"></param>
        public static void AddRepositoryKeyDownEventForComboboxEdits(this GridView gridView)
        {
            //we add the handler only if it is not already added
            if (!gridViewsWithKeyDownEventHandled.Contains(gridView))
            {
                gridView.KeyDown += GridViewPorts_KeyDown;

                gridViewsWithKeyDownEventHandled.Add(gridView);
            }

            ////this allows the assigment of
            //repositoryGridViewAssignments.Add(comboBox, gridView);
        }

        public static void RemoveRepositoryKeyDownEventForComboboxEdits(this GridView gridView)
        {
            gridView.KeyDown -= GridViewPorts_KeyDown;
            gridViewsWithKeyDownEventHandled.Remove(gridView);
        }

        private static void GridViewPorts_KeyDown(object sender, KeyEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.ColumnEdit is RepositoryItemComboBox && view.ActiveEditor != null)
            {
                ComboBoxEdit edit = view.ActiveEditor as ComboBoxEdit;
                if (e.KeyData == Keys.Down && edit.SelectedIndex < edit.Properties.Items.Count - 1)
                    edit.SelectedIndex++;
                else if (e.KeyData == Keys.Up && edit.SelectedIndex > 0)
                    edit.SelectedIndex--;

                e.Handled = e.KeyData == Keys.Down || e.KeyData == Keys.Up;
            }
            else if (e.KeyCode == Keys.Return)
            {
                //view.FocusedRowHandle++;
                //e.Handled = true;
            }
        }

        #region AllowEdit

        public static void DisableEditForSpecificColumns(this GridView gridView, params string[] columns)
        {
            foreach (string column in columns)
                gridView.Columns[column].OptionsColumn.AllowEdit = false;
        }
        public static void EnableEditForSpecificColumns(this GridView gridView, params string[] columns)
        {
            var disableColumnNames = gridView.Columns.Select(c => c.FieldName).Except(columns);
            if (!disableColumnNames.Any()) return;

            foreach (string column in disableColumnNames)
                gridView.Columns[column].OptionsColumn.AllowEdit = false;
        }
        #endregion


        #region Format and Wrap (Column extensions)

        public static void SetNumericFormat(this GridColumn column, string format)
        {
            column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            column.DisplayFormat.FormatString = format;
        }

        public static void SetHeaderWrapText(this GridColumn column, string text)
        {
            column.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            column.Caption = text;
        }

        #endregion


        public static GridColumn GetGridColumn(this BaseContainerValidateEditorEventArgs e, GridView gridView) =>
            (e as EditFormValidateEditorEventArgs)?.Column ?? gridView.FocusedColumn;


        private static Dictionary<ColumnView, int[]> viewSelections;
        public static void BackupSelection(this ColumnView view)

        {
            if (viewSelections == null) viewSelections = new Dictionary<ColumnView, int[]>();

            if (!viewSelections.ContainsKey(view))
                viewSelections.Add(view, view.GetSelectedRows());
            else
                viewSelections[view] = view.GetSelectedRows();
        }

        public static void RestoreSelection(this ColumnView view)
        {
            if (!viewSelections?.ContainsKey(view) ?? false) return;

            view.ClearSelection();

            view.BeginSelection();

            view.ClearSelection();
            foreach (int iRow in viewSelections[view])
                if (view.IsValidRowHandle(iRow))
                    view.SelectRow(iRow);
            view.EndSelection();
        }


        //private static Dictionary<ColumnView, Form> viewsWithEnabledClickAndSelectColumns;

        private static HashSet<ColumnView> viewsWithEnabledClickAndSelectColumns;
        public static void EnableClickAndSelectCell(this ColumnView view)
        {
            if (viewsWithEnabledClickAndSelectColumns == null)
                viewsWithEnabledClickAndSelectColumns = new HashSet<ColumnView>();

            if (viewsWithEnabledClickAndSelectColumns.Contains(view)) return;

            view.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseUp;

            viewsWithEnabledClickAndSelectColumns.Add(view);
            //view.ShownEditor += View_ShownEditor;
            view.MouseDown += View_MouseDown;
        }

        private static void View_ShownEditor(object sender, EventArgs e)
        {
            //ColumnView view = sender as GridView;
          
            //if (!(view.ActiveEditor is TextEdit)) return;

            //TextEdit te = view.ActiveEditor as TextEdit;
            //te.Focus();
            //te.SelectAll();
        }

        private static void View_MouseDown(object sender, MouseEventArgs e)
        {
            GridView gridView = sender as GridView;
            GridHitInfo hitInfo = gridView.CalcHitInfo(e.Location);

            if (hitInfo.Column == null) return;

            Type t = hitInfo.Column.ColumnType;
            if (t == typeof(bool) || hitInfo.Column.ColumnEdit != null)
            {
                gridView.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
                gridView.ShowEditor();
            }
            else //this allows selection when a simple textedit is the (default) columnedit
                gridView.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseUp;

            //try
            //{
            //    viewsWithEnabledClickAndSelectColumns[gridView]. //the form will invoke
            //        BeginInvoke(new MethodInvoker(delegate { gridView.ShowEditorByMouse(); }));
            //}
            //catch { }
        }
    }
}
