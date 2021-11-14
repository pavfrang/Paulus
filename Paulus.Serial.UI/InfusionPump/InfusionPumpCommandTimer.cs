using Paulus.Serial.InfusionPump;
using Paulus.Serial.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Paulus.Serial.InfusionPump.UI
{

    public enum MeasurementTimerCommand
    {
        None,
        GetDeliveredVolume,
        GetError
    }

    public class InfusionPumpCommandTimer : DeviceTimer<InfusionPumpCommander, MeasurementTimerCommand>
    {
        public InfusionPumpCommandTimer(InfusionPumpCommander commander) : base(commander)
        { }

        protected override void sendNextCommand()
        {
            switch (CurrentCommandId)
            {
                case MeasurementTimerCommand.None:
                case MeasurementTimerCommand.GetError:
                    CurrentCommandTask = DeviceCommander.GetDeliveredVolume(); break;
                case MeasurementTimerCommand.GetDeliveredVolume:
                    CurrentCommandTask = DeviceCommander.GetError(); break;
            }
        }
    }
}
