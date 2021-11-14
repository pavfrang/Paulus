using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Paulus.Forms
{
    public class ProgressChangedEventArgs<T> : ProgressChangedEventArgs
    {
        public ProgressChangedEventArgs( int progressPercentage, T typedUserState) 
        : base(progressPercentage,typedUserState)
        {
            _typedUserState = typedUserState;
        }

        protected T _typedUserState;
        public T TypedUserState { get { return _typedUserState; } set { _typedUserState = value; } }
    }
}
