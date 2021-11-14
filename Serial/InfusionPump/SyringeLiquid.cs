using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Paulus.IO;

namespace Paulus.Serial.InfusionPump
{
    public class SyringeLiquid : LibraryItem
    {
        //Needed for generics use of Library<T> where T:LibraryItem
        public SyringeLiquid() { }

        /// <summary>
        /// Creates a SyringeLiquid from an XML fragment.
        /// </summary>
        /// <param name="xmlSyringe"></param>
        public SyringeLiquid(XmlElement xmlSyringeLiquid) : base(xmlSyringeLiquid)
        { }

        public SyringeLiquid(string name,
            int carbons, float density, string densityUnit, float molecularWeight)
        {
            ID = name;

            Carbons = carbons;
            Density = density;
            DensityUnit = densityUnit;
            MolecularWeight = molecularWeight;
        }

        #region XML
        protected internal override string xmlElementTag
        {
            get
            {
                return "liquid";
            }
        }

        protected override void readFromXml(XmlElement xmlLiquid)
        {
            // <liquid name="C10H22" carbons="10" density="0.73" density_unit="g/ml" molecular_weight="142.29" />
            try
            {
                ID = xmlLiquid.GetAttributeOrElementText("name");
                Carbons = int.Parse(xmlLiquid.GetAttributeOrElementText("carbons"));
                Density = float.Parse(xmlLiquid.GetAttributeOrElementText("density"), en);
                DensityUnit = xmlLiquid.GetAttributeOrElementText("density_unit", "g/ml");
                MolecularWeight = float.Parse(xmlLiquid.GetAttributeOrElementText("molecular_weight"), en);
            }
            catch (System.Exception exception)
            {
                throw new XmlException($"Cannot parse {xmlLiquid.OuterXml}.", exception);
            }
        }
        #endregion

        #region Properties

        public int Carbons { get; private set; }

        public float Density { get; private set; }

        /// <summary>
        /// Density unit is in g/ml or mg/cm3.
        /// </summary>
        public string DensityUnit { get; private set; }


        public float GetDensityInGramsPerMilliliter()
        {
            switch (DensityUnit)
            {
                case "g/ml":
                    return Density;
                case "mg/cm3":
                case "mg/cm³":
                    return Density * 0.001f;
                default:
                    throw new InvalidUnitException("liquid density", DensityUnit, "g/ml", "mg/cm3", "mg/cm³");
            }

        }

        public float MolecularWeight { get; private set; }

        #endregion



        public override string ToString() => ID;


        #region Datatable
        public override DataTable GetDataTableSchema()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Liquid", typeof(SyringeLiquid));
           // table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Molecular Weight", typeof(float));
            table.Columns.Add("Density", typeof(float));
            return table;
        }

        public override object[] GetDataRowValues()
        {
            return new object[]
            {
                this,
            //    ID, //Name
                MolecularWeight,
                Density
            };

        }
        #endregion


        #region SyringeLiquid Equality

        public static bool operator ==(SyringeLiquid left, SyringeLiquid right)
        {
            //both nulls
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;

            //one null
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            //non-nulls
            return left.ID == right.ID;
        }

        public static bool operator !=(SyringeLiquid left, SyringeLiquid right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(SyringeLiquid other)
        {
            return this == other;
        }


        public override bool Equals(object obj)
        {
            if (!(obj is SyringeLiquid)) return false;
            return this == (SyringeLiquid)obj;
        }


        #endregion


    }
}
