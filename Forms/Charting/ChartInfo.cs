using System.Collections.Generic;
using System.Linq;
using System.Globalization;

using Paulus.Collections;
using Paulus.Common;

namespace Paulus.Forms.Charting
{
    public class ChartInfo //used when loading an experiment project
    {
        public string Title;
        public string[] SeriesNames;
        public double MaxX, MaxY, MinX, MinY;
        public bool IsAutoMaxX, IsAutoMaxY, IsAutoMinX, IsAutoMinY;

        public static ChartInfo Parse(string line)
        {
            string[] tokens = line.Split(',');

            ChartInfo ch = new ChartInfo();

            #region Read scale
            //the last 4 tokens are always xMin,xMax,yMin,yMax
            string[] limits = tokens.Slice(tokens.Length - 4);
            if (limits[0] == "auto") { ch.MinX = double.NaN; ch.IsAutoMinX = true; }
            else ch.MinX = double.Parse(limits[0], CultureInfo.InvariantCulture);
            if (limits[1] == "auto") { ch.MaxX = double.NaN; ch.IsAutoMaxX = true; }
            else ch.MaxX = double.Parse(limits[1], CultureInfo.InvariantCulture);
            if (limits[2] == "auto") { ch.MinY = double.NaN; ch.IsAutoMinY = true; }
            else ch.MinY = double.Parse(limits[2], CultureInfo.InvariantCulture);
            if (limits[3] == "auto") { ch.MaxY = double.NaN; ch.IsAutoMaxY = true; }
            else ch.MaxY = double.Parse(limits[3], CultureInfo.InvariantCulture);
            #endregion

            #region Get series and labels

            string[] labels = tokens.Slice(tokens.Length - 5, false);

            //3 different configurations

            bool hasTitle = labels[0].StartsWith("'") && labels[0].EndsWith("'");
            if (hasTitle) ch.Title = labels[0].Substring2(0, '\'', '\'');
            else //assume title if possible
            {
                string candidateTitle = labels[0].SubstringBeforeChar(0, ' ');
                //return the candidate title if all series names/collections begin with this value
                //eg NH3 will return NH3
                //eg NO in, NO out, NO out exp will return NO
                bool foundMismatch = false;
                for (int i = 1; i < labels.Length; i++)
                    if (!labels[i].StartsWith(candidateTitle + " ")) { foundMismatch = true; break; }
                if (!foundMismatch) ch.Title = candidateTitle;
            }

            //'NOx',NO,NO2,120,auto,0,1.3e-4   or 'NOx',NO in,NO2 out exp,120,auto,0,1.3e-4

            int iStart = hasTitle ? 1 : 0;
            List<string> series = new List<string>();

            for (int i = iStart; i < labels.Length; i++)
            {
                bool isNameSimpleSeries = labels[i].EndsWith(" in") || labels[i].EndsWith(" out exp") || labels[i].EndsWith(" out sim");
                if (isNameSimpleSeries || labels[i].Contains(' '))
                    series.AddIfUniqueAndNotNull(labels[i]);
                else
                { //only labels without spaces
                    string[] seriesFromName = new string[] { labels[i] + " in", labels[i] + " out wet", labels[i] + " out sim" };
                    series.AddRange(seriesFromName);
                }
            }
            ch.SeriesNames = series.ToArray();
            #endregion

            return ch;
        }
    }


}
