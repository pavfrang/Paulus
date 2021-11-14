using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Serial.DownTino
{
    public class DownTinoRecorder : Recorder
    {
        public DownTinoRecorder(string basePath, int timeStepInMs,
         DownTinoCommander agent) :
            base("Downtino", basePath, timeStepInMs)
        {
            this.agent = agent;
        }

        DownTinoCommander agent { get; }

        public override Variable[] Variables
        {
            get
            {
                return new Variable[]
                    {
                        new Variable("Relative Pressure 1","mbar"), //kPa
                        new Variable("Relative Pressure 2","mbar"), //kPa
                        new Variable("Differential Pressure 1","mbar"), //psi
                        new Variable("Differential Pressure 2","mbar"), //psi
                        new Variable("Temperature 1","°C"),
                        new Variable("Temperature 2","°C"),
                   };
            }
        }

        protected override object[] Values
        {
            get
            {
                var d = agent.DownTino;
                return new object[]
                {
                    d.RelativePressure1.ActualValue,
                    d.RelativePressure2.ActualValue,
                   d.DifferentialPressure1.ActualValue,
                   d.DifferentialPressure2.ActualValue,
                   d.Temperature1.ActualValue,
                   d.Temperature2.ActualValue
                };
            }
        }
    }
}
