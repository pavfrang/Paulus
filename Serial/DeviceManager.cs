using Paulus.Collections;
using Paulus.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;

using Paulus.Serial.GasMixer;
using System.IO;
using Paulus.IO;
using System.Xml;
using System.Collections;
using System.Data;
using System.Drawing;
using System.ComponentModel;
using Paulus.Serial.InfusionPump;
using Paulus.Common;
using Paulus.Serial.Arduino;
using System.Diagnostics;

namespace Paulus.Serial
{
    public enum DeviceType
    {
        [Description("Environics Series 2000")]
        GasMixer,
        [Description("Cole-Parmer Infusion Pump")]
        InfusionPump,
        [Description("Arduino Uno Rev3")]
        Arduino
    }

    //public static class DevicesExtensions
    //{
    //    public static async Task<bool> TestConnection(this DeviceType deviceType, string portName)
    //    {
    //        switch (deviceType)
    //        {
    //            case DeviceType.GasMixer:
    //                return await GasMixer.GasMixerCommander.IsDeviceConnectedAt(portName);
    //            default:
    //                throw new NotImplementedException(nameof(deviceType));
    //        }

    //    }
    //}

    public class DeviceManager : IEnumerable<DeviceCommander>, IDisposable
    {
        public DeviceManager(string xmlFile)
        {
            loadFromFile(xmlFile);
        }

        public DeviceManager()
        { }

        public string FilePath { get; protected set; }

        #region Collections and IEnumerable methods
        public Dictionary<string, Library> Libraries { get; private set; }

        public Dictionary<SerialPort, DeviceCommander> PortAssociations { get; private set; }
        public Dictionary<string, DeviceCommander> DeviceCommanders { get; private set; }

        /// <summary>
        /// Adding the device via this method allows the firing of the event, without the need of implementing 
        /// </summary>
        /// <param name="device"></param>
        public void AddDevice(DeviceCommander device)
        {
            DeviceCommanders.Add(device.Name, device);
            OnAddedDevice();
        }

        public void RemoveDevice(DeviceCommander device)
        {
            //disconnect device
            device.Dispose();

            //remove all internal assignments
            DeviceCommanders.Remove(device.Name);
            PortAssociations.Remove(device.SerialPort);
        }

        public event EventHandler AddedDevice;
        protected virtual void OnAddedDevice()
            => AddedDevice?.Invoke(this, EventArgs.Empty);

        public DeviceCommander this[string deviceName]
        {
            get
            {
                return
                  DeviceCommanders.ContainsKey(deviceName) ?
                  DeviceCommanders[deviceName] : null;
            }
        }

        public DeviceCommander this[int index]
        {
            get
            {
                return index < DeviceCommanders.Count &&
                    index >= 0 ? DeviceCommanders.ElementAt(index).Value : null;
            }
        }


        public IEnumerator<DeviceCommander> GetEnumerator() => DeviceCommanders.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        public event EventHandler ScanningPort;
        public string CurrentPortScanning { get; private set; }

        public async Task ScanDevices(bool connect)
        {
            string[] portNames = SerialPort.GetPortNames();
            DeviceType[] deviceTypes = Enum.GetValues(typeof(DeviceType)).Cast<DeviceType>().ToArray();

            DeviceCommanders = new Dictionary<string, DeviceCommander>();
            PortAssociations = new Dictionary<SerialPort, DeviceCommander>();

            foreach (string portName in portNames)
            {
                //Debug.WriteLine($"Scanning port {portName}.");
                CurrentPortScanning = portName;
                ScanningPort?.Invoke(this,EventArgs.Empty);

                //foreach (DeviceType deviceType in deviceTypes)
                //{

                //The default cylinder library must be set before scanning any gas mixers!

                //connection is tested in all cases
                if (await SerialDevice.IsDeviceConnectedAt<GasMixerCommander>(portName))
                {
                    //we always assume the default port settings
                    GasMixerCommander agent = new GasMixerCommander(portName);
                    if (connect)
                    {
                        await agent.Connect(false, false);
                        await agent.ReadDeviceInformation();
                        await agent.QueryCylinderInformation();
                    }
                    DeviceCommanders.Add(portName, agent);
                }
                else if (await SerialDevice.IsDeviceConnectedAt<InfusionPumpCommander>(portName))
                {
                    InfusionPumpCommander commander = new InfusionPumpCommander(this);
                    commander.SerialPort = commander.GetDefaultSerialPortSettings(portName);

                    if (connect)
                    {
                        await commander.Connect(false, false);
                        await commander.ReadDeviceInformation();
                    }
                    DeviceCommanders.Add(portName, commander);
                    PortAssociations.Add(commander.SerialPort, commander);
                }

                // }
            }
            CurrentPortScanning = "";
        }

        /// <summary>
        /// Saves last connection port/devices configuration to file.
        /// </summary>
        /// <param name="path"></param> 
        public void SaveToFile(string path)
        {
            using (StreamWriter writer = XmlExtensions.GetWriterAndWriteProlog(path))
            {
                writer.WriteLine("<configuration>");

                writer.WriteLine("<libraries>");
                foreach (var l in Libraries.Values)
                {
                    //string relativePath = PathExtensions.GetRelativePath(path, l.Path);
                    string lPath = Path.GetFileName(l.Path);

                    string libraryType = "";
                    if (l is CylinderLibrary)
                        libraryType = "cylinders";
                    else if (l is Library<Syringe>)
                        libraryType = "syringes";
                    else if (l is Library<SyringeLiquid>)
                        libraryType = "syringe_liquids";

                    writer.WriteLine($"<library type=\"{libraryType}\" file=\"{lPath}\" name=\"{l.Name}\" />");
                }
                writer.WriteLine("</libraries>");

                writer.WriteLine("<serialports>");
                foreach (var entry in PortAssociations)
                {
                    var port = entry.Key;
                    writer.WriteLine($"<serialport name=\"{port.PortName}\" device=\"{entry.Value.Name}\" baudrate=\"{port.BaudRate}\" parity=\"{port.Parity}\" databits=\"{port.DataBits}\" stopbits=\"{port.StopBits}\" />");
                }
                writer.WriteLine("</serialports>");


                writer.WriteLine("<devices>");
                foreach (var entry in DeviceCommanders)
                    writer.WriteLine(entry.Value.EditSettings.ToXmlString());
                writer.WriteLine("</devices>");

                writer.WriteLine("</configuration>");
            }
        }

        private void loadFromFile(string path)
        {
            FilePath = path;

            XmlElement configuration = XmlExtensions.GetRootXmlElement(path);

            //get the libraries first
            Libraries = new Dictionary<string, Library>();
            XmlNodeList nodes = configuration.SelectNodes("libraries/library");
            foreach (XmlElement node in nodes)
            {
                string libraryType = node.Attributes["type"].Value;
                string libraryPath = Path.Combine(Path.GetDirectoryName(path), node.Attributes["file"].Value);
                if (!File.Exists(libraryPath)) libraryPath = PathExtensions.CombineWithExecutablePath(node.Attributes["file"].Value);

                string name = node.Attributes["name"].Value;
                switch (libraryType)
                {
                    case "cylinders":
                        //<library type="cylinders" file ="cylinders.xml" name ="Cylinders #1" />
                        Libraries.Add(name, new CylinderLibrary(name, libraryPath)); break;
                    case "syringes":
                        //<library type="syringes" file ="syringes.xml" name ="Syringes #1" />
                        Libraries.Add(name, new Library<Syringe>(name, libraryPath)); break;
                    case "syringe_liquids":
                        //<library type="syringe_liquids" file ="syringe_liquids.xml" name ="Syringe Liquids #1" />
                        Libraries.Add(name, new Library<SyringeLiquid>(name, libraryPath)); break;
                }
            }


            //then the devices
            DeviceCommanders = new Dictionary<string, DeviceCommander>();
            nodes = configuration.SelectNodes("devices/device");
            foreach (XmlElement xmlDevice in nodes)
            {
                string deviceType = xmlDevice.Attributes["type"].Value;
                //name is set by the xml internally
                //string name = node.Attributes["name"].Value;

                switch (deviceType)
                {
                    case "gas mixer":
                        AddDevice(new GasMixerCommander(this, xmlDevice));
                        break;
                    case "infusion pump":
                        AddDevice(new InfusionPumpCommander(this, xmlDevice));
                        break;
                    case "arduino":
                        AddDevice(new ArduinoRev3Commander(this, xmlDevice));
                        break;
                }
            }

            //and then the port associations 
            PortAssociations = new Dictionary<SerialPort, DeviceCommander>();
            nodes = configuration.SelectNodes("serialports/serialport");
            foreach (XmlElement xmlSerialPort in nodes)
            {
                SerialPort port = xmlSerialPort.ToSerialPort();
                string device = xmlSerialPort.Attributes["device"].Value;

                //add the association only if the device declaration exists in the file
                //else ignore it
                if (DeviceCommanders.ContainsKey(device))
                {
                    PortAssociations.Add(port, DeviceCommanders[device]);
                    //also assign the port to the device commander!
                    DeviceCommanders[device].SerialPort = port;
                }
            }
        }


        public DataTable DeviceTable { get; protected set; }
        public bool AtLeastOneConnectedDevice
        {
            get
            {
                return this.DeviceCommanders.Any(d => d.Value.IsConnected);
            }
        }

        public DataTable UpdateDeviceTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Device Type", typeof(string));
            table.Columns.Add("Port", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Image", typeof(Image));
            table.Columns.Add("Enabled", typeof(bool));

            foreach (var device in this)
            {
                DataRow row = table.NewRow();
                row["Name"] = device.Name;
                row["Device Type"] = device.GetType().GetDescription();

                row["Image"] = device.Image;
                row["Port"] = device.PortName;
                row["Description"] = $"{row["Device Type"]} ({row["Port"]})";
                row["Enabled"] = device.IsConnected;

                table.Rows.Add(row);
            }

            return DeviceTable = table;
        }

        #region IDisposable Support
        private bool _isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    foreach (DeviceCommander c in this) c.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _isDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeviceManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
