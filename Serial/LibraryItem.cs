using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Paulus.Serial
{
    public abstract class LibraryItem
    {
        public LibraryItem(XmlElement xmlItem)
        {
            readFromXml(xmlItem);
        }

        public LibraryItem() { }

        protected CultureInfo en = CultureInfo.GetCultureInfo("en-us");

        protected abstract void readFromXml(XmlElement xmlItem);

        protected internal abstract string xmlElementTag { get; }

        public abstract object[] GetDataRowValues();

        public abstract DataTable GetDataTableSchema();

        public string ID { get; protected set; } //name must be unique

    }



}
