using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Paulus.IO;

namespace Paulus.Serial.GasMixer
{
    /// <summary>
    /// Settings represent the state of the gas mixer device only after connection.
    /// Settings are storable in two files: cylinder file and gas file.
    /// </summary>
    public class GasMixerSettings : DeviceSettingsBase
    {
        public GasMixerSettings()
        {
            DataTables = new GasMixerSettingsDataTables(this);
        }

        //public GasMixerSettings(string path) : base(path)
        //{
        //    DataTables = new GasMixerSettingsDataTables(this);
        //}

        //public GasMixerSettings(XmlNode node) : base(node)
        //{
        //    DataTables = new GasMixerSettingsDataTables(this);
        //}

        /// <summary>
        /// The deviceManager should be supplied as a parameter in all cases because the cylinders library is used.
        /// </summary>
        /// <param name="deviceManager"></param>
        public GasMixerSettings(DeviceManager deviceManager) :
            base(deviceManager)
        {
            DataTables = new GasMixerSettingsDataTables(this);
        }

        public GasMixerSettings(DeviceManager deviceManager, string path) :
            base(deviceManager, path)
        {
            DataTables = new GasMixerSettingsDataTables(this);
        }

        public GasMixerSettings(DeviceManager deviceManager, XmlElement xmlDevice) :
            base(deviceManager, xmlDevice)
        {
            DataTables = new GasMixerSettingsDataTables(this);
        }

        protected CylinderLibrary cylinderLibrary;
        /// <summary>
        /// Cylinder library must be predefined before any standard operation of the object.
        /// The DeviceManager must have preloaded libraries if LoadFromXml or LoadFromFile is used.
        /// </summary>
        public CylinderLibrary CylinderLibrary
        {
            get
            {
                return cylinderLibrary;
            }
            set
            {
                cylinderLibrary = value;
            }
        }

        public Dictionary<int, MFC> MFCs { get; private set; } = new Dictionary<int, MFC>();

        public Dictionary<int, Port> Ports { get; private set; } = new Dictionary<int, Port>();

        public MFC BalanceMfc { get; set; }

        //the total target flow is used only in concentration mode
        public float TotalTargetFlowInCcm { get; set; }

        //this is used by runtime settings only

        #region Runtime Settings only
        public float TotalActualFlowInCcm { get; set; }
        public Mode Mode { get; set; }

        public void CreateMFCs(int mfcsCount)
        {
            MFCs = new Dictionary<int, MFC>();
            for (int i = 1; i <= mfcsCount; i++)
                MFCs.Add(i, new MFC(i));
        }

        public void CreatePorts(int portsCount)
        {
            Ports = new Dictionary<int, Port>();
            for (int i = 1; i <= portsCount; i++)
                Ports.Add(i, new Port(i));
        }


        #endregion

        public override string ToXmlString()
        {
            //if (string.IsNullOrWhiteSpace(DeviceName)) DeviceName = Commander.Name;

            StringBuilder sb = new StringBuilder();
            //the mode is in fact needed only in fast connect
            sb.AppendLine($"<device type=\"gas mixer\" name=\"{DeviceName}\" cylinders=\"{cylinderLibrary.Name}\" mode = \"{Mode}\" guid=\"{{{DeviceId}}}\" >");
            sb.AppendLine("<ports>");
            foreach (Port port in Ports.Values)
                sb.AppendLine($"<port id=\"{port.ID}\" cylinder_id=\"{port.Cylinder.ID}\" />"); //CylinderNumber

            sb.AppendLine("</ports>");

            sb.AppendLine("<mfcs>");
            foreach (MFC mfc in MFCs.Values)
            {
                sb.AppendLine($"<mfc id=\"{mfc.ID}\" current_port_id=\"{mfc.CurrentPort.ID}\" size=\"{mfc.SizeInCcm}\" >");
                foreach (Port port in mfc.Ports)
                    sb.AppendLine($"<port id=\"{port.ID}\"/>");

                sb.AppendLine("</mfc>");
            }
            sb.AppendLine("</mfcs>");

            sb.AppendLine($"<concentration total_target_flow=\"{TotalTargetFlowInCcm}\" balance =\"{BalanceMfc.ID}\" >");
            foreach (MFC mfc in MFCs.Values)
                sb.AppendLine($"<mfc id=\"{mfc.ID}\" target_concentration=\"{mfc.TargetConcentrationInPpm}\" />");
            sb.AppendLine("</concentration>");

            sb.AppendLine("<flow>");
            foreach (MFC mfc in MFCs.Values)
                sb.AppendLine($"<mfc id=\"{mfc.ID}\" target_flow=\"{mfc.TargetFlowInCcm}\" />");
            sb.AppendLine("</flow>");

            sb.AppendLine("<purge>");
            foreach (MFC mfc in MFCs.Values)
                sb.AppendLine($"<mfc id=\"{mfc.ID}\" on=\"{(mfc.IsPurgeOn ? "yes" : "no")}\" target_flow=\"{mfc.TargetPurgeFlowInCcm}\" />");
            sb.AppendLine("</purge>");


            sb.AppendLine("</device>");
            return sb.ToString();
        }

        public override void LoadFromXml(XmlElement xmlDevice)
        {
            ////if the name is defined then set its parent name from here
            //DeviceName = xmlDevice.Attributes["name"].Value;
            //if (Commander != null) Commander.Name = DeviceName;
            base.LoadFromXml(xmlDevice);

            //read the cylinders from the node name
            cylinderLibrary = (CylinderLibrary)DeviceManager.Libraries[xmlDevice.Attributes["cylinders"].Value];


            if (xmlDevice.HasAttribute("mode"))
                Mode = (Mode)Enum.Parse(typeof(Mode), xmlDevice.Attributes["mode"].Value);

            //load ports
            Ports.Clear();
            XmlNodeList nodes = xmlDevice.SelectNodes("ports/port");
            foreach (XmlNode node in nodes)
            {
                XmlElement element = node as XmlElement;
                int portID = int.Parse(element.Attributes["id"].Value);
                string cylinderNumber = element.Attributes["cylinder_id"].Value;

                //any invalid cylinder number will be interpreted as AIR
                Cylinder cylinder =
                    cylinderLibrary[cylinderNumber] ?? cylinderLibrary["NONE"];

                Ports.Add(portID, new Port(portID, cylinder));
            }

            //load mfcs
            MFCs.Clear();
            nodes = xmlDevice.SelectNodes("mfcs/mfc");
            foreach (XmlNode node in nodes)
            {
                XmlElement element = node as XmlElement;
                int mfcID = int.Parse(element.Attributes["id"].Value);
                int currentPortID = int.Parse(element.Attributes["current_port_id"].Value);
                float sizeInCcm = float.Parse(element.Attributes["size"].Value);

                MFC newMfc = new MFC(mfcID);
                XmlNodeList portNodes = node.SelectNodes("port");

                List<Port> ports = portNodes.Cast<XmlNode>().Select(n2 =>
                     Ports[int.Parse((n2 as XmlElement).Attributes["id"].Value)]).ToList();

                MFCs.Add(mfcID, new MFC(mfcID, ports) { CurrentPort = Ports[currentPortID], SizeInCcm = sizeInCcm });
            }

            //load concentration mode settings
            XmlElement concentrationElement = xmlDevice["concentration"];
            TotalTargetFlowInCcm = float.Parse(concentrationElement.Attributes["total_target_flow"].Value);
            BalanceMfc = MFCs[int.Parse(concentrationElement.Attributes["balance"].Value)];
            nodes = concentrationElement.SelectNodes("mfc");
            foreach (XmlNode node in nodes)
            {
                XmlElement element = node as XmlElement;
                int mfcID = int.Parse(element.Attributes["id"].Value);
                MFC mfc = MFCs[mfcID];
                mfc.TargetConcentrationInPpm = float.Parse(element.Attributes["target_concentration"].Value);
            }

            //flow mode
            nodes = xmlDevice.SelectNodes("flow/mfc");
            foreach (XmlNode node in nodes)
            {
                XmlElement element = node as XmlElement;
                int mfcID = int.Parse(element.Attributes["id"].Value);
                MFC mfc = MFCs[mfcID];
                mfc.TargetFlowInCcm = float.Parse(element.Attributes["target_flow"].Value);
            }

            //purge mode
            nodes = xmlDevice.SelectNodes("purge/mfc");
            foreach (XmlNode node in nodes)
            {
                XmlElement element = node as XmlElement;
                int mfcID = int.Parse(element.Attributes["id"].Value);
                MFC mfc = MFCs[mfcID];
                mfc.IsPurgeOn = element.GetAttributeOrElementBool("on") ?? true;

                mfc.TargetPurgeFlowInCcm = float.Parse(element.Attributes["target_flow"].Value);
            }
        }

        public GasMixerSettingsDataTables DataTables { get; }

    }
}
