using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Paulus.IO;

namespace Paulus.Serial.GasMixer
{
    public class Cylinder : LibraryItem, IComparable<Cylinder>, IComparable
    {
        private const string floatPattern = @"\d+(\.\d+)?";
        private static Regex regexConcentration =
           new Regex($@"(?<concentration>{floatPattern})(±(?<tolerance>{floatPattern}))?\s*(?<unit>ppm|%)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //static Cylinder()
        //{
        //    string c = @"5.8%/1.92%";
        //    MatchCollection mc = regexConcentration.Matches(c);
        //}

        public Cylinder() { }

        public Cylinder(List<CylinderComponent> components, float initialSizeInLiters, string cylinderNumber, string cylinderCode, string balanceGasName)
        {
            // CylinderNumber = cylinderNumber;
            ID = cylinderNumber;
            CylinderCode = cylinderCode;
            //InitialPressureInBar = initialPressureInBar;
            InitialSizeInLiters = CurrentSizeInLiters = initialSizeInLiters;
            Components = components;
            BalanceGasName = balanceGasName;
        }
        public Cylinder(XmlElement xmlCylinder) : base(xmlCylinder) { }


        public static Cylinder GetN2Cylinder()
        {
            //in the end add the N2 cylinder manually
            Cylinder N2Cylinder = new Cylinder(
                new List<CylinderComponent> { new CylinderComponent("N2", 100.0f, 0.0f, ConcentrationUnit.PerCent) }, 50.0f, "N2", "N2", "");

            N2Cylinder.CalculateGasMixture();
            return N2Cylinder;
        }

        //The AirO2Cylinder is preferred.
        public static Cylinder GetAirCylinder()
        {
            //in the end add the N2 cylinder manually
            Cylinder airCylinder = new Cylinder(
                new List<CylinderComponent> { new CylinderComponent("Air", 100.0f, 0.0f, ConcentrationUnit.PerCent) }, 50.0f, "AIR", "AIR", "");

            airCylinder.CalculateGasMixture();
            return airCylinder;
        }

        /// <summary>
        /// Retrieves a 21.5% O2 cylinder.
        /// </summary>
        /// <returns></returns>
        public static Cylinder GetAirO2Cylinder()
        {
            Cylinder airCylinder = new Cylinder(
                new List<CylinderComponent>
                {
                    new CylinderComponent("O2",21.5f,0.0f,ConcentrationUnit.PerCent)
                }, 50.0f, "AIR", "AIR", "N2");
            airCylinder.CalculateGasMixture();
            return airCylinder;
        }

        /// <summary>
        /// The Empty cylinder is a special case of an AIR cylinder, which is wrapped in order to appear as NONE inside the program.
        /// </summary>
        /// <returns></returns>
        public static Cylinder GetEmptyCylinder()
        {
            Cylinder airCylinder = GetAirCylinder();
            airCylinder.Name = "<NONE>";
            airCylinder.ID = "NONE";
            airCylinder.CylinderCode = ""; //remove the AIR cylinder code
            return airCylinder;
        }

        public bool IsEmptyCylinder() { return Name == "<NONE>"; }

        #region XML

        protected internal override string xmlElementTag
        {
            get
            {
                return "cylinder";
            }
        }

        protected override void readFromXml(XmlElement xmlItem)
        {
            //examples:
            //<cylinder name="C3H6/CH4" concentration="5078ppm/1666ppm" balance="N2" size="50" id="355786" code="E1F8H" />
            //<cylinder name="CO" concentration="0.0095" balance="N2" size="20" id="D721879" code="EX5WY" />
            //<cylinder name="CO/H2" concentration="5.99%/1.96%" balance="N2" size="50" id="125358" code="EX5Y7" />
            //<cylinder name="SO2" concentration="800.7±8 ppm" balance="N2" size="50" id="627171" code="" />
            //<cylinder name="C3H8" concentration="1990 ppm" balance="N2" size="50" id="11260487" code="" />

            try
            {
                //check the id first
                string sNumber = xmlItem.Attributes["id"].Value;

                //check for default N2
                if (sNumber == "N2")
                {
                    //fill the data for the default N2 cylinder
                    Components = new List<CylinderComponent> { new CylinderComponent("N2", 100.0f, 0.0f, ConcentrationUnit.PerCent) };
                    CurrentSizeInLiters = InitialSizeInLiters = 50.0f;
                    ID = CylinderCode = "N2";
                    CalculateGasMixture();
                    return;
                }
                //return the default AIR cylinder
                else if (sNumber == "AIR")
                {
                    Components = new List<CylinderComponent> { new CylinderComponent("Air", 100.0f, 0.0f, ConcentrationUnit.PerCent) };
                    CurrentSizeInLiters = InitialSizeInLiters = 50.0f;
                    ID = CylinderCode = "AIR";
                    CalculateGasMixture();
                    return;
                }
                //return the default EMPTY cylinder
                else if (sNumber =="EMPTY" || sNumber=="NONE" || sNumber=="")
                {
                    Components = new List<CylinderComponent> { new CylinderComponent("Air", 100.0f, 0.0f, ConcentrationUnit.PerCent) };
                    CurrentSizeInLiters = InitialSizeInLiters = 50.0f;
                    CalculateGasMixture();
                    Name = "<NONE>";
                    ID = "NONE";
                    //CylinderCode = "";
                    return;
                }

                string sGases = xmlItem.Attributes["name"].Value;
                string[] gasNames = sGases.Replace("Ν", "N").Replace("Ο", "O").Split('/').Select(s => s.Trim()).ToArray();
                int gasesCount = gasNames.Length;

                string sConcentration = xmlItem.Attributes["concentration"].Value;
                //N2 is the default balance if it is missing
                string sBalance = xmlItem.GetAttributeOrElementText("balance", "N2");
                //xmlItem.Attributes["balance"].Value;
                float size = (float)xmlItem.GetAttributeOrElementDouble("size", 50.0); //xmlItem.Attributes["size"].Value;
                string sCode = xmlItem.Attributes["code"].Value;

                //975 ppm,3010ppm,5078ppm/1666ppm,911±9.1 ppm,901.1±9 ppm
                //0.95%,100%,5.8%/1.92%,3.005±0.03%,10.00%.100.00%
                //string[] tests = new string[] {
                //    "975 ppm" ,"3010ppm","3010.2ppm","911±9.1 ppm" ,"901.1±9 ppm", "5078ppm/1666ppm",
                //    "0.95%","100%","5.8%/1.92%","3.005±0.03%","10.00%","100.00%"};

                List<CylinderComponent> components = new List<CylinderComponent>();

                float percentConcentration;
                bool parsed = float.TryParse(sConcentration, out percentConcentration); //if it is parsed then it is a single gas and percent!
                if (parsed && gasesCount == 1)
                    components.Add(new CylinderComponent(sGases, percentConcentration * 100.0f, 0.0f, ConcentrationUnit.PerCent));
                else
                {
                    MatchCollection mc = regexConcentration.Matches(sConcentration);
                    if (mc.Count != gasesCount) throw new InvalidDataException($"Invalid concentration information for Cylinder #{sNumber} (Gases: {sGases}, Concentrations: {sConcentration}).");
                    //components do not include the balance gas which is assumed nitrogen in all cases
                    for (int iGas = 0; iGas < gasesCount; iGas++)
                    {
                        Match m = mc[iGas];
                        float concentration = float.Parse(m.Groups["concentration"].Value);
                        float tolerance = string.IsNullOrEmpty(m.Groups["tolerance"].Value) ? 0.0f : float.Parse(m.Groups["tolerance"].Value);
                        ConcentrationUnit unit = m.Groups["unit"].Value != "%" ? ConcentrationUnit.PPM : ConcentrationUnit.PerCent;
                        components.Add(new CylinderComponent(gasNames[iGas], concentration, tolerance, unit));
                    }
                }

                //Cylinder newCylinder = new Cylinder(components, float.Parse(sSize), sNumber, sCode, sBalance);
                //newCylinder.CalculateGasMixture();

                ID = sNumber; // CylinderNumber = sNumber;
                CylinderCode = sCode;
                //InitialPressureInBar = initialPressureInBar;
                InitialSizeInLiters = CurrentSizeInLiters = size;
                Components = components;
                BalanceGasName = sBalance;

                CalculateGasMixture();
            }
            catch (System.Exception exception)
            {
                throw new XmlException($"Cannot parse {xmlItem.OuterXml}.", exception);
            }

        }

        #endregion

        #region Properties
        public List<CylinderComponent> Components { get; set; } //C3H6 or C3H6/CH4

        public ConcentrationUnit GetDefaultConcentrationUnit() =>
            Components.Count > 0 ? Components[0].ConcentrationUnit : ConcentrationUnit.PerCent;

        public GasMixture GasMixture { get; set; }

        public float InitialSizeInLiters { get; set; }

        public float InitialPressureInBar { get; set; }

        public string CylinderCode { get; set; }

        // public string CylinderNumber { get; set; } //This is the ID

        public float CurrentSizeInLiters { get; set; }

        //public string Comment { get; set; }

        public string BalanceGasName { get; set; }

        public float MainComponentConcentrationInPpm
        {
            get
            {
                return GasMixture.NonBalanceComponents.Count > 0 ?
                        GasMixture.NonBalanceComponents[0].Concentration : 1e6f;
            }
        }
        #endregion

        #region Calculations
        public void CalculateGasMixture()
        {
            bool hasBalance = Components.Count > 0 && Components.Sum(c => c.ToGasComponent().Concentration) < 1000000.0f;
            if (!hasBalance) { GasMixture = GasMixture.CreateGasMixtureWithBalanceOnly(Components[0].GasName); return; }

            //else the balance must be provided
            if (string.IsNullOrWhiteSpace(BalanceGasName)) BalanceGasName = "N2";
            GasMixture = new GasMixture(ToString(), SimpleGas.CommonGases[BalanceGasName], Components.Select(c => c.ToGasComponent()));
        }

        //if there is a Name defined then ToString shows the name instead of the components 
        public string Name;

        public string GetComponentsString() =>
            string.Join("/", Components.Select(c => c.GasName));

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name)) return Name;

            string sComponents = string.Join(", ", Components.Select(c => c.ToString()));

            //if (!string.IsNullOrWhiteSpace(ID) && ID != "Air" && ID != "N2")
            return $"{sComponents} (#{ID})";
            //else
            //    return sComponents;
        }

        /// <summary>
        /// Returns a string with maximum length of 35 characters (this can be safely sent to the gas mixer).
        /// </summary>
        /// <returns></returns>
        public string GetSafeString()
        {
            string safeString;
            string sComponents = string.Join(",", Components.Select(c => c.ToString()));

            //we assume that an ID must always exist
            //if (!string.IsNullOrWhiteSpace(ID) && ID != "Air" && ID != "N2")
            safeString = $"{sComponents} #{ID}";
            if (safeString.Length > 35)
                safeString = sComponents.Substring(0, 35 - ID.Length - 2);

            return safeString;
        }
        #endregion

        #region Comparison (needed for sort operations)

        public int CompareTo(Cylinder other)
        {
            if (GetComponentsString() != other.GetComponentsString())
                return GetComponentsString().CompareTo(other.GetComponentsString());

            else
                return Components[0].Concentration.CompareTo(other.Components[0].Concentration);
        }

        public int CompareTo(object obj)
            => CompareTo((Cylinder)obj);
        #endregion

        #region Datatable
        public override DataTable GetDataTableSchema()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Cylinder", typeof(Cylinder));
            table.Columns.Add("Gases", typeof(string));
            table.Columns.Add("Concentrations", typeof(string));
            table.Columns.Add("Balance", typeof(string));
            table.Columns.Add("Cylinder Number", typeof(string));
            table.Columns.Add("Cylinder Code", typeof(string));
            return table;
        }

        public override object[] GetDataRowValues()
        {
            return new object[] {
                this, //Cylinder
                string.Join("/", Components.Select(c => c.GasName)), //Gases
                string.Join("/", Components.Select(c => c.GetConcentrationString())), //Concentrations
                Components[0].GetConcentrationString() != "100%" ? BalanceGasName : "", //Balance
                ID, //Cylinder Number
                CylinderCode };

        }

        #endregion
    }

}
