using Paulus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Serial.Dropletino
{
    public class SensorRelaysRecorder : Recorder
    {
        public SensorRelaysRecorder(string basePath, int timeStepInMs,
       SensorRelayCommander agent) : base("Dropletino", basePath, timeStepInMs)
        {
            this.agent = agent;
        }

        SensorRelayCommander agent { get; }

        public override Variable[] Variables
        {
            get
            {
                return new Variable[]
                    {
                        new Variable("Sensor 1","-"),
                        new Variable("Sensor 2","-"),
                        new Variable("Syringe","-"),
                        new Variable("Delivered Volume","-"),
                        new Variable("Delivered Volume unit","-"),
                        new Variable("Target Volume","-"),
                        new Variable("Target Volume unit","-"),
                        new Variable("Infusion Rate","-"),
                        new Variable("Infusion Rate unit","-"),

                        new Variable("Automation Step"),
                        new Variable("Loops completed")
                   };
            }
        }

        protected override object[] Values
        {
            get
            {
                var settings = agent.InfusionPumpCommander.RuntimeSettings as InfusionPump.InfusionPumpSettings;
                if (agent.InfusionPumpCommander.RuntimeSettings == null)
                    settings = agent.InfusionPumpCommander.EditSettings as InfusionPump.InfusionPumpSettings;

                string automationStep;
                if (agent.Automation != null)
                    if (agent.Automation.IsStopped) automationStep = "Stopped";
                    else if (agent.Automation.IsPaused) automationStep = "Paused";
                    else automationStep = agent.Automation.CurrentAutomationStep.GetDescription();
                else
                    automationStep = "<None>";

                return new object[]
                {
                    //agent.HeatersFanSupply.Heaters.ActualValue ==1.0f ? "ON":"OFF",
                    //agent.HeatersFanSupply.Supply.ActualValue ==1.0f ? "ON":"OFF",
                    //agent.HeatersFanSupply.Fan.ActualValue ==1.0f ? "ON":"OFF",
                    agent.SensorRelays.Sensor1Relay.ActualValue,
                    agent.SensorRelays.Sensor2Relay.ActualValue,

                    settings.Syringe.ToString(),
                    settings.DeliveredVolume,
                    settings.DeliveredVolumeUnit,
                    settings.TargetVolume,
                    settings.TargetVolumeUnit,
                    settings.InfusionRate,
                    settings.InfusionRateUnit,

                    //agent.LastSerialMessage.MessageSent,
                    
                    automationStep,
                    agent.Automation?.LoopsCompleted ?? 0
                };
            }
        }
    }
}
