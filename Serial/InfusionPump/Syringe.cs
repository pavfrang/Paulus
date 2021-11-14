using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Paulus.IO;

namespace Paulus.Serial.InfusionPump
{
    public class Syringe : LibraryItem, IEquatable<Syringe>
    {
        /// <summary>
        /// Creates a Syringe by defining its properties.
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <param name="totalVolumeInMilliliters"></param>
        /// <param name="diameterInMillimeters"></param>
        public Syringe(string id, string manufacturer, float totalVolumeInMilliliters, float diameterInMillimeters)
        {
            ID = id;
            Manufacturer = manufacturer;
            TotalVolumeInMilliliters = totalVolumeInMilliliters;
            DiameterInMillimeters = diameterInMillimeters;
        }

        /// <summary>
        /// Creates a Syringe from an XML fragment.
        /// </summary>
        /// <param name="xmlSyringe"></param>
        public Syringe(XmlElement xmlSyringe) : base(xmlSyringe)
        { }

        public Syringe() { } //needed for generics use of Library<T> where T:LibraryItem

        #region XML
        //should be static but inheritance does not work for static objects
        protected internal override string xmlElementTag
        {
            get
            {
                return "syringe";
            }
        }


        protected override void readFromXml(XmlElement xmlSyringe)
        {
            //XML #1 (Verbose) (this is used in the syringes library)
            //volumeUnit: one of ml, ul, μl or missing (ml is implied)
            //diameterUnit: one of mm, cm or missing (mm is implied)
            //<syringe brand="Hamilton" volume="1" volumeUnit="ml" diameter="4.61" diameterUnit="mm"/>
            //XML #2 (Simplified) (volumeUnit = "ml" and diameterUnit = "mm" are implied)
            //<syringe brand="Hamilton" volume="1" diameter="4.61"/>

            try
            {
                ID = xmlSyringe.Attributes["id"].Value;

                Manufacturer = xmlSyringe.Attributes["brand"].Value;

                //read volume
                string sVolume = xmlSyringe.Attributes["volume"].Value;
                //volume unit is optional (ml is implied)
                bool hasVolumeUnit = xmlSyringe.HasAttribute("volumeUnit"); //only ml,ul,μl (or none) are allowed
                string volumeUnit = xmlSyringe.GetAttributeOrElementText("volumeUnit", "ml");

                if (volumeUnit == "ml") //ml
                    TotalVolumeInMilliliters = float.Parse(sVolume);
                else if (volumeUnit == "ul" || volumeUnit == "μl")//μl
                    TotalVolumeInMilliliters = float.Parse(sVolume, en) / 1000.0f;
                else
                    throw new InvalidUnitException("volume", volumeUnit, "ml", "ul", "μl");

                //read diameter
                string sDiameter = xmlSyringe.Attributes["diameter"].Value;
                //diameter unit (only cm, mm (or none) are allowed)
                string diameterUnit = xmlSyringe.GetAttributeOrElementText("diameterUnit", "mm");
                if (diameterUnit == "mm")
                    DiameterInMillimeters = float.Parse(sDiameter, en);
                else if (diameterUnit == "cm")
                    DiameterInMillimeters = float.Parse(sDiameter, en) * 10.0f;
                else
                    throw new InvalidUnitException("diameter", diameterUnit, "mm", "cm");
            }
            catch (System.Exception exception)
            {
                throw new XmlException($"Cannot parse {xmlSyringe.OuterXml}.", exception);
            }
        }

        #endregion

        #region Properties

        public string Manufacturer { get; private set; }
        public float TotalVolumeInMilliliters { get; private set; }

        public float DiameterInMillimeters { get; private set; }



        #endregion

        public override string ToString() =>
            $"{Manufacturer} ({TotalVolumeInMilliliters} ml)";


        #region Datatable
        public override DataTable GetDataTableSchema()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Syringe", typeof(Syringe)); //invisible
            table.Columns.Add("ID", typeof(string)); //invisible
            table.Columns.Add("Manufacturer", typeof(string));
            table.Columns.Add("Volume [ml]", typeof(float));
            table.Columns.Add("Diameter [mm]", typeof(float));
            return table;
        }

        public override object[] GetDataRowValues()
        {
            return new object[] {this, //invisible
                ID, //invisible
                Manufacturer,
                TotalVolumeInMilliliters,
                DiameterInMillimeters };
        }

        #endregion


        #region Syringe Equality

        public static bool operator ==(Syringe left, Syringe right)
        {
            //both nulls
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;

            //one null
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            //non-nulls
            return left.ID == right.ID;
            //return left.TotalVolumeInMl == right.TotalVolumeInMl &&
            //       left.DiameterInMm == right.DiameterInMm &&
            //       string.Compare(left.Manufacturer, right.Manufacturer, true) == 0;
        }

        public static bool operator !=(Syringe left, Syringe right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
            // TotalVolumeInMl.GetHashCode() ^ DiameterInMm.GetHashCode() ^ Manufacturer.GetHashCode();
        }

        public bool Equals(Syringe other)
        {
            return this == other;
        }


        public override bool Equals(object obj)
        {
            if (!(obj is Syringe)) return false;
            return this == (Syringe)obj;
        }


        #endregion





    }
}
