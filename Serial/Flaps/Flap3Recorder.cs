using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial.Flaps
{
    public class Flap3Recorder : Recorder
    {
        public Flap3Recorder(string basePath, int timeStepInMs, Flap3Commander agent) : base("FLAP", basePath, timeStepInMs)
        {
            this.agent = agent;
        }

        Flap3Commander agent;

        public override Variable[] Variables
        {
            get
            {
                return new Variable[]
                {
                    new Variable("Flap A Target Position","%"),
                    new Variable("Flap B Target Position","%"),
                    new Variable("Flap C Target Position","%"),
                    new Variable("Flap A Actual Position","%"),
                    new Variable("Flap B Actual Position","%"),
                    new Variable("Flap C Actual Position","%"),

                    new Variable("Flap A Actual Voltage","V"),
                    new Variable("Flap A Minimum Voltage","V"),
                    new Variable("Flap A Maximum Voltage","V"),

                    new Variable("Flap B Actual Voltage","V"),
                    new Variable("Flap B Minimum Voltage","V"),
                    new Variable("Flap B Maximum Voltage","V"),

                    new Variable("Flap C Actual Voltage","V"),
                    new Variable("Flap C Minimum Voltage","V"),
                    new Variable("Flap C Maximum Voltage","V")
                };
            }
        }

        protected override object[] Values
        {
            get
            {
                Flap flapA = agent?.FlapA, flapB = agent?.FlapB, flapC = agent?.FlapC;

                return new object[] {
                    flapA?.Position.TargetValue.ToString("0.0", en) ??"",
                    flapB?.Position.TargetValue.ToString("0.0", en) ??"",
                    flapC?.Position.TargetValue.ToString("0.0", en) ??"",
                    flapA?.Position.ActualValue.ToString("0.0", en) ??"",
                    flapB?.Position.ActualValue.ToString("0.0", en) ??"",
                    flapC?.Position.ActualValue.ToString("0.0", en) ??"",

                    flapA?.Voltage.ActualValue.ToString("0.00", en) ??"",
                    flapA?.MinimumVoltage.ActualValue.ToString("0.00", en) ??"",
                    flapA?.MaximumVoltage.ActualValue.ToString("0.00", en) ??"",

                    flapB?.Voltage.ActualValue.ToString("0.00", en) ??"",
                    flapB?.MinimumVoltage.ActualValue.ToString("0.00", en) ??"",
                    flapB?.MaximumVoltage.ActualValue.ToString("0.00", en) ??"",

                    flapC?.Voltage.ActualValue.ToString("0.00", en) ??"",
                    flapC?.MinimumVoltage.ActualValue.ToString("0.00", en) ??"",
                    flapC?.MaximumVoltage.ActualValue.ToString("0.00", en) ??""
            };
            }
        }
    }
}
