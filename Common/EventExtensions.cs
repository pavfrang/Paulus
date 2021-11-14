using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Common
{
    public static class EventExtensions
    {
        public static void RemoveHandlers(this EventHandler e)
        {
            if (e == null) return;
            Delegate[] delegates = e.GetInvocationList();
            foreach (Delegate d in delegates) e -= (EventHandler)d;
        }

        public static void RemoveHandlers<T>(this EventHandler<EventArgs<T>> e)
        {
            if (e == null) return;
            Delegate[] delegates = e.GetInvocationList();
            foreach (Delegate d in delegates) e -= (EventHandler<EventArgs<T>>)d;
        }

        public static void RemoveHandlers<T>(this EventHandler<T> e) where T : EventArgs
        {
            if (e == null) return;
            Delegate[] delegates = e.GetInvocationList();
            foreach (Delegate d in delegates) e -= (EventHandler<T>)d;
        }
    }
}
