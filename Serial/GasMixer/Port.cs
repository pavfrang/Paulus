using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial.GasMixer
{
    public class Port
    {
        public Port(int id)
        {
            ID = id;
        }

        public Port(int id, Cylinder cylinder) : this(id)
        {
            Cylinder = cylinder;
        }

        public int ID { get; set; }

        public Cylinder Cylinder { get; set; }

        public float KFactor { get { return Cylinder?.GasMixture?.KFactor ?? 0.0f; } }



        /// <summary>
        /// Concentration of the main ingredient of the cylinder (except for the balance). In a multi-gas cylinder the KFactor is calculated based on all the gases concentrations.
        /// The value is used only for display purposes only.
        /// </summary>
        public float ConcentrationInPpm
        {
            get
            {
                return Cylinder?.MainComponentConcentrationInPpm ?? 0.0f;
            }
        }

        public override string ToString()
        {
            if (Cylinder == null)
                return $"Port: {ID}, Cylinder: -";
            else
                return $"Port: {ID}, Cylinder: {Cylinder}";
        }

        //public static Port GetSingleGasWithN2Cylinder(int id, string gasName, float concentrationInPpm) =>
        //     new Port(id,
        //         new GasMixture($"{gasName} ({concentrationInPpm / 1000.0:0.0#}%)", concentrationInPpm, gasName));

        ////[%]*1000 -> ppm
        ////4.9% = 49000 ppm
        //public static Port GetO2Cylinder(int id, float concentrationInPpm = 49000f) => //4.9% 
        //    GetSingleGasWithN2Cylinder(id, "O2", concentrationInPpm);

        ////public static Port GetN2Cylinder(int id) =>
        ////     new Port(id, SimpleGas.DefaultGases["N2"]);
        ////public static Port GetCO2Cylinder(int id) =>
        ////    new Port(id, SimpleGas.DefaultGases["CO2"]);

        //public static Port GetCOCylinder(int id, float concentrationInPpm = 122000f) => //12.2%
        //    GetSingleGasWithN2Cylinder(id, "CO", concentrationInPpm);
        //public static Port GetNOCylinder(int id, float concentrationInPpm = 30030f) => //3.003%
        //    GetSingleGasWithN2Cylinder(id, "NO", concentrationInPpm);
        //public static Port GetC3H6Cylinder(int id, float concentrationInPpm = 5125f) => //3.003%
        //    GetSingleGasWithN2Cylinder(id, "C3H6", concentrationInPpm);


    }

    [Serializable]
    public class PortEventArgs : EventArgs
    {
        public PortEventArgs(Port port)
        {
            this.port = port;
        }

        private Port port;
        public Port Port { get { return port; } }
    }

}
