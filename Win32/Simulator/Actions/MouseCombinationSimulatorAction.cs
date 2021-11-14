using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Paulus.Win32.Simulator.Actions
{
    public class MouseCombinationSimulatorAction : MouseSimulatorAction
    {
        public MouseCombinationSimulatorAction(int delayBefore, int delayAfter, params MouseSimulatorAction[] mouseEvents)
            : this((IEnumerable<MouseSimulatorAction>)mouseEvents, delayBefore, delayAfter) { }

        public MouseCombinationSimulatorAction(IEnumerable<MouseSimulatorAction> mouseEvents, int delayBefore = 0, int delayAfter = 0)
            : base(MouseActionType.Combination, MousePositionType.Relative, Point.Empty, delayBefore, delayAfter)
        {
            _actions = new List<MouseSimulatorAction>();
            _actions.AddRange(mouseEvents);
        }


        protected List<MouseSimulatorAction> _actions;
        public List<MouseSimulatorAction> Actions { get { return _actions; } }

        public override bool Send()
        {
            bool sent = true;
            foreach (MouseSimulatorAction action in _actions)
                if (!(sent && action.Send())) return false;

            return sent;

            //the following could be valid only for simple (non-combination) simulator actions
            //List<WinAPI.INPUT> inputs = new List<WinAPI.INPUT>();
            //foreach (MouseSimulatorAction action in _actions)
            //    inputs.Add(action.ToInputAPI());

            //return SimulatorAction.Send(inputs);
        }
    }
}
