using Paulus.Common;


namespace Paulus.Forms.Charting
{
    //useful for separation of label and unit
    public class LabelUnit
    {
        public string Label;
        public string Unit;

        public bool HasUnit { get { return !string.IsNullOrWhiteSpace(Unit) && Unit != "-"; } }

        public static LabelUnit Parse(string text)
        {
            LabelUnit lu = new LabelUnit();
            lu.Unit = ChartExtensions.GetUnitFromLabel(text).Trim();
            if (lu.Unit.Length > 0)
                lu.Label = text.Substring2(0, text.LastIndexOf('[') - 1).Trim();
            else
                lu.Label = text.Trim();

            return lu;
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Unit) ? 
                Label + " [-]" : 
                Label + " [" + Unit +  "]";
        }
    }
}
