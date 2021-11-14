using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Common
{
    public class ExceptionEventArgs  :EventArgs
    {
        public ExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
