using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Win32.Simulator
{
    public class MouseOperationsGroup
    {
        #region Constructors
        public MouseOperationsGroup(string name) : this()
        {
            _name = name;
        }

        public MouseOperationsGroup()
        {
            _mouseOperations = new List<SimpleMouseOperation>();
        }
        #endregion

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private List<SimpleMouseOperation> _mouseOperations;
        public List<SimpleMouseOperation> MouseOperations { get { return _mouseOperations; } }


    }
}
