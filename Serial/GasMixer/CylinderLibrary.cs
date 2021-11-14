using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using System.Text.RegularExpressions;
using System.Diagnostics;

using System.Xml;
using System.Data;
using System.Windows.Forms;

namespace Paulus.Serial.GasMixer
{

    public class CylinderLibrary : Library<Cylinder>
    {

        public CylinderLibrary(string name, string filePath) : base(name, filePath)
        { }

        public CylinderLibrary() { }


        /// <summary>
        /// A default cylinder library is useful in cases where the cylinder library is global in all cases.
        /// </summary>
        public static CylinderLibrary Default { get; set; }

        protected override void readFromXml(XmlDocument doc)
        {
            base.readFromXml(doc);

            //this is a dummy cylinder in order to allow empty cylinders to be declared
            Items.Add(Cylinder.GetEmptyCylinder());

            Items.Add(Cylinder.GetN2Cylinder());
            //Items.Add(Cylinder.GetAirCylinder());
            Items.Add(Cylinder.GetAirO2Cylinder());
            sortCylinders();
        }

        private void sortCylinders()
        {
            Items = Items.
                OrderBy(cylinder => cylinder.GetComponentsString()).
                ThenBy(cylinder => cylinder.Components[0].Concentration).
                ThenBy(cylinder => cylinder.InitialSizeInLiters).ToList();
        }

        //public Cylinder this[string cylinderNumber]
        //{
        //    get { return Items?.FirstOrDefault(c => c.ID == cylinderNumber); }
        //}

        //public static string GetDefaultFilePath() =>
        //    Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "gases.xml");

    }
}
