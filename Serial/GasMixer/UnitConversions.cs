using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Paulus.Serial.GasMixer
{
    public enum ConcentrationUnit
    {
        [Display(Name = "-")]
        None,
        [Display(Name = "%")]
        PerCent,
        [Display(Name = "ppm")]
        PPM
    }

    public enum FlowUnit
    {
        [Display(Name = "-")]
        None,
        [Display(Name = "l/min")]
        LPM,
        [Display(Name = "cm³/min")]
        CCM
    }

    public static class UnitExtensions
    {
        public static string GetLabelWithPrefixedSpace(this ConcentrationUnit unit) =>
            unit == ConcentrationUnit.PPM ? " ppm" : "%";
    }

    public static class UnitConversions
    {

        public static float ToPerCent(float ppm) => ppm / 10000.0f;
        public static float ToPpm(float percent) => percent * 10000.0f;

        public static float ToLpm(float ccm) => ccm / 1000.0f;
        public static float ToCcm(float lpm) => lpm * 1000.0f;


        public static float GetPpm(float concentration, string unit) =>
            unit == "%" ? concentration * 10000.0f : concentration;
        public static float GetPpmOrPercent(float ppm, string unit) =>
            unit == "%" ? ppm / 10000.0f : ppm;



    }
}
