using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paulus.Forms
{
    /// <summary>
    /// Represents an input dialog form with validation (optional).
    /// </summary>
    /// <example>
    /////main parameters
    ///int maxValue = 500;
    ///int currentSpotValue = 0;
    ///
    ///InputDialog input;
    ///
    /////this must be called once in order to initialize the input box
    ///private void initializeInputBox()
    ///{
    ///    //basic way to initialize the dialog box
    ///    input = new InputDialog(string.Format("Select a value between 0 and {0}.", maxValue), "Input value", currentSpotValue.ToString());
    ///
    ///    ////alternative way to initialize the dialog box
    ///    //input = new InputDialog();
    ///    //input.Prompt = string.Format("Select a value between 0 and {0}.", maxValue);
    ///    //input.Text = "Input value";
    ///    //input.Input = currentSpotValue.ToString(); //fill the default value
    ///    
    ///    input.Validating += input_Validating;
    ///    input.ErrorText = string.Format("Please enter a value between 0 and {0}.", maxValue);
    ///}
    ///
    ///void input_Validating(object sender, CancelEventArgs e)
    ///{
    ///    int iValue;
    ///    bool isValid = int.TryParse((sender as InputDialog).Input, out iValue);
    ///    isValid &= iValue &lt;= maxValue && iValue &gt;= 0;
    ///    e.Cancel = !isValid;
    ///}
    ///
    /////this must be called to show the input box
    ///private void updateCurrentValueFromInput()
    ///{
    ///    DialogResult result = input.ShowDialog();
    ///    if (result == System.Windows.Forms.DialogResult.OK)
    ///        currentSpotValue = int.Parse(input.Input);
    ///}

    /// </example>
    public partial class InputDialog : Form
    {
        public InputDialog()
        {
            InitializeComponent();

            _errorText = "Please enter a valid value.";
        }

        public InputDialog(string prompt, string caption, string defaultInput = "")
        {
            InitializeComponent();

            _errorText = "Please enter a valid value.";
            lblPrompt.Text = prompt;
            Text = caption;
            txtOutput.Text = defaultInput;
        }

        public string Input { get { return txtOutput.Text; } set { txtOutput.Text = value; } }

        public string Prompt { get { return lblPrompt.Text; } set { lblPrompt.Text = value; } }

        string _errorText;
        public string ErrorText { get { return _errorText; } set { _errorText = value; } }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //notify the user to validate the input
            CancelEventArgs args = new CancelEventArgs(false);
            OnValidating(args);

            if (!args.Cancel) //everything ok, just set the dialog result and hide the form (this is obligatory)
                DialogResult = DialogResult.OK;
            else //show the error and ask the user to reset the input
                errorProvider1.SetError(txtOutput, _errorText);
        }

        //clear the error every time the form is shown
        private void InputDialog_Activated(object sender, EventArgs e)
        {
            errorProvider1.Clear();
        }

    }
}
