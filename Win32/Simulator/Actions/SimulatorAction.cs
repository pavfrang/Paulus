using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Paulus.Win32;

namespace Paulus.Win32.Simulator.Actions
{
    public abstract class SimulatorAction
    {
        public SimulatorAction() { Active = true; }

        //used for simple simulatoractions (not multiple)
        protected abstract User32.INPUT ToInputAPI();

        public virtual bool Send()
        {
            User32.INPUT input = ToInputAPI();
            return Send(input);
        }

        protected static bool Send(User32.INPUT input)
        {
            uint result = User32.SendInput(1, new User32.INPUT[] { input }, User32.INPUT.Size);
            return result != 0;
        }

        internal static bool Send(IEnumerable<User32.INPUT> inputs)
        {
            User32.INPUT[] array = inputs.ToArray();
            uint result = User32.SendInput((uint)array.Length, array, User32.INPUT.Size);
            return result != 0;
        }

        public static bool Send(IEnumerable<SimulatorAction> simulatedActions)
        {
            return Send(simulatedActions.ToArray());
        }

        public static bool Send(params SimulatorAction[] simulatedActions)
        {
            User32.INPUT[] inputs = new User32.INPUT[simulatedActions.Length];
            for (int i = 0; i < inputs.Length; i++)
                inputs[i] = simulatedActions[i].ToInputAPI();
            uint result = User32.SendInput((uint)inputs.Length, inputs, User32.INPUT.Size);
            return result != 0u;
        }


        //handle of the point below the position (useful in press/release/wheel events)
        //this allows to identify Click events
        //only under move it is not retrieved
        public IntPtr hWnd { get; set; }

        //used in simulation 
        public bool Active { get; set; }

        /// <summary>
        /// The time delay before the action in ms.
        /// </summary>
        public int DelayBefore { get; set; }

        /// <summary>
        /// The time delay after the action in ms.
        /// </summary>
        public int DelayAfter { get; set; }

        public DateTime? ActionTime;
        public TimeSpan? TimeElapsedSinceStart;
    }
}
