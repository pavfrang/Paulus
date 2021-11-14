using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Win32.Simulator.Actions
{
    [Serializable]
    public class MouseSimulatorActionEventArgs : EventArgs
    {
        public MouseSimulatorActionEventArgs(MouseSimulatorAction action)
        {
            _action = action;
        }

        protected MouseSimulatorAction _action;
        public MouseSimulatorAction MouseSimulatorAction { get { return _action; } }
    }

}
