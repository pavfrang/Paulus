using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Paulus.Serial
{
    public abstract class Library
    {

        public Library(string name, string path)
        {
            Name = name;
            Path = path;
            readFromXmlFile(path);
        }

        public Library() { }

        public string Name { get; set; }

        public string Path { get; set; }

        protected abstract void readFromXml(XmlDocument document);

        protected void readFromXmlFile(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            readFromXml(doc);
        }


        public abstract DataTable GetDataTable();

    }

    public class Library<T> : Library, IEnumerable<T> where T : LibraryItem, new()
    {

        public Library(string name, string path) : base(name, path)
        { }

        public Library() { }

        #region Items
        public List<T> Items { get; protected set; }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)Items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)Items).GetEnumerator();
        }

        public T this[int i]
        {
            get { return Items[i]; }
        }
        public T this[string id]
        {
            get { return Items.FirstOrDefault(i => i.ID == id); }
        }

        #endregion

        protected override void readFromXml(XmlDocument doc)
        {
            string itemTag = (new T()).xmlElementTag;
            XmlNodeList nodes = doc.SelectNodes($"//{itemTag}");

            Items = new List<T>();
            foreach (XmlElement node in nodes)
                // Items.Add(new T(node));
                Items.Add((T)Activator.CreateInstance(typeof(T), node));
        }
        public override DataTable GetDataTable()
        {
            DataTable table = (new T()).GetDataTableSchema();

            foreach (T item in Items)
                table.LoadDataRow(item.GetDataRowValues(), true);

            return table;
        }
    }

}
