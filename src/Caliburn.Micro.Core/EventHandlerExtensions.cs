using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caliburn.Micro
{
    public static class EventHandlerExtensions
    {
        public static IEnumerable<EventHandler<TEventArgs>> GetHandlers<TEventArgs>(this EventHandler<TEventArgs> handler)
            where TEventArgs : EventArgs
        {
            return handler.GetInvocationList().Cast<EventHandler<TEventArgs>>().ToList();
        }

        public static void InvokeAll<TEventArgs>(this EventHandler<TEventArgs> handler, object sender, TEventArgs e)
            where TEventArgs : EventArgs
        {
            Parallel.ForEach(handler.GetHandlers(), handler => handler(sender, e));
        }
    }
}
