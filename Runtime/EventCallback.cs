using System;
using System.Reflection;

namespace rmMinusR.EventBus
{
    public abstract class EventCallback
    {
        public readonly Type eventType;
        public readonly IListener owner;

        internal EventCallback(Type eventType, IListener owner)
        {
            this.eventType = eventType;
            this.owner = owner;
        }

        internal abstract void Dispatch(Event e);
    }

}