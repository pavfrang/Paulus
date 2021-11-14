using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial
{
    public class SerialCommandBatch
    {


        protected int _maxAttempts = 5;
        public int MaxAttempts
        {
            get { return _maxAttempts; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(MaxAttempts),"The number of attempts must not be less than one.");

                _maxAttempts = value;
            }
        }
    }
}
