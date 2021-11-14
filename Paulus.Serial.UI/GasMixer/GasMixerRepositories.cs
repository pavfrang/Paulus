using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using Paulus.Serial.GasMixer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Paulus.Serial.UI.GasMixer
{
    //https://documentation.devexpress.com/WindowsForms/9511/Controls-and-Libraries/Editors-and-Simple-Controls/Simple-Editors/Examples/How-to-Share-Editors-Between-Multiple-XtraGrid-Controls
    public static class GasMixerRepositories
    {
        static GasMixerRepositories()
        {
            loadRepositories();
        }

        public static PersistentRepository Repository { get; } = new PersistentRepository();

        private static void loadRepositories()
        {
            //RepositoryCylinders.DropDownRows = 20;
            RepositoryCylinders2.DropDownRows = 20;

            RepositoryConcentrationUnits.Items.AddRange(new string[] { "%", "ppm" });
            RepositoryFlowUnits.Items.AddRange(new string[] { "l/min", "cm³/min" });

            var combos = new RepositoryItemComboBox[] {// RepositoryCylinders,
                RepositoryPorts, RepositoryConcentrationUnits, RepositoryFlowUnits };
            foreach (var c in combos)
                c.TextEditStyle = TextEditStyles.DisableTextEditor;
            //RepositoryCylinders2.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

            Repository.Items.AddRange(combos);
            Repository.Items.Add(RepositoryCylinders2);

            RepositoryCheckOnly.CheckStyle = CheckStyles.Radio;
            Repository.Items.Add(RepositoryCheckOnly);
            //prevent the value to be changed if the checkbox is already checked
            RepositoryCheckOnly.EditValueChanging += BooleanRepository_EditValueChanging;

            //automatically change the cell value
            foreach (RepositoryItem item in Repository.Items)
                item.EditValueChanged += Repository_EditValueChanged;
        }

        //public static RepositoryItemComboBox RepositoryCylinders { get; private set; } = new RepositoryItemComboBox();

        public static RepositoryItemComboBox RepositoryPorts { get; private set; } = new RepositoryItemComboBox();

        public static RepositoryItemComboBox RepositoryConcentrationUnits { get; private set; } = new RepositoryItemComboBox();

        public static RepositoryItemComboBox RepositoryFlowUnits { get; private set; } = new RepositoryItemComboBox();

        //https://documentation.devexpress.com/WindowsForms/9496/Controls-and-Libraries/Editors-and-Simple-Controls/Simple-Editors/Examples/How-to-Create-Lookup-Editor-and-Bind-It-to-Data-Source
        public static RepositoryItemLookUpEdit RepositoryCylinders2 { get; private set; } = new RepositoryItemLookUpEdit();

        public static RepositoryItemCheckEdit RepositoryCheckOnly { get; private set; } = new RepositoryItemCheckEdit();

        public static void LoadCylinders(CylinderLibrary cylinderLibrary)
        {
            //RepositoryCylinders.Items.Clear();
            //RepositoryCylinders.Items.AddRange(cylinderLibrary.Cylinders);

            RepositoryCylinders2.DataSource = cylinderLibrary.GetDataTable();
            RepositoryCylinders2.ValueMember = RepositoryCylinders2.DisplayMember = "Cylinder";
            RepositoryCylinders2.BestFitMode = BestFitMode.BestFitResizePopup;
            RepositoryCylinders2.SearchMode = SearchMode.OnlyInPopup;
            RepositoryCylinders2.AutoSearchColumnIndex = 1;
            //RepositoryCylinders2.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        }

        public static void LoadPorts(IEnumerable<Port> ports)
        {
            RepositoryPorts.Items.Clear();
            RepositoryPorts.Items.AddRange(ports.Select(p => p.ID).ToArray());
        }

        private static void BooleanRepository_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            if ((bool)e.OldValue) e.Cancel = true;
        }

        private static void Repository_EditValueChanged(object sender, EventArgs e)
        {
            BaseEdit edit = sender as BaseEdit;
            BaseView view = (edit.Parent as GridControl).MainView;
            view.PostEditor();
        }
    }

}
