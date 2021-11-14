using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paulus.UI
{
    /// <summary>
    /// The enumeration corresponds to the indices of the InformationIconsImageList.
    /// </summary>
    public enum InformationState
    {
        Error, Information, Warning
    }

    /// <summary>
    /// Contains a wrapper of a Status GridColumn (of type InformationState). Value 0 represents Error, 1 represents Information and 2 represents Warning state.
    /// </summary>
    /// <example>
    /// messages = new DataTable();
    /// messages.Columns.Add("Status", typeof(InformationState));
    /// ...
    /// DataRow newRow = messages.NewRow();
    /// newRow["Status"] = InformationState.Error;
    /// ...
    /// messages.Rows.Add(newRow);
    /// ...
    /// gridControlMessages.DataSource = messages;
    /// ...
    /// gridViewMessages.Columns["Status"].SetStatusGridColumn();
    /// </example>
    public static class StatusGridColumn
    {
        /// <summary>
        /// Returns an image list with the error (0), info (1) and warning (2) icons with size 12x12.
        /// </summary>
        /// <param name="container">Form.components is commonly the container of the control</param>
        /// <returns></returns>
        public static ImageList GetInformationIconsImageList(IContainer container = null)
        {
            ImageList control = container == null ? new ImageList() : new ImageList(container);

            control.ImageSize = new System.Drawing.Size(12, 12);

            control.Images.Add(ResourceUI.error_small);
            control.Images.Add(ResourceUI.info_small);
            control.Images.Add(ResourceUI.warning_small);

            control.TransparentColor = System.Drawing.Color.Transparent;
            control.Images.SetKeyName(0, "Error");
            control.Images.SetKeyName(1, "Information");
            control.Images.SetKeyName(2, "Warning");

            return control;
        }

        public static RepositoryItemImageComboBox GetInformationIconsRepository(ImageList informationIconsImageList)
        {
            RepositoryItemImageComboBox component = new RepositoryItemImageComboBox();

            component.AutoHeight = false;
            component.GlyphAlignment = DevExpress.Utils.HorzAlignment.Center;
            component.Items.AddRange(new ImageComboBoxItem[] {
                new ImageComboBoxItem("Error",0, 0),
                new ImageComboBoxItem("Information", 1, 1),
                new ImageComboBoxItem("Warning", 2, 2)});

            component.SmallImages = informationIconsImageList;

            return component;
        }

        /// <summary>
        /// Create default controls for static use.
        /// </summary>
        static StatusGridColumn()
        {
            InformationImageList = GetInformationIconsImageList();
            InformationIconsRepository = GetInformationIconsRepository(InformationImageList);
        }

        public static ImageList InformationImageList { get; }
        public static RepositoryItemImageComboBox InformationIconsRepository {get;}


        public static void SetStatusGridColumn(this GridColumn gridColumn)
        {
            gridColumn.OptionsColumn.AllowEdit = false;
            //gridColumn.OptionsFilter.AllowFilter = false;
            gridColumn.FilterMode = DevExpress.XtraGrid.ColumnFilterMode.DisplayText;
            gridColumn.ColumnEdit = InformationIconsRepository;
            gridColumn.OptionsColumn.ShowCaption = false;
            gridColumn.Width = 30;

        }


    }
}
