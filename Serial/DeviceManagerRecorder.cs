using Paulus.Common;
using Paulus.Serial.InfusionPump;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Serial
{
    public class DeviceManagerRecorder : Recorder
    {
        public DeviceManagerRecorder(
            DeviceManager deviceManager,
            string prefix, string basePath, int timeStepInMs) : base(prefix , basePath, timeStepInMs)
        {
            DeviceManager = deviceManager;
        }

     

        public DeviceManager DeviceManager { get; }

        public override Variable[] Variables
        {
            get
            {
                return DeviceManager.Where(c => c.IncludeInRecorder).SelectMany(c=>c.GetVariables()).ToArray();
            }
        }

        protected override object[] Values
        {
            get
            {
                return DeviceManager.Where(c => c.IncludeInRecorder).SelectMany(c => c.GetVariableValues()).ToArray();
            }
        }
    }
}
