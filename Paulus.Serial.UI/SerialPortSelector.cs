using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Paulus.Serial;
using System.IO.Ports;

namespace Paulus.Serial.UI
{
    public partial class SerialPortSelector : UserControl
    {
        public SerialPortSelector()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                initializeDialog();

                RefreshPorts();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            ShowPortDialog();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshPorts();
        }

        private void cboPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_port != null)
            {
                if (_port != null && _port.IsOpen)
                    _port.Close();
                _port.PortName = cboPorts.Text;
            }
            else
                _port = new SerialPort(cboPorts.Text);
        }


        SerialPortDialog settings;

        private void initializeDialog()
        {
            settings = new SerialPortDialog();
            settings.Text = "Select the COM port";
        }

        private SerialPort _port;
        [Category("Serial Port")]
        public SerialPort Port
        {
            get
            {
                if (_port == null && cboPorts.Items.Count > 0) //this is needed for the first time
                    _port = new SerialPort(cboPorts.Text);

                return _port;
            }
            set
            {
                _port = value;
                if (_port != null)
                    cboPorts.SelectedItem = _port.PortName;
            }
        }

        [Category("Serial Port"), DefaultValue("Port:")]

        public string PortLabel
        {
            get { return lblPortName.Text; }
            set
            {
                lblPortName.Text = value;
            }
        }

        [Category("Serial Port"), DefaultValue(true)]

        public bool CanEditPortSettings
        {
            get { return btnEdit.Enabled; }
            set { btnEdit.Enabled = value; }
        }


        public void ShowPortDialog()
        {
            //load the current port to the port settings
            if (_port != null) settings.Port = _port;

            //load port settings 
            if (settings.ShowDialog() == DialogResult.OK)
            {
                this.Port = settings.Port;
                cboPorts.SelectedIndexChanged -= cboPorts_SelectedIndexChanged;
                cboPorts.SelectedItem = settings.Port.PortName;
                cboPorts.SelectedIndexChanged += cboPorts_SelectedIndexChanged;
            }
        }

        public void RefreshPorts()
        {
            cboPorts.Items.Clear();
            cboPorts.Items.AddRange(SerialPort.GetPortNames().Distinct().ToArray());

            //select the first available port
            if (cboPorts.Items.Count > 0)
                cboPorts.SelectedIndex = 0;
        }

        public string[] PortNames()
        {
            return SerialPort.GetPortNames().Distinct().ToArray();
        }

    }
}
