using Paulus.Serial.Fluke;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Paulus.Serial.Fluke
{
    public class FlukeRecorder : Recorder
    {
        public FlukeRecorder(string basePath, int timeStepInMs,
            FlukeCommander flukeAgent) :
            base("Fluke", basePath, timeStepInMs)
        {
            this.flukeAgent = flukeAgent;
        }

        FlukeCommander flukeAgent;

        public override Variable[] Variables
        {
            get
            {
                return new Variable[] {
                    new Variable("Fluke Resistance","Ohm"),
                    new Variable("Is Overload","-"),
                    new Variable("Last Fluke response")
                };
            }
        }

        protected override object[] Values
        {
            get
            {
                return new object[]
                {
                    //flukeAgent.Fluke.IsOverload ? "OL" : flukeAgent.Fluke.Resistance.ActualValue.ToString("#0.000E+00"),
                    flukeAgent.Fluke.IsOverload ? 5e8.ToString("#0.000E+00") : flukeAgent.Fluke.Resistance.ActualValue.ToString("#0.000E+00"),
                    //flukeAgent.Fluke.Resistance.ActualValue.ToString("#0.000E+00"),
                    flukeAgent.Fluke.IsOverload ? "YES" : "NO",
                    flukeAgent.LastSerialMessage?.ReceivedFilteredMessage2 ?? ""
                };
            }
        }
    }
}
