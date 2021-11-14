using DevExpress.XtraEditors;
using Paulus.Serial.InfusionPump;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paulus.Serial.UI.InfusionPump
{
    public static class InfusionPumpControls
    {
        public static void AttachSyringesControls(LookUpEdit edit, InfusionPumpSettings settings)
        {
            edit.EditValueChanged += setToUnsavedStateHandler;
            edit.EditValueChanged += (o, e) => settings.Syringe = (Syringe)edit.EditValue;

            FillSyringes(edit, settings);
        }

        public static void FillSyringes(LookUpEdit edit, InfusionPumpSettings settings)
        {
            DataTable table = settings.SyringeLibrary.GetDataTable();
            edit.Properties.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
            edit.Properties.DataSource = table;
            edit.Properties.ValueMember = edit.Properties.DisplayMember = "Syringe";
            edit.Properties.ForceInitialize();
            edit.Properties.PopulateColumns();

            if (edit.Properties.Columns.Count != 0)
            {
                edit.Properties.Columns["Syringe"].Visible = false;
                edit.Properties.Columns["ID"].Visible = false;
                edit.Properties.BestFit();
            }
            edit.EditValue = settings.Syringe;
        }

        public static void FillLiquids(LookUpEdit edit, InfusionPumpSettings settings)
        {
            DataTable table = settings.LiquidLibrary.GetDataTable();
            edit.Properties.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
            edit.Properties.DataSource = table;
            edit.Properties.DisplayMember = edit.Properties.ValueMember = "Liquid";
            edit.Properties.PopulateColumns();
            //ComboLiquids.Properties.Columns["Liquid"].Visible = false;
            edit.Properties.BestFit();

            edit.EditValue = settings.Liquid;
        }

        private static List<TextEditWithUnitComboBoxWithUserChangeTracker> textEditUnitTrackers = new List<TextEditWithUnitComboBoxWithUserChangeTracker>();

        #region Initial Volume
        public static void AttachInitialVolumeControls(TextEdit initialVolumeEdit, ComboBoxEdit initialVolumeUnit, InfusionPumpSettings settings)
        {

            TextEditWithUnitComboBoxWithUserChangeTracker initialVolumeTracker = new TextEditWithUnitComboBoxWithUserChangeTracker(
                 initialVolumeEdit, initialVolumeUnit, new string[] { "ml", "μl" }, UnitConversionsLibrary.ConvertVolume);

            initialVolumeTracker.ValueChanged += (o, e) =>
            {
                float value;
                bool parsed = float.TryParse(initialVolumeEdit.Text, out value);
                if (parsed) settings.InitialVolume = Convert.ToSingle(initialVolumeEdit.Text);
            };
            initialVolumeTracker.UnitChanged += (o, e) =>
            {
                float value;
                bool parsed = float.TryParse(initialVolumeEdit.Text, out value);
                if (parsed) settings.InitialVolume = Convert.ToSingle(initialVolumeEdit.Text);
                settings.InitialVolumeUnit = initialVolumeUnit.Text;
            };

            textEditUnitTrackers.Add(initialVolumeTracker);

            LoadInitialVolume(initialVolumeEdit, initialVolumeUnit, settings);
        }
        public static void LoadInitialVolume(TextEdit initialVolumeEdit, ComboBoxEdit initialVolumeUnit, InfusionPumpSettings settings)
        {
            initialVolumeEdit.EditValue = settings.InitialVolume;
            initialVolumeUnit.EditValue = settings.InitialVolumeUnit != "μl" ?
                settings.InitialVolumeUnit : "ul";
        }
        #endregion

        #region Target Volume
        public static void AttachTargetVolumeControls(TextEdit targetVolumeEdit, ComboBoxEdit targetVolumeUnit, InfusionPumpSettings settings)
        {

            TextEditWithUnitComboBoxWithUserChangeTracker targetVolumeTracker = new TextEditWithUnitComboBoxWithUserChangeTracker(
                 targetVolumeEdit, targetVolumeUnit, new string[] { "ml", "μl" }, UnitConversionsLibrary.ConvertVolume);

            targetVolumeTracker.ValueChanged += (o, e) =>
            {
                float value;
                bool parsed = float.TryParse(targetVolumeEdit.Text, out value);
                if (parsed) settings.TargetVolume = Convert.ToSingle(targetVolumeEdit.Text);
            };
            targetVolumeTracker.UnitChanged += (o, e) =>
            {
                float value;
                bool parsed = float.TryParse(targetVolumeEdit.Text, out value);
                if (parsed) settings.TargetVolume = Convert.ToSingle(targetVolumeEdit.Text);
                settings.TargetVolumeUnit = targetVolumeUnit.Text;
            };

            textEditUnitTrackers.Add(targetVolumeTracker);
            LoadTargetVolume(targetVolumeEdit, targetVolumeUnit, settings);
        }

        public static void LoadTargetVolume(TextEdit targetVolumeEdit, ComboBoxEdit targetVolumeUnit, InfusionPumpSettings settings)
        {
            targetVolumeEdit.EditValue = settings.TargetVolume;
            targetVolumeUnit.EditValue = settings.TargetVolumeUnit != "μl" ?
                settings.TargetVolumeUnit : "ul";
        }
        #endregion

        #region Infusion rate
        public static void AttachInfusionRateControls(TextEdit infusionRateEdit, ComboBoxEdit infusionRateUnit, InfusionPumpSettings settings)
        {

            infusionRateEdit.EditValueChanged += setToUnsavedStateHandler;
            infusionRateEdit.EditValueChanged += (o, e) =>
            {
                float value;
                bool parsed = float.TryParse(infusionRateEdit.Text, out value);
                if (parsed) settings.InfusionRate = value;
            };
            infusionRateUnit.EditValueChanged += (o, e) => settings.InfusionRateUnit = infusionRateUnit.Text;

            LoadInfusionRate(infusionRateEdit, infusionRateUnit, settings);
        }

        public static void LoadInfusionRate(TextEdit infusionRateEdit, ComboBoxEdit infusionRateUnit, InfusionPumpSettings settings)
        {
            infusionRateEdit.EditValue = settings.InfusionRate;
            if (settings.InfusionRateUnit.StartsWith("μ"))
            {
                settings.InfusionRateUnit = "u" + settings.InfusionRateUnit.Substring(1);
                infusionRateUnit.EditValue = settings.InfusionRateUnit;
            }

        }
        #endregion

        private static void setToUnsavedStateHandler(object sender, EventArgs e) => (sender as Control).BackColor = Color.LightYellow;

    }
}
