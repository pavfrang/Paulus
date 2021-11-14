using Paulus.Serial.Fluke;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Paulus.Serial.Bosch
{
    public class BoschRecorder : Recorder
    {
        public BoschRecorder(string basePath, int timeStepInMs,
            BoschCommander boschAgent, FlukeCommander flukeAgent) :
            base("Bosch", basePath, timeStepInMs)
        {
            this.boschAgent = boschAgent;
            this.flukeAgent = flukeAgent;
        }

        BoschCommander boschAgent;
        FlukeCommander flukeAgent;

        public override Variable[] Variables
        {
            get
            {
                return new Variable[] {
                    new Variable("Fluke Resistance","Ohm"),

                    new Variable("Sensor 1 Status"),
                    new Variable("Sensor 1 Resistance","Ohm"),
                    new Variable("Last Sensor 1 Resistance","Ohm"),
                    new Variable("Sensor 1 Regeneration Flag"),

                    new Variable("Sensor 2 Status"),
                    new Variable("Sensor 2 Resistance","Ohm"),
                    new Variable("Last Sensor 2 Resistance","Ohm"),
                    new Variable("Sensor 2 Regeneration Flag"),

                    new Variable("Last command sent"),
                    new Variable("Last Fluke response")
                };
            }
        }

        protected override object[] Values
        {
            get
            {
                BoschSensor sensor1 = boschAgent.Sensor1, sensor2 = boschAgent.Sensor2;
                return new object[]
                {
                    flukeAgent.Fluke.IsOverload ? "OL" : flukeAgent.Fluke.Resistance.ActualValue.ToString("#0.000E+00"),

                    sensor1?.CurrentStatus ?? SensorStatus.Undefined,
                    sensor1 != null ? sensor1.CurrentStatus == SensorStatus.Measuring ? flukeAgent.Fluke.Resistance.ActualValue.ToString("#0.000E+00") : "" : "",
                    sensor1 != null ? sensor1.Resistance.ToString("#0.000E+00") : "",
                    sensor1!=null && sensor1.CurrentStatus==SensorStatus.Regenerating ? 1:0,

                    sensor2?.CurrentStatus ?? SensorStatus.Undefined,
                    sensor2 != null ? sensor2.CurrentStatus == SensorStatus.Measuring ? flukeAgent.Fluke.Resistance.ActualValue.ToString("#0.000E+00") : "" : "",
                    sensor2 != null ? sensor2.Resistance.ToString("#0.000E+00") : "",
                    sensor2!=null && sensor2.CurrentStatus==SensorStatus.Regenerating ? 1:0,

                    boschAgent.LastSerialMessage.MessageSent,
                    flukeAgent.LastSerialMessage.ReceivedFilteredMessage2
                };
            }
        }
    }
}
