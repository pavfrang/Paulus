using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.IO;

using System.Xml.Serialization;
using Paulus.Common;

namespace Paulus.IO
{
    public static class SerializeExtensions
    {
        //we assume that T has the Serializable attribute and its base class and its members too (properties and fields)!
        public static void SaveAs<T>(T obj, string path, bool overwrite = true)
        {
            using (FileStream fs = new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew))
            {
                SoapFormatter formatter = new SoapFormatter();
                formatter.Serialize(fs, obj);
            }
        }

        public static T Open<T>(string path)
        {
            T obj = default(T);
            using (FileStream fs = File.OpenRead(path))//File.Open(
            {
                SoapFormatter formatter = new SoapFormatter();
                obj = (T)formatter.Deserialize(fs);
            }
            return obj;
        }

        public static void SaveAsBinary<T>(T obj, string path, bool overwrite = true)
        {
            using (FileStream fs = new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, obj);
            }
        }

        /// <summary>
        /// Perform a deep Copy of the object (Serializable only).
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        public static T OpenBinary<T>(string path)
        {
            FileStream fs = File.OpenRead(path);  //File.Open(
            BinaryFormatter formatter = new BinaryFormatter();
            T obj = (T)formatter.Deserialize(fs);
            fs.Close();
            return obj;
        }

        public static bool IsSerializable(Type type)
        {
            return type.HasAttribute(typeof(SerializableAttribute)); //type.GetInterfaces().Contains(typeof(ISerializable)) || 
        }

        public static bool IsSerializable<T>()
        {
            Type type = typeof(T);
            return IsSerializable(type);
        }

        public static bool IsSerializable<T>(T obj)
        {
            return IsSerializable<T>();
        }


        public static void SaveAsXML<T>(T item, string path, bool overwrite = true)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (FileStream fs = new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                    serializer.Serialize(writer, item);
            }
        }

        public static T OpenXML<T>(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException();

            XmlSerializer deserializer = new XmlSerializer(typeof(T));
            T obj = default(T);
            using (StreamReader reader = new StreamReader(path))
                obj = (T)deserializer.Deserialize(reader);
            return obj;
        }
    }
}
