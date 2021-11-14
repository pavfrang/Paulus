using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Paulus.Win32;
using Paulus.Win32.Simulator.Actions;

namespace Paulus.Win32.Simulator
{
    public class KeyboardSimulatorAction : SimulatorAction
    {
        protected override User32.INPUT ToInputAPI()
        {
            throw new NotImplementedException();
        }
    }
}
