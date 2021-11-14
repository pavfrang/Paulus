using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;

using System.IO;

namespace Paulus.IO
{
    public static class XmlExtensions
    {
        public const string XmlProlog  = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

        public static StreamWriter GetWriterAndWriteProlog(string path)
        {
            StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8);
            writer.WriteLine(XmlProlog);
            return writer;
        }

        public static XmlElement GetRootXmlElement(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            return doc.DocumentElement;
        }

        public static string GetAttributeOrElementText(this XmlElement e, string propertyOrAttributeName, string valueIfMissing="")
        {
            //if (e.Attributes[propertyOrAttributeName] != null)
            //return e.Attributes[propertyOrAttributeName].Value;
            if (e.HasAttribute(propertyOrAttributeName))
                return e.GetAttribute(propertyOrAttributeName);
            else if (e[propertyOrAttributeName] != null)
                return e[propertyOrAttributeName].InnerText;
            else
                return valueIfMissing;
        }

        public static double? GetAttributeOrElementDouble(this XmlElement e, string propertyOrAttributeName,double defaultValue=0)
        {
            return GetAttributeOrElementDouble(e, propertyOrAttributeName, CultureInfo.InvariantCulture,defaultValue);
        }

        public static double? GetAttributeOrElementDouble(this XmlElement e, string propertyOrAttributeName, IFormatProvider provider, double defaultValue=0)
        {
            string sDouble = GetAttributeOrElementText(e, propertyOrAttributeName);

            if (sDouble != null)
            {
                double value;
                bool parsed = double.TryParse(sDouble, NumberStyles.Float, provider, out value);
                return parsed ? value : defaultValue;
            }
            return defaultValue;
        }

        public static int GetChildNodePosition(this XmlNode parent, XmlNode child, int startIndex = 1)
        {
            int i = -1;
            for (i = 0; i < parent.ChildNodes.Count; i++)
                if (parent.ChildNodes[i] == child) break;

            return i != -1 ? i + startIndex : -1;
        }

        public static bool? GetAttributeOrElementBool(this XmlElement e, string propertyOrAttributeName)
        {
            string sBool = GetAttributeOrElementText(e, propertyOrAttributeName);
            if (sBool != null)
                switch (sBool.ToLower())
                {
                    case "false":
                    case "no":
                        return false;
                    case "true":
                    case "yes":
                        return true;
                }
            return null;
        }

        public static DateTime? GetAttributeOrElementDateTime(this XmlElement e, string propertyOrAttributeName, string format)
        {
            return GetAttributeOrElementDateTime(e, propertyOrAttributeName, new string[] { format }, CultureInfo.InvariantCulture);
        }

        public static DateTime? GetAttributeOrElementDateTime(this XmlElement e, string propertyOrAttributeName, string format, IFormatProvider provider)
        {
            return GetAttributeOrElementDateTime(e, propertyOrAttributeName, new string[] { format }, provider);
        }

        public static DateTime? GetAttributeOrElementDateTime(this XmlElement e, string propertyOrAttributeName, string[] formats)
        {
            return GetAttributeOrElementDateTime(e, propertyOrAttributeName, formats, CultureInfo.InvariantCulture);
        }

        public static DateTime? GetAttributeOrElementDateTime(this XmlElement e, string propertyOrAttributeName, string[] formats, IFormatProvider provider)
        {
            string sDate = XmlExtensions.GetAttributeOrElementText(e, propertyOrAttributeName);

            if (sDate != null)
            {
                DateTime value;
                bool parsed = DateTime.TryParseExact(sDate, formats, provider, DateTimeStyles.AssumeLocal, out value);
                if (parsed) return value;
            }

            return null;
        }

        public static T GetAttributeOrElementCustom<T>(this XmlElement e, string propertyOrAttributeName, Dictionary<string, T> dictionary, T invalidValue)
        {
            string sEnumValue = GetAttributeOrElementText(e, propertyOrAttributeName);
            if (sEnumValue != null && dictionary.ContainsKey(sEnumValue)) return dictionary[sEnumValue];
            return invalidValue;
        }

        public static T GetAttributeOrElementEnum<T>(this XmlElement e, string propertyOrAttributeName)
        {
            string sEnumValue = GetAttributeOrElementText(e, propertyOrAttributeName);
            return (T)Enum.Parse(typeof(T), sEnumValue);
        }


        //e.g. read <import type="settings" file="settings.xml"/>
        //or <settings>...</settings>
        public static XmlElement GetSettingsElementFromRootOrImport(this XmlElement root, string baseFilePath, string settingsName)
        {
            XmlElement settings = root[settingsName];
            if (settings != null) return settings;

            XmlNodeList l = root.GetElementsByTagName("import");

            foreach (XmlElement n in l)
            {
                string file = n.GetAttribute("file");

                if (n.GetAttribute("type") == settingsName)
                {
                    XmlDocument settingsXml = new XmlDocument();
                    file = PathExtensions.GetAbsolutePath2(baseFilePath, file, true);

                    StreamReader reader = new StreamReader(file, Encoding.UTF8);
                    settingsXml.Load(reader);

                    reader.Close();

                    return settingsXml[settingsName];
                }
            }

            return null;
        }

        //public static XmlDocument GetXmlDocument(this string xmlPath)
        //{
        //    XmlDocument d = new XmlDocument();
        //    d.Load(xmlPath);
        //    return d;
        //}
    }
}
