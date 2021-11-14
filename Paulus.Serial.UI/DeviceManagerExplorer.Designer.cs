namespace Paulus.Serial.UI
{
    partial class DeviceManagerExplorer
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gridControlDevices = new DevExpress.XtraGrid.GridControl();
            this.winExplorerViewDevices = new DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlDevices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.winExplorerViewDevices)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControlDevices
            // 
            this.gridControlDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlDevices.Location = new System.Drawing.Point(0, 0);
            this.gridControlDevices.MainView = this.winExplorerViewDevices;
            this.gridControlDevices.Name = "gridControlDevices";
            this.gridControlDevices.Size = new System.Drawing.Size(269, 213);
            this.gridControlDevices.TabIndex = 0;
            this.gridControlDevices.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.winExplorerViewDevices});
            // 
            // winExplorerViewDevices
            // 
            this.winExplorerViewDevices.GridControl = this.gridControlDevices;
            this.winExplorerViewDevices.Name = "winExplorerViewDevices";
            // 
            // DeviceManagerExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControlDevices);
            this.Name = "DeviceManagerExplorer";
            this.Size = new System.Drawing.Size(269, 213);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlDevices)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.winExplorerViewDevices)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControlDevices;
        private DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView winExplorerViewDevices;
    }
}
