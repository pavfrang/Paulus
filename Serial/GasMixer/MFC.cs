using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial.GasMixer
{
    public class MFC
    {
        public MFC(int id, IEnumerable<Port> ports) : this(id)
        {
            Ports.AddRange(ports);
        }

        public MFC(int id)
        {
            ID = id;
        }

        public int ID { get; private set; }
        public List<Port> Ports { get; internal set; } = new List<Port>();

        public Port CurrentPort { get; set; }

        /// <summary>
        /// The present size of MFC in ccm.
        /// </summary>
        public float SizeInCcm { get; internal set; }


        /// <summary>
        /// Actual flow is used in flow mode (as a read value) and in concentration mode (as a calculated value).
        /// </summary>
        public float ActualFlowInCcm { get; internal set; }

        /// <summary>
        /// Target flow is used in flow mode (as a read value) and in concentration mode (as a calculated value).
        /// </summary>
        public float TargetFlowInCcm { get; set; }


         public float ActualPurgeFlowInCcm { get; internal set; }
        /// <summary>
        /// Target purge flow is used in purge mode only.
        /// </summary>
       public float TargetPurgeFlowInCcm { get; set; }


        /// <summary>
        /// Actual concentration is used in concentration mode only.
        /// </summary>
        public float ActualConcentrationInPpm { get; internal set; }

        public float TargetConcentrationInPpm { get; set; }

        public Warning Warning { get; set; }
        public bool IsPurgeOn { get; set; }

        public override string ToString()
        {
            string sPorts = string.Join(", ", Ports.Select(p => $"{p.ID}"));
            if (CurrentPort != null)
                return $"MFC: {ID}, Assigned Port: {CurrentPort.ID}, Available Ports: {sPorts}, Size: {SizeInCcm:0.0} ccm";
            else
                return $"MFC: {ID}, Assigned Port: -, Available Ports: {sPorts}, Size {SizeInCcm:0.0} ccm";

        }
    }

}
