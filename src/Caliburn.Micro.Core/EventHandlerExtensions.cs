using System;
using System.Collections.Generic;
using System.Linq;

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
            foreach (var item in handler.GetHandlers())
            {
                item(sender, e);
            }
        }
    }
}
