using System;
using System.IO.Ports;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Paulus.Collections;
using System.Text.RegularExpressions;

namespace Paulus.Serial.UI
{
    /// <summary>
    /// Implements a dialog box for the selection of a serial port.
    /// </summary>
    /// <example>
    /// PortSettingsDialog settings;
    /// SerialPortMessageMonitor portMonitor;
    /// SerialPort Port;
    /// 
    /// private void InitializeDialog()
    /// {
    ///     settings = new PortSettingsDialog();
    ///     settings.Text = "Select the COM port";
    ///     portMonitor = new SerialPortMessageMonitor();
    /// }
    /// 
    /// private void SelectPort()
    /// {
    ///     //load the current port to the port settings
    ///     if (Port != null) settings.Port = Port;
    ///     
    ///     //load port settings 
    ///     if (settings.ShowDialog() == System.Windows.Forms.DialogResult.OK)
    ///     {
    ///         portMonitor.Port = settings.Port;
    ///         this.Port = settings.Port;
    ///     }
    /// }
    /// </example>
    public partial class SerialPortDialog : Form
    {
        #region Constructors and initialization
        public SerialPortDialog()
        {
            InitializeComponent();

            InitializeMembers();

            InitializeControls();

            //_port = new SerialPort();
        }

        public SerialPortDialog(SerialPort port) : this()
        {
            this.Port = port;
        }

        private void InitializeMembers()
        {
            stopBits = new BiDictionaryOneToOne<float, StopBits>();
            stopBits.Add(1.0f, StopBits.One);
            stopBits.Add(1.5f, StopBits.OnePointFive);
            stopBits.Add(2.0f, StopBits.Two);
        }

        private void InitializeControls()
        {
            //load port names
            cboPorts.Items.Clear();
            cboPorts.Items.AddRange(SerialPort.GetPortNames().Distinct().OrderBy(n =>
            {
                //get com number if it exists
                Match m = Regex.Match(n, @"\d+");
                if (m.Success) return int.Parse(m.Value).ToString("00");
                else return n;
            }).ToArray());

            //select the first item (as the default)
            if (cboPorts.Items.Count > 0)
                cboPorts.SelectedIndex = 0;
            else return;

            //load data bits
            cboDataBits.Items.Clear();
            foreach (int item in Enumerable.Range(4, 5))
                cboDataBits.Items.Add(item);
            cboDataBits.SelectedItem = 8;

            //load baud rate
            cboBaudRate.Items.Clear();
            foreach (var item in Enum.GetValues(typeof(BaudRate)))
                cboBaudRate.Items.Add((int)item);
            cboBaudRate.SelectedItem = 9600;

            //load parity values
            cboParity.Items.Clear();
            foreach (var item in Enum.GetValues(typeof(Parity)))
                cboParity.Items.Add(item);
            cboParity.SelectedItem = Parity.None;

            //load stop bits
            cboStopBits.Items.Clear();
            cboStopBits.Items.Add(stopBits[StopBits.One]);
            cboStopBits.Items.Add(stopBits[StopBits.OnePointFive]);
            cboStopBits.Items.Add(stopBits[StopBits.Two]);
            cboStopBits.SelectedItem = 1.0f;
        }
        #endregion

        #region Members

        BiDictionaryOneToOne<float, StopBits> stopBits;

        SerialPort _port;
        public SerialPort Port
        {
            get { return _port; }
            set
            {
                _port = value;
                if (_port != null)
                {
                    cboPorts.SelectedItem = _port.PortName;
                    cboBaudRate.SelectedItem = _port.BaudRate;
                    cboDataBits.SelectedItem = _port.DataBits;
                    cboParity.SelectedItem = _port.Parity;
                    cboStopBits.SelectedItem = stopBits[_port.StopBits];
                    txtReadTimeout.Text = _port.ReadTimeout.ToString();
                    txtWriteTimeout.Text = _port.WriteTimeout.ToString();
                }
            }
        }
        #endregion

        private void btnOK_Click(object sender, EventArgs e)
        {
            //set the port properties
            _port.PortName = (string)cboPorts.SelectedItem;
            _port.BaudRate = (int)cboBaudRate.SelectedItem;
            _port.DataBits = (int)cboDataBits.SelectedItem;
            _port.Parity = (Parity)cboParity.SelectedItem;
            _port.StopBits = stopBits[(float)cboStopBits.SelectedItem];
            _port.ReadTimeout = int.Parse(txtReadTimeout.Text);
            _port.WriteTimeout = int.Parse(txtWriteTimeout.Text);

            //this also hides the form
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Refresh the serial ports list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PortSettingsDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
                InitializeControls();
        }

        private void txtReadTimeout_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int i;
            e.Cancel = !int.TryParse(txtReadTimeout.Text, out i);
        }

        private void txtWriteTimeout_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int i;
            e.Cancel = !int.TryParse(txtWriteTimeout.Text, out i);

        }
    }
}
