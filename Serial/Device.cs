using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Timers;

namespace Paulus.Serial
{
    //TODO: Per signal simulation and then per device.

    /// <summary>
    /// Can be a device that communicates with the Arduino board such as a Flap.
    /// </summary>
    public class Device
    {
        public Device(bool isSimulated = false)
        {
            signals = new Dictionary<string, Signal>();
            this.isSimulated = isSimulated;
        }

        protected bool isSimulated;
        public bool IsSimulated
        {
            get { return isSimulated; }
            //set
            //{
            //    isSimulated = value;

            //    //change the isSimulated value only for the simulate-able signals
            //    //the SimulatorSignalType.Manual always need manual updating
            //    foreach (var entry in signals)
            //        if (entry.Value.SimulatorSignalType != SimulatorSignalType.Manual)
            //            entry.Value.IsSimulated = isSimulated;
            //}
        }


        protected Dictionary<string, Signal> signals;
        public Dictionary<string, Signal> Signals { get { return signals; } }

        /// <summary>
        /// Device values might be changed depending on the message "type".
        /// </summary>
        /// <param name="message"></param>
        public virtual void UpdateDeviceValuesBySerialMessage(string message, string messageType="")
        {

        }

        //The following methods would be needed only if the signals were NOT auto-updated.

        ///// <summary>
        ///// Makes sense only if the device is simulated.
        ///// </summary>
        //protected virtual void updateDeviceValuesBySimulation()
        //{
        //    foreach (var entry in signals)
        //        entry.Value.updateValue();

        //    updateSignalDependentValuesBySimulation();
        //}



        ///// <summary>
        ///// Makes sense only if the device is simulated.
        ///// </summary>
        //protected virtual void updateSignalDependentValuesBySimulation()
        //{

        //}


    }
}
