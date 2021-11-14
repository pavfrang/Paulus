using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Paulus.IO;
using System.ComponentModel;

namespace Paulus.Serial.InfusionPump
{
    public enum Status
    {
        None,
        Infusing,
        Withdrawing,
        Stopped,
        [Description("Not Applicable")]
        NotApplicable,
        Error
    }


    public class InfusionPumpSettings : DeviceSettingsBase, IEquatable<InfusionPumpSettings>
    {
        //public InfusionPumpSettings(string path) : base(path)
        //{
        //    DataTables = new InfusionPumpSettingsDataTables(this);
        //}

        //public InfusionPumpSettings(XmlNode node) : base(node)
        //{
        //    DataTables = new InfusionPumpSettingsDataTables(this);
        //}

        /// <summary>
        /// The deviceManager should be supplied as a parameter in all cases because the syringes/liquids library is used.
        /// </summary>
        /// <param name="deviceManager"></param>
        public InfusionPumpSettings(DeviceManager deviceManager) :
            base(deviceManager)
        {
            LoadDefaults(deviceManager);
        }

        public InfusionPumpSettings(DeviceManager deviceManager, string path) :
            base(deviceManager, path)
        {
            LoadDefaults(deviceManager);
        }

        public InfusionPumpSettings(DeviceManager deviceManager, XmlElement xmlDevice) :
            base(deviceManager, xmlDevice)
        { }

        public InfusionPumpSettings()
        { }

        #region XML

        /// <summary>
        /// There must be at least one
        /// </summary>
        /// <param name="deviceManager"></param>
        public void LoadDefaults(DeviceManager deviceManager)
        {
            //get first syringe library in the device manager
            SyringeLibrary = deviceManager.Libraries.Values.OfType<Library<Syringe>>().First();
            //SyringeLibrary = deviceManager.Libraries["Syringes #1"] as Library<Syringe>;
            Syringe = SyringeLibrary["H10"];

            LiquidLibrary = deviceManager.Libraries.Values.OfType<Library<SyringeLiquid>>().First();
            //LiquidLibrary = deviceManager.Libraries["Syringe Liquids #1"] as Library<SyringeLiquid>;
            Liquid = LiquidLibrary["H2O"];

            InfusionRate = 40.0f;
            InfusionRateUnit = "ul/min";

            RemainingTimeThreshold = 5.0f;
            RemainingTimeThresholdUnit = "min";

            RemainingVolumeThresholdPercentage = 10;

            InitialVolume = 15.0f;
            InitialVolumeUnit = "ml";
            TargetVolume = 4.0f;
            TargetVolumeUnit = "ml";

            SelectedGasMixerNames = new List<string>();

        }

        public override void LoadFromXml(XmlElement xmlInfusionPump)
        {
            base.LoadFromXml(xmlInfusionPump);

            //DeviceName = xmlInfusionPump.Attributes["name"].Value;

            //example:
            //<libraries>
            //  <library type="cylinders" file ="cylinders.xml" name ="Cylinders #1" />
            //  <library type="syringes" file ="syringes.xml" name ="Syringes #1" />
            //  <library type="syringe_liquids" file ="syringe_liquids.xml" name ="Syringe Liquids #1" />
            //</libraries>

            //<device type="infusion pump" name ="Infusion Pump #1" syringes ="Syringes #1" liquids ="Syringe Liquids #1" guid="{BC0F3D58-22E4-4639-AFCF-0FBCB6834215}">
            //  <syringe id="H25" />
            //  <liquid name="C10H22" />
            //  <infusion rate="50" rate_unit="ul/min" />
            //  <thresholds volume_percentage ="10" time="5" time_unit="min"/>
            // <gasmixer>Gas Mixer #1</gasmixer>
            // <gasmixer>Gas Mixer #2</gasmixer>
            // <volume initial="10" initial_unit="ml" target="2" target_unit="ml" />
            //</device>

            try
            {
                string sLiquidsLibrary = xmlInfusionPump.Attributes["liquids"].Value;
                string sLiquid = xmlInfusionPump["liquid"].Attributes["name"].Value;
                LiquidLibrary = DeviceManager.Libraries[sLiquidsLibrary] as Library<SyringeLiquid>;
                Liquid = LiquidLibrary[sLiquid];

                string sSyringesLibrary = xmlInfusionPump.Attributes["syringes"].Value;
                string sSyringe = xmlInfusionPump["syringe"].Attributes["id"].Value;
                SyringeLibrary = DeviceManager.Libraries[sSyringesLibrary] as Library<Syringe>;
                Syringe = SyringeLibrary[sSyringe];

                XmlElement xmlRate = xmlInfusionPump["infusion"];
                InfusionRate = float.Parse(xmlRate.Attributes["rate"].Value, en);
                InfusionRateUnit = xmlRate.Attributes["rate_unit"].Value; //ul/min (ul/m),ul/h,ml/min (ml/m),ml/h

                XmlElement xmlThresholds = xmlInfusionPump["thresholds"];
                RemainingTimeThresholdUnit = xmlThresholds.GetAttributeOrElementText("time_unit", "min"); //one of min/m,s,h
                RemainingTimeThreshold = float.Parse(xmlThresholds.Attributes["time"].Value, en);

                RemainingVolumeThresholdPercentage = int.Parse(xmlThresholds.Attributes["volume_percentage"].Value);

                XmlElement xmlVolume = xmlInfusionPump["volume"];
                InitialVolume = float.Parse(xmlVolume.Attributes["initial"].Value, en);
                InitialVolumeUnit = xmlVolume.Attributes["initial_unit"].Value;
                TargetVolume = float.Parse(xmlVolume.Attributes["target"].Value, en);
                TargetVolumeUnit = xmlVolume.Attributes["target_unit"].Value;

                SelectedGasMixerNames = new List<string>();
                XmlNodeList xmlGasMixers = xmlInfusionPump.SelectNodes("./gasmixer");
                if (xmlGasMixers.Count > 0)
                    SelectedGasMixerNames.AddRange(
                        xmlGasMixers.Cast<XmlNode>().Select(n => ((XmlElement)n).InnerText));
            }
            catch (System.Exception exception)
            {
                throw new XmlException($"Cannot parse {xmlInfusionPump.OuterXml}.", exception);
            }

        }

        public override string ToXmlString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<device type=\"infusion pump\" name =\"{DeviceName}\" syringes =\"{SyringeLibrary.Name}\" liquids =\"{LiquidLibrary.Name}\" guid=\"{{{DeviceId}}}\" >");

            sb.AppendLine($"<syringe id=\"{Syringe.ID}\" />");
            sb.AppendLine($"<liquid name=\"{Liquid.ID}\" />");
            sb.AppendLine($"<infusion rate=\"{InfusionRate}\" rate_unit=\"{InfusionRateUnit}\" />");
            sb.AppendLine($"<thresholds volume_percentage=\"{RemainingVolumeThresholdPercentage}\" time=\"{RemainingTimeThreshold}\" time_unit=\"{RemainingTimeThresholdUnit}\" />");

            sb.AppendLine($"<volume initial=\"{InitialVolume}\" initial_unit=\"{InitialVolumeUnit}\" target=\"{TargetVolume}\" target_unit=\"{TargetVolumeUnit}\" />");

            if (SelectedGasMixerNames.Count > 0)
                foreach (string gasMixerName in SelectedGasMixerNames)
                    sb.AppendLine($"<gasmixer>{gasMixerName}</gasmixer>");

            sb.AppendLine($"</device>");
            return sb.ToString();
        }
        #endregion

        #region Properties

        public string Version { get; set; }

        public Library<Syringe> SyringeLibrary { get; set; }
        public Library<SyringeLiquid> LiquidLibrary { get; set; }

        public List<string> SelectedGasMixerNames { get; set; }


        public SyringeLiquid Liquid { get; set; }

        public Syringe Syringe { get; set; }

        public float InfusionRate { get; set; }

        public string InfusionRateUnit { get; set; }


        public float GetInfusionRateInMicrolitersPerMinute()
        {
            switch (InfusionRateUnit)
            {
                case "ul/m":
                case "ul/min":
                case "μl/m":
                case "μl/min":
                    return InfusionRate;
                case "ml/m":
                case "ml/min":
                    return InfusionRate * 1000.0f;
                case "ul/h":
                case "μl/h":
                    return InfusionRate / 60.0f;
                case "ml/h":
                    return InfusionRate * 1000.0f / 60.0f;
                default:
                    throw new InvalidUnitException("infusion rate", InfusionRateUnit, "ul/m", "ul/min", "μl/m", "μl/min", "ml/m", "ml/min", "ul/h", "μl/h", "ml/h");
            }

        }

        public string GetFriendlyInfusionRateUnit()
        {
            switch (InfusionRateUnit)
            {
                case "μl/min":
                case "μl/m":
                case "ul/min":
                    return "ul/m";
                case "ml/min":
                    return "ml/m";
                case "μl/h":
                    return "ul/h";
                default:
                    return InfusionRateUnit;
            }
        }

        public float InitialVolume { get; set; }
        public string InitialVolumeUnit { get; set; }//ml, ul
        public float GetInitialVolumeInMicroLiters()
        {
            if (InitialVolumeUnit == "ul" | InitialVolumeUnit == "μl") return InitialVolume;
            else if (InitialVolumeUnit == "ml") return InitialVolume * 1000;
            else
                throw new InvalidUnitException("initial volume", InitialVolumeUnit, "ml,ul,μl");// InvalidUnit
        }

        public float TargetVolume { get; set; }
        public string TargetVolumeUnit { get; set; } //ml, ul

        public float GetTargetVolumeInMicroLiters()
        {
            if (TargetVolumeUnit == "ul" | TargetVolumeUnit == "μl") return TargetVolume;
            else if (TargetVolumeUnit == "ml") return TargetVolume * 1000;
            else
                throw new InvalidUnitException("target volume", TargetVolumeUnit, "ml,ul,μl");// InvalidUnit
        }


        public float DeliveredVolume { get; set; }
        public string DeliveredVolumeUnit { get; set; }
        public float GetDeliveredVolumeInMicroLiters()
        {
            if (DeliveredVolumeUnit == null) DeliveredVolumeUnit = TargetVolumeUnit;

            if (DeliveredVolumeUnit == "ul" || DeliveredVolumeUnit == "μl") return DeliveredVolume;
            else if (DeliveredVolumeUnit == "ml") return DeliveredVolume * 1000;
            else
                throw new InvalidUnitException("delivered volume", DeliveredVolumeUnit, "ml,ul,μl");// InvalidUnit
        }

        public float TotalDeliveredVolume { get; set; }

        public float RemainingVolume { get; set; }
        public string RemainingVolumeUnit { get; set; }

        public float GetConcentrationInPpm(float infusionRateInMicrolitersPerMinute, float flowInLitersPerMinute)
        {
            float density = Liquid.GetDensityInGramsPerMilliliter();
            return infusionRateInMicrolitersPerMinute * 22.4f * density / 1000.0f / Liquid.MolecularWeight * 1e6f * Liquid.Carbons / flowInLitersPerMinute;
        }

        public float GetConcentrationInPpm(float flowInLitersPerMinute)
            => GetConcentrationInPpm(GetInfusionRateInMicrolitersPerMinute(), flowInLitersPerMinute);

        public int RemainingVolumeThresholdPercentage { get; set; }

        public float RemainingTimeThreshold { get; set; }
        public string RemainingTimeThresholdUnit { get; set; }
        public float RemainingVolumeFromInitial { get; set; }

        public float GetRemainingTimeThresholdInMinutes()
        {
            if (RemainingTimeThresholdUnit == "min" || RemainingTimeThresholdUnit == "m") return RemainingTimeThreshold;
            if (RemainingTimeThresholdUnit == "s") return RemainingTimeThreshold / 60.0f;
            if (RemainingTimeThresholdUnit == "h") return RemainingTimeThreshold * 60.0f;

            throw new InvalidUnitException("remaining time", RemainingTimeThresholdUnit, "m", "min", "s", "h");
        }
        public Status? Status { get; set; }
        public InfusionPumpException LastException { get; internal set; }

        #endregion

        public override object Clone()
        {
            return MemberwiseClone();
        }

        #region InfusionPumpSettings Equality

        public static bool operator ==(InfusionPumpSettings left, InfusionPumpSettings right)
        {
            //both nulls
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;

            //one null
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            //non-nulls
            return left.DeviceName == right.DeviceName &&
                   left.InfusionRate == right.InfusionRate &&
                   left.InfusionRateUnit == right.InfusionRateUnit &&
                   left.InitialVolume == right.InitialVolume &&
                   left.InitialVolumeUnit == right.InitialVolumeUnit &&
                   left.Liquid == right.Liquid &&
                   left.LiquidLibrary == right.LiquidLibrary &&
                   left.Syringe == right.Syringe &&
                   left.SyringeLibrary == right.SyringeLibrary &&
                   left.TargetVolume == right.TargetVolume &&
                   left.TargetVolumeUnit == right.TargetVolumeUnit;
        }

        public static bool operator !=(InfusionPumpSettings left, InfusionPumpSettings right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return DeviceName.GetHashCode() ^
                InfusionRate.GetHashCode() ^ InfusionRateUnit.GetHashCode() ^
                InitialVolume.GetHashCode() ^ InitialVolumeUnit.GetHashCode() ^
                Liquid.GetHashCode() ^ LiquidLibrary.GetHashCode() ^
                Syringe.GetHashCode() ^ SyringeLibrary.GetHashCode() ^
                TargetVolume.GetHashCode() ^ TargetVolumeUnit.GetHashCode();
        }

        public bool Equals(InfusionPumpSettings other)
        {
            return this == other;
        }


        public override bool Equals(object obj)
        {
            if (!(obj is InfusionPumpSettings)) return false;
            return this == (InfusionPumpSettings)obj;
        }


        #endregion

    }
}

