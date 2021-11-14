using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;

namespace Paulus.Forms
{
    partial class InputDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new Container();
            this.txtOutput = new TextBox();
            this.lblPrompt = new Label();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.errorProvider1 = new ErrorProvider(this.components);
            ((ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtOutput
            // 
            this.errorProvider1.SetIconAlignment(this.txtOutput, ErrorIconAlignment.TopLeft);
            this.txtOutput.Location = new Point(27, 105);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.Size = new Size(296, 20);
            this.txtOutput.TabIndex = 0;
            this.txtOutput.Text = "0";
            // 
            // lblPrompt
            // 
            this.lblPrompt.Location = new Point(24, 17);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new Size(221, 85);
            this.lblPrompt.TabIndex = 1;
            this.lblPrompt.Text = "Prompt";
            // 
            // btnOK
            // 
            this.btnOK.Location = new Point(251, 17);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(72, 26);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new global::System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(251, 49);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(72, 26);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // InputDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new Size(355, 141);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblPrompt);
            this.Controls.Add(this.txtOutput);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Title";
            this.Activated += new global::System.EventHandler(this.InputDialog_Activated);
            ((ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox txtOutput;
        private Label lblPrompt;
        private Button btnOK;
        private Button btnCancel;
        private ErrorProvider errorProvider1;
    }
}