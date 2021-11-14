using Paulus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial.HeatersFanSupply
{
    public class HeatersFanSupplyRecorder : Recorder
    {
        public HeatersFanSupplyRecorder(string basePath, int timeStepInMs,
          HeatersFanSupplyCommander agent) :
            base("Acidino", basePath, timeStepInMs)
        {
            this.agent = agent;
        }

        HeatersFanSupplyCommander agent { get; }

        public override Variable[] Variables
        {
            get
            {
                return new Variable[]
                    {
                        new Variable("Heaters","-"),
                        new Variable("Supply","-"),
                        new Variable("Fan","-"),
                        new Variable("Temperature","°C"),

                        new Variable("Last command set"),
                        new Variable("Automation Step"),
                        new Variable("Loops completed")
                   };
            }
        }

        protected override object[] Values
        {
            get
            {
                return new object[]
                {
                    //agent.HeatersFanSupply.Heaters.ActualValue ==1.0f ? "ON":"OFF",
                    //agent.HeatersFanSupply.Supply.ActualValue ==1.0f ? "ON":"OFF",
                    //agent.HeatersFanSupply.Fan.ActualValue ==1.0f ? "ON":"OFF",
                    agent.HeatersFanSupply.Heaters.ActualValue,
                    agent.HeatersFanSupply.Supply.ActualValue,
                    agent.HeatersFanSupply.Fan.ActualValue,
                    agent.HeatersFanSupply.Temperature.ActualValue,

                    agent.LastCommandSent,
                    //agent.LastSerialMessage.MessageSent,
                    agent.CurrentAutomationStep.GetDescription(),
                    agent.LoopsCompleted
                };
            }
        }
    }
}
