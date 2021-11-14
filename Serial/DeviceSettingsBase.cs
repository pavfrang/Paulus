using Paulus.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Xml;

namespace Paulus.Serial
{
    public abstract class DeviceSettingsBase : ICloneable
    {
        public DeviceSettingsBase() { }

        public DeviceSettingsBase(string path) : this()
        { LoadFromFile(path); }
        public DeviceSettingsBase(XmlElement xmlDevice) : this()
        { LoadFromXml(xmlDevice); }

        //public DeviceSettingsBase(DeviceCommander commander) { Commander = commander; }

        //public DeviceSettingsBase(DeviceCommander commander, string path) : this(commander)
        //{ LoadFromFile(path); }
        //public DeviceSettingsBase(DeviceCommander commander, XmlNode node) : this(commander)
        //{ LoadFromXml(node); }

        public DeviceSettingsBase(DeviceManager deviceManager) { DeviceManager = deviceManager; }

        public DeviceSettingsBase(DeviceManager deviceManager, string path) : this(deviceManager)
        { LoadFromFile(path); }
        public DeviceSettingsBase(DeviceManager deviceManager, XmlElement xmlDevice) : this(deviceManager)
        { LoadFromXml(xmlDevice); }

        protected CultureInfo en = CultureInfo.GetCultureInfo("en-us");

        public DeviceCommander Commander { get; set; }

        public DeviceManager DeviceManager { get; set; }

        public string DeviceName { get; set; }

        public Guid DeviceId { get; set; }

        public Dictionary<string, Library> Libraries { get { return DeviceManager?.Libraries; } }

        public void SaveToFile(string path)
        {
            using (StreamWriter writer = XmlExtensions.GetWriterAndWriteProlog(path))
                writer.Write(ToXmlString());
        }
        public abstract string ToXmlString();

        public void LoadFromFile(string path) =>
            LoadFromXml(XmlExtensions.GetRootXmlElement(path));
        //{
        //    XmlDocument doc = new XmlDocument();
        //    doc.Load(path);
        //    LoadFromXml(doc["device"]);
        //}

        public virtual void LoadFromXml(XmlElement xml)
        {
            string sGuid = xml.GetAttributeOrElementText("guid");
            Guid guid;
            bool parsed = Guid.TryParseExact(sGuid, "B", out guid);
            DeviceId = parsed ? guid : Guid.NewGuid();
            if (Commander != null) Commander.Guid = DeviceId;

            //if the name is defined then set its parent name from here
            DeviceName = xml.Attributes["name"].Value;
            if (Commander != null) Commander.Name = DeviceName;
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
