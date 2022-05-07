using Events;
using System;
using System.Reflection;

namespace Events
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

        internal abstract void Dispatch(PubSubEvent e);
    }

    internal sealed class StaticCallback : EventCallback
    {
        internal StaticCallback(Type eventType, IListener owner, MethodInfo target) : base(eventType, owner)
        {
            this.target = target;
        }

        public readonly MethodInfo target;

        internal override void Dispatch(PubSubEvent e) => target.Invoke(owner, new object[] { e });
    }

    internal sealed class DynamicCallback<TEvent> : EventCallback where TEvent : PubSubEvent
    {
        internal DynamicCallback(IListener owner, EventAPI.HandlerFunction<TEvent> target) : base(typeof(TEvent), owner)
        {
            this.target = target;
        }

        public readonly EventAPI.HandlerFunction<TEvent> target;

        internal override void Dispatch(PubSubEvent e) => target((TEvent)e);
    }

}