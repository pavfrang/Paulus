using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel.DataAnnotations;

namespace Paulus.Serial.GasMixer
{

    public enum Mode
    {
        //Disconnected,
        [Display(Name = "Maintain Ports")]
        MaintainPorts,
        [Display(Name = "Concentration Mode")]
        Concentration,
        [Display(Name = "Flow Mode")]
        Flow,
        [Display(Name = "Purge Mode")]
        Purge
    }



    //Environics 2000
    /// <summary>
    /// The GasMixer object contains the values 
    /// </summary>
    public class GasMixer : Device
    {
        //the object should include simulated objects
        public GasMixer(bool isSimulated = false) : base(isSimulated)
        { }

        public Mode Mode { get; set; }

        public DateTime? DeviceTime { get; internal set; }

        public Dictionary<int, MFC> MFCs { get; private set; } = new Dictionary<int, MFC>();
        public Dictionary<int, Port> Ports { get; } = new Dictionary<int, Port>();
        public MFC BalanceMfc { get; internal set; }

        /// <summary>
        /// This is used in concentration flow only.
        /// </summary>
        public float TotalTargetFlowInCcm { get; set; }
        public float TotalActualFlow { get; internal set; }

        internal void createMFCs(int count)
        {
            //MFCs.Clear();

            //for (int i = 0; i < PortAssignments.Count; i++)
            //    MFCs.Add(i + 1, new MFC(i + 1, Ports[PortAssignments[i]]));

            //MFCs = Enumerable.Range(1, count).
            //       Select(i => new MFC(i)).ToDictionary(mfc => mfc.ID, mfc => mfc);

            MFCs.Clear();
            for (int i = 1; i <= count; i++)
                MFCs.Add(i, new MFC(i));
        }

        internal void createPorts(int count)
        {
            Ports.Clear();
            for (int i = 1; i <= count; i++)
                Ports.Add(i, new Port(i));
        }
    }
}
