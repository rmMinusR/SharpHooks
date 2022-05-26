using System;
using System.Reflection;

namespace rmMinusR.EventBus
{
    internal sealed class StaticCallback : EventCallback
    {
        internal StaticCallback(Type eventType, IListener owner, MethodInfo target) : base(eventType, owner)
        {
            this.target = target;
        }

        public readonly MethodInfo target;

        internal override void Dispatch(Event e) => target.Invoke(owner, new object[] { e });
    }
}