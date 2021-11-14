using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Serial.UI
{
    public class TextEditWithUnitComboBox
    {
        //The unitConversioFunc should have the following signature (the names only may be different):
        //float ConvertVolume(string fromUnit, string toUnit, float fromValue)

        //see functions inside the UnitConversionsLibrary static class
        public TextEditWithUnitComboBox(
            TextEdit textEdit, ComboBoxEdit comboEdit,
                string[] units,
                Func<string, string, float, float> unitConversionFunc)
        {
            TextEdit = textEdit;
            ComboEdit = comboEdit;
            convertFunc = unitConversionFunc;

            initializeControls(units);
        }

        private void initializeControls(string[] units)
        {
            //allow only float
            TextEdit.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            ComboEdit.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            ComboEdit.Properties.Items.Clear();
            ComboEdit.Properties.Items.AddRange(units);
            ComboEdit.EditValueChanging += comboEdit_EditValueChanging;
            ComboEdit.EditValueChanged += unit_EditValueChanged;
        }

        //sets the values and overrides the handlers
        public virtual void SetValues(float value, string unit)
        {
            ComboEdit.EditValueChanging -= comboEdit_EditValueChanging;
            ComboEdit.EditValueChanged -= unit_EditValueChanged;

            TextEdit.EditValue = value;
            ComboEdit.EditValue = unit;

            ComboEdit.EditValueChanging += comboEdit_EditValueChanging;
            ComboEdit.EditValueChanged += unit_EditValueChanged;
        }

        private string newUnit, oldUnit;
        private void comboEdit_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            newUnit = (string)e.NewValue;
            oldUnit = (string)e.OldValue;
        }

        protected virtual void unit_EditValueChanged(object sender, EventArgs e)
        {
            TextEdit.EditValue = convertFunc(oldUnit, newUnit, (float)TextEdit.EditValue);
        }

        Func<string, string, float, float> convertFunc;


        public TextEdit TextEdit { get; }

        public ComboBoxEdit ComboEdit { get; }

    }


    //Also changes the background of the textbox to yellow only if the user changes the edit text.
    //If the text is changed from a unit change then the textbox is NOT drawn yellow.
    public class TextEditWithUnitComboBoxWithUserChangeTracker : TextEditWithUnitComboBox
    {
        //the unitConversioFunc should have the following signature (the names only may be different):
        //float ConvertVolume(string fromUnit, string toUnit, float fromValue)
        public TextEditWithUnitComboBoxWithUserChangeTracker(
            TextEdit textEdit, ComboBoxEdit comboEdit,
                string[] units,
                Func<string, string, float, float> unitConversionFunc) : base(textEdit, comboEdit, units, unitConversionFunc)
        {

            TextEdit.EditValueChanged += TextEdit_EditValueChanged;
        }

        public void SetValues(float value, string unit, bool changeColorToTracked)
        {
            if (!changeColorToTracked)
                TextEdit.EditValueChanged -= TextEdit_EditValueChanged;

            base.SetValues(value, unit);

            if (!changeColorToTracked)
                TextEdit.EditValueChanged += TextEdit_EditValueChanged;
        }

        private void TextEdit_EditValueChanged(object sender, EventArgs e)
        {
            TextEdit.BackColor = System.Drawing.Color.LightYellow;
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ValueChanged;

        public event EventHandler UnitChanged;


        protected override void unit_EditValueChanged(object sender, EventArgs e)
        {
            TextEdit.EditValueChanged -= TextEdit_EditValueChanged;
            base.unit_EditValueChanged(sender, e);
            UnitChanged?.Invoke(this, EventArgs.Empty);
            TextEdit.EditValueChanged += TextEdit_EditValueChanged;
        }
    }
}
