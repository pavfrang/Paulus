using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors.Repository;
using System.IO.Ports;
using Paulus.UI;

namespace Paulus.Serial.UI
{
    public partial class DeviceManagerExplorer : UserControl
    {
        public DeviceManagerExplorer()
        {
            InitializeComponent();

            initializeControls();
        }

        private RepositoryItemComboBox repositoryItemComboBoxPorts;

        private void initializeControls()
        {
             gridControlDevices.RepositoryItems.Add(repositoryItemComboBoxPorts =
                new RepositoryItemComboBox());
        }


        protected DeviceManager deviceManager;
        public DeviceManager DeviceManager
        {
            get { return deviceManager; }
            set {
                if (deviceManager == value) return;
                deviceManager = value;
                updateDevicesExplorer(deviceManager.UpdateDeviceTable());
            }
        }
        public event EventHandler DeviceManagerChanged;
        protected void OnDeviceManagerChanged() =>
            DeviceManagerChanged?.Invoke(this, EventArgs.Empty);

        private void updateDevicesExplorer(DataTable table)
        {
            gridControlDevices.DataSource = table;
            winExplorerViewDevices.PopulateColumns(table);

            var columnSet = winExplorerViewDevices.ColumnSet;
            var columns = winExplorerViewDevices.Columns;

            columns["Port"].AppearanceCell.Font = new Font(columns["Port"].AppearanceCell.Font.FontFamily, 12f);

            columns["Device Type"].OptionsColumn.AllowEdit = false;
            columns["Port"].OptionsColumn.AllowEdit = false;
            columns["Description"].OptionsColumn.AllowEdit = false;
            //columns["Port"].ColumnEdit = repositoryItemComboBoxPorts;
            repositoryItemComboBoxPorts.Items.Clear();
            repositoryItemComboBoxPorts.Items.AddRange(SerialPort.GetPortNames());

            winExplorerViewDevices.OptionsView.Style = DevExpress.XtraGrid.Views.WinExplorer.WinExplorerViewStyle.Large;
            winExplorerViewDevices.OptionsViewStyles.Large.ShowDescription = DevExpress.Utils.DefaultBoolean.True;
            winExplorerViewDevices.OptionsSelection.MultiSelect = true;

            winExplorerViewDevices.Appearance.ItemDescriptionHovered.ForeColor =
                winExplorerViewDevices.Appearance.ItemDescriptionNormal.ForeColor =
                Color.Blue;

            //set.GroupColumn = columns["Device Type"];
            columnSet.TextColumn = columns["Name"];
            columnSet.EnabledColumn = columns["Enabled"];
            columnSet.DescriptionColumn = columns["Description"];
            columnSet.MediumImageColumn = columns["Image"];
        }

        public void UpdateDevicesAndStates()
        {
            winExplorerViewDevices.BackupSelection();
            updateDevicesExplorer(deviceManager.UpdateDeviceTable());
            winExplorerViewDevices.RestoreSelection();
        }

        public void SelectAllDevices()
        {
            winExplorerViewDevices.SelectAll();
        }

        public void SelectDevice(string name)
        {

        }

    }
}
