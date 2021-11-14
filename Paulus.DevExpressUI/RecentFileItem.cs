using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using DevExpress.XtraBars.Ribbon;
using Paulus.IO;
using Paulus.Collections;

namespace Paulus.UI
{
    public class RecentFileItem
    {
        public string FilePath { get; set; }
        public bool Pinned { get; set; }

        public DateTime LastAccessedDate { get; set; }

        public RecentFileItem(string path, bool pinned, DateTime lastAccessedDate)
        {
            FilePath = path;
            Pinned = pinned;
            LastAccessedDate = lastAccessedDate;
        }

        public RecentFileItem() { }
        public RecentFileItem(XmlElement xmlElement)
        {
            loadFromXml(xmlElement);
        }


        //<file date="2017/09/12 11:30:00" path="d:\config1.path" pinned="yes" />
        public void loadFromXml(XmlElement xmlElement)
        {
            LastAccessedDate = xmlElement.GetAttributeOrElementDateTime("date", @"yyyy/MM/dd HH:mm:ss").Value;
            FilePath = xmlElement.GetAttributeOrElementText("path");
            Pinned = xmlElement.GetAttributeOrElementBool("pinned").Value;
        }

        public override string ToString()
        {
            return FilePath;
        }

        public string ToXmlString()
        {
            return $"<file date=\"{LastAccessedDate:yyyy/MM/dd HH:mm:ss}\" path=\"{FilePath}\" pinned=\"{(Pinned ? "yes" : "no")}\" />";
        }

        // public RecentPinItem PinItem { get; private set; }

        public RecentPinItem ToRecentPinItem()
        {

            return new RecentPinItem() { Caption = Path.GetFileName(FilePath), Description = FilePath, Tag = LastAccessedDate, PinButtonChecked = Pinned };
        }
    }


}
