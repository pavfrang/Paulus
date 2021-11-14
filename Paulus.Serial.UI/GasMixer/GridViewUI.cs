using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Base;
using Paulus.UI;
using Paulus.Serial.GasMixer;

namespace Paulus.Serial.UI.GasMixer
{
    public abstract class GridViewUI : DeviceUIBase<GasMixerCommander, GasMixerSettings>
    {
        public GridViewUI(GridView gridView, DeviceUIBase<GasMixerCommander, GasMixerSettings> parent)
            : base(parent)
        {
            this.gridView = gridView;
            initialize();
        }

        public GridView gridView;
        public GridView GridView { get { return gridView; } }

        public event EventHandler GridViewChanged;
        protected void OnGridViewChanged() =>
            GridViewChanged?.Invoke(this, EventArgs.Empty);


        #region Initialization
        protected virtual void initialize()
        {
            gridView.InitializeStaticGridView();

            //add control handlers
            gridView.ValidatingEditor += GridView_ValidatingEditor;
            gridView.CustomDrawCell += GridView_CustomDrawCell;
            gridView.CellValueChanged += GridView_CellValueChanged;
        }

        #region Properties for initialization
        /// <summary>
        /// Repository assignments to columns.
        /// </summary>
        public Dictionary<string, RepositoryItem> ColumnRepositories { get; } = new Dictionary<string, RepositoryItem>();

        /// <summary>
        /// Numeric formats for columns.
        /// </summary>
        public Dictionary<string, string> ColumnNumericFormats { get; } = new Dictionary<string, string>();

        public Dictionary<string, string> HeaderWrapTexts { get; } = new Dictionary<string, string>();

        /// <summary>
        /// One of EnableEditColumns, DisableEditColumns must be defined.
        /// </summary>
        public List<string> EnableEditColumns { get; } = new List<string>();
        public List<string> DisableEditColumns { get; } = new List<string>();

        protected int cellsMargin = 10;
        /// <summary>
        /// Margin must be set before calling updateColumnsAfterLoading.
        /// </summary>
        public int CellsMargin
        {
            get { return cellsMargin; }
            set
            {
                if (cellsMargin == value) return;

                if (cellsMargin < 0)
                    throw new ArgumentOutOfRangeException(nameof(CellsMargin), "Value cannot be less than zero.");

                cellsMargin = value;

                if (gridView.Columns.Any())
                    gridView.BestFitColumnsWithMargin(value, true);
            }
        }
        #endregion

        #endregion

        #region Data loading

        protected HashSet<int> unsavedRows = new HashSet<int>();

        //this must be called when we fast connect to the device
        public void ClearUnsavedRows()
        {
            unsavedRows.Clear();
            //the only way to invalidate the control!
            gridView.LayoutChanged();
        }

        public void ResetUnsavedRows()
        {
            //track the unsaved rows
            for (int iRow = 0; iRow < dataTable.Rows.Count; iRow++)
                unsavedRows.Add(gridView.GetRowHandle(iRow));
            gridView.LayoutChanged();

        }


        protected DataTable dataTable;
        public DataTable DataTable
        {
            get { return dataTable; }

            set
            {
                if (dataTable == value) return;

                this.dataTable = value;

                //load table to grid
                gridView.LoadTableAndPrepareColumns(dataTable);

                //track the unsaved rows
                ResetUnsavedRows();

                //update column properties 
                UpdateColumnsAfterLoading();
            }

        }

        protected void UpdateColumnsAfterLoading()
        {
            var columns = gridView.Columns;

            if (EnableEditColumns.Any())
                gridView.EnableEditForSpecificColumns(EnableEditColumns.ToArray());
            else if (DisableEditColumns.Any())
                gridView.DisableEditForSpecificColumns(DisableEditColumns.ToArray());

            //set formats for numeric values
            foreach (var entry in ColumnNumericFormats)
            {
                string columnName = entry.Key;
                string format = entry.Value;
                columns[columnName].SetNumericFormat(entry.Value);
            }

            //set the repositories
            foreach (var entry in ColumnRepositories)
            {
                string columnName = entry.Key;
                RepositoryItem repository = entry.Value;
                columns[columnName].ColumnEdit = entry.Value;
            }

            //the function allows the correct tracking of Key Up/Down keys
            if (ColumnRepositories.Values.Any(r => r is RepositoryItemComboBox))
                gridView.AddRepositoryKeyDownEventForComboboxEdits();

            //set the header wrap texts
            foreach (var entry in HeaderWrapTexts)
            {
                string columnName = entry.Key;
                string wrapText = entry.Value;
                columns[columnName].SetHeaderWrapText(wrapText);
            }

            gridView.BestFitColumnsWithMargin(CellsMargin, true);
        }

        #endregion

        protected virtual void GridView_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e) { }

        protected abstract void GridView_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e);

        protected virtual void GridView_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (unsavedRows.Contains(e.RowHandle))
                e.Appearance.BackColor = Color.LightYellow;
        }

    }
}
