using System;
using System.Reflection;

namespace rmMinusR.EventBus
{

    public sealed class DynamicCallback<TEvent> : EventCallback where TEvent : Event
    {
        internal DynamicCallback(IListener owner, Func target) : base(typeof(TEvent), owner)
        {
            this.target = target;
        }

        public delegate void Func(TEvent @event);

        public readonly Func target;

        internal override void Dispatch(Event e) => target((TEvent)e);
    }

}