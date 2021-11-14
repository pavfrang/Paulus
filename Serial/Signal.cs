using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Timers;

namespace Paulus.Serial
{
    public enum SimulatorSignalType
    {
        Manual,
        Instant,
        Linear,
        Sigmoid,
        Exponential
    }

    //TODO: Allow other data types and custom value initialization.
    //Signal-dependent signals?
    //Read-only signals such as temperature (perhaps this is already covered here)?
    public class Signal
    {
        public Signal(string name, bool isSimulated=true) : this(isSimulated)
        {
            Name = name;

            lastCommandTime = DateTime.Now;
            targetValue = 0.0f;
            previousTargetValue = 0.0f;
            actualValue = 0.0f;
        }

        public Signal(bool isSimulated)
        {
            rnd = new Random(++seed);
            noiseRange = 0.8f;
            timeConstant = 0.4f;
            simulatorSignalType = SimulatorSignalType.Sigmoid;

            IsSimulated = isSimulated;

            useTimer = false;
            timer = new Timer(100);
            timer.Elapsed += Timer_Elapsed;
            if (useTimer)
                timer.Start();

        }

        public const float InvalidValue = -9999.9f;

        protected static int seed;

        public string Name { get; set; }

        protected bool useTimer;
        public bool UseTimer
        {
            get
            {
                return useTimer;
            }
            set
            {
                useTimer = value;
                if (useTimer)
                    timer.Start();
                else
                    timer.Stop();
            }
        }

        protected Timer timer;
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateValue();
        }

        protected Random rnd;

        protected float noiseRange;
        /// <summary>
        /// The actual value is affected in the range +/- NoiseRange value. The random noise follows the uniform distribution.
        /// </summary>
        public float NoiseRange
        {
            get
            { return noiseRange; }
            set
            {
                noiseRange = value;
            }
        }

        protected float timeConstant;
        public float TimeConstant
        {
            get
            {
                return timeConstant;
            }
            set
            {
                if (timeConstant == value)
                    return;

                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(timeConstant), "The time constant must be a non-zero positive value.");

                timeConstant = value;
            }
        }

        //These properties are per signal
        protected DateTime lastCommandTime;
        public DateTime LastCommandTime { get { return lastCommandTime; } }

        protected SimulatorSignalType simulatorSignalType;
        public SimulatorSignalType SimulatorSignalType
        {
            get { return simulatorSignalType; }
            set
            {
                simulatorSignalType = value;
            }
        }

        public bool IsSimulated { get; set; }


        protected float actualValue;
        public float ActualValue
        {
            get
            {
                if (!useTimer && IsSimulated && simulatorSignalType != SimulatorSignalType.Manual)
                    UpdateValue();

                OnRequestingActualValue();

                return actualValue;
            }
            internal set //this can be set only from a Device object
            {
                actualValue = value;
            }
        }

        /// <summary>
        /// The event is needed for the SimulatorSignalType.Manual signals where the signal value must be set before retrieving the value.
        /// Useful if the device is simulated and the signal is calculated based on other signals.
        /// </summary>
        public event EventHandler RequestingActualValue;
        
        protected virtual void OnRequestingActualValue()
        {
            RequestingActualValue?.Invoke(this, EventArgs.Empty);
        }

        protected float targetValue, previousTargetValue;
        public float TargetValue
        {
            get { return targetValue; }
            set
            {
                previousTargetValue = targetValue;
                targetValue = value;
                lastCommandTime = DateTime.Now;

                if (!timer.Enabled && useTimer && IsSimulated)
                    timer.Start();

            }
        }

        public virtual void UpdateValue()
        {
            //the call is ignored if the signal is not simulated
            if (IsSimulated && simulatorSignalType != SimulatorSignalType.Manual)
                actualValue = getActualValue(lastCommandTime, simulatorSignalType, targetValue, previousTargetValue);
        }


        protected float getActualValue(DateTime commandTime, SimulatorSignalType simulatorSignalType, float targetValue, float previousTargetValue)
        {
            float actualValue;

            float t = (float)(DateTime.Now - commandTime).TotalSeconds;
            if (t == 0f)
                actualValue = previousTargetValue;

            if (t > 10.0f * timeConstant)
                actualValue = targetValue;
            else
            {
                //calculate based on simulator device type
                switch (simulatorSignalType)
                {
                    case SimulatorSignalType.Instant:
                        actualValue = targetValue;
                        break;
                    case SimulatorSignalType.Linear:
                        actualValue = previousTargetValue + (targetValue - previousTargetValue) * t / (10.0f * timeConstant);
                        break;
                    case SimulatorSignalType.Exponential:
                        actualValue = targetValue + (previousTargetValue - targetValue) * (float)Math.Exp(-t / timeConstant / 2.0f);
                        break;
                    case SimulatorSignalType.Sigmoid:
                        actualValue = previousTargetValue + (targetValue - previousTargetValue) / (1.0f + (float)Math.Exp(-t / timeConstant));
                        break;
                    default:
                        throw new InvalidOperationException($"Simulator signal type for '{Name}' is invalid.");
                }
            }

            //add noise
            actualValue += noiseRange / 2.0f - noiseRange * (float)rnd.NextDouble();

            return actualValue;
        }

    }


}
