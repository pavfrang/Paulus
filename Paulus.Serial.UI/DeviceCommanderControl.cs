using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Paulus.Serial.UI
{
    //the class cannot be abstract or else cannot design the control in design mode 
    public partial class DeviceCommanderControl : UserControl
    {
        public DeviceCommanderControl()
        {
            InitializeComponent();
        }

        public bool IsConnected { get; protected set; }
        public bool IsConnecting { get; protected set; }



        protected DeviceCommander commander;
        public DeviceCommander Device
        {
            get { return commander; }
            set
            {
                if (commander == value) return;

                commander = value;
                initializeUI();
            }
        }

        protected virtual void initializeUI()
        { }

        protected void traceNotConnected()
        {
            //IsModeLoading = false;
            IsConnecting = IsConnected = false;
            Device.TraceSource.TraceEvent(
                       TraceEventType.Error, 0, $"Could not connect to device at {Device.PortName}.");
        }

        public event EventHandler Connecting;

        public event EventHandler Connected;
        public event EventHandler Disconnected;

        //useful event in order to store for example the state of the object
        public event EventHandler DeviceChanged;
        protected void OnDeviceChanged() =>
            DeviceChanged?.Invoke(this, EventArgs.Empty);


        protected virtual async Task<bool> connect(bool connectFast)
        {
            Device.TraceSource.TraceInformation($"Connecting at {Device.PortName}...");

            //tstStatus.Text = "Assigning Ports/MFCs...";
            //    IsModeLoading = true;

            IsConnecting = true;
            bool connected = await Device.Connect(false, true);
            if (!connected)
            {
                traceNotConnected();
                return false;
            }


            if (!connectFast)
            {
                //this must be used for verification
                bool read = await Device.ReadDeviceInformation();
                if (!read)
                {
                    traceNotConnected();
                    return false;
                }
            }

            IsConnecting = false;
            IsConnected = true;
            Device.TraceSource.TraceInformation(
                $"Successfully connected at {Device.PortName}.");

            return true; //successfully connected if arrived here
        }

        public async Task<bool> Connect(bool connectFast)
        {
            Connecting?.Invoke(this, EventArgs.Empty);
            bool connected = await connect(connectFast);
            if (connected)
                Connected?.Invoke(this, EventArgs.Empty);
            return connected;            
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task<bool> Disconnect()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return true;
        }

        public void SaveDeviceToFile(string path)
        {
            Device?.RuntimeSettings?.SaveToFile(path);
        }

    }
}
