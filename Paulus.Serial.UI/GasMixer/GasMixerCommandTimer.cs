using Paulus.Serial.GasMixer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace Paulus.Serial.UI.GasMixer
{
         public enum MeasurementTimerCommand
        {
            None, //the first command should always be None
            ReadFlowTotalActual,
            ReadActualConcentrations,
            ReadFlowTotalActual2,
            ReadActualConcentrations2,

            //we read the warnings with half the frequency we want
            ReadIsLowFlow,
            ReadWarnings,
        }
   public class GasMixerCommandTimer : DeviceTimer<GasMixerCommander, MeasurementTimerCommand>
    {
        public GasMixerCommandTimer(GasMixerCommander commander) : base(commander)
        { }

        protected override void sendNextCommand()
        {
            //send the next command
            switch (CurrentCommandId)
            {
                //case CurrentMeasurementTimerCommand.None:
                //    break;
                case MeasurementTimerCommand.ReadFlowTotalActual:
                case MeasurementTimerCommand.ReadFlowTotalActual2:
                     CurrentCommandTask = DeviceCommander.ReadFlowTotalActual();
                     break;
                case MeasurementTimerCommand.ReadActualConcentrations:
                case MeasurementTimerCommand.ReadActualConcentrations2:
                    CurrentCommandTask = DeviceCommander.ReadConcentrationAllActual();
                    break;
                case MeasurementTimerCommand.ReadIsLowFlow:
                    CurrentCommandTask = DeviceCommander.ReadIsLowFlow();
                    break;
                case MeasurementTimerCommand.ReadWarnings:
                    CurrentCommandTask = DeviceCommander.ReadWarnings();
                    break;
            }
        }

    }
}
