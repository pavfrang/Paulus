using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial.Kern
{
    public class KernRecorder : Recorder
    {
        public KernRecorder(string basePath, int timeStepInMs,
            KernArduinoAgent agent) :
            base("Kern", basePath, timeStepInMs)
        {
            this.agent = agent;
        }

        KernArduinoAgent agent { get; }

        public override Variable[] Variables
        {
            get
            {
                return new Variable[]
                    {
                        new Variable("Scale 1","g"),
                        new Variable("Scale 2","g"),
                        new Variable("Scale 1 moving average","g"),
                        new Variable("Scale 2 moving agerage","g"),
                        new Variable("Scale 1 Status"),
                        new Variable("Scale 2 Status"),
                        new Variable("Scale 1 Empty","g"),
                        new Variable("Scale 2 Empty","g"),
                        new Variable("Scale 1 Loaded","g"),
                        new Variable("Scale 2 Loaded","g"),
                        new Variable("Temperature","°C"),
                        new Variable("Last command set"),
                        new Variable("Automation Step")
                    };
            }
        }

        protected override object[] Values
        {
            get
            {
                return new object[]
                {
                    agent.Scale1Agent.KernScale.Weight.ActualValue,
                    agent.Scale2Agent.KernScale.Weight.ActualValue,
                    agent.Scale1MovingAverage,
                    agent.Scale2MovingAverage,
                    agent.Scale2Agent.KernScale.Weight.ActualValue,
                    agent.Scale1Agent.KernScale.IsOverloaded ? "OVERLOAD" : "NORMAL",
                    agent.Scale2Agent.KernScale.IsOverloaded ? "OVERLOAD" : "NORMAL",
                    agent.Scale1Offset,
                    agent.Scale2Offset,
                    agent.Scale1Loaded,
                    agent.Scale2Loaded,
                    agent.Temperature.ActualValue,
                    agent.LastSerialMessage.MessageSent,
                    agent.CurrentAutomationStep
                };
            }
        }
    }
}
