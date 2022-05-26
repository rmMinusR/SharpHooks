using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace rmMinusR.EventBus
{
    /// <summary>
    /// Static event handler marker
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class EventHandlerAttribute : Attribute
    {
        public Priority priority = Priority.Normal;
    }
}