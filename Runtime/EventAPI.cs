using System;

namespace Events
{
    public static class EventAPI
    {
        #region Handler management

        /// <summary>
        /// Registers any functions marked with the [QueryHandler] or
        /// [MessageHandler] annotations. ScopedListener runs this by default.
        /// </summary>
        public static void RegisterStaticHandlers(IListener listener)
        {
            foreach (StaticHandlerCache.Record i in StaticHandlerCache.GetHandlersOrScan(listener)) //Retrieve all valid methods marked as handlers
            {
                EventBus.Instance.AddCallbackStatic(listener, i.eventType, i.callInfo, i.priority); //Add the listener
            }
        }

        /// <summary>
        /// Registers a function typically not marked with the Handler annotations.
        /// Scripts that register using this are responsible for either using
        /// UnregisterAllHandlers or using the returned callback handle to unregister only that handler.
        /// </summary>
        public static EventCallback RegisterDynamicHandler<TEvent>(IListener listener, HandlerFunction<TEvent> target, Priority priority) where TEvent : PubSubEvent
        {
            return EventBus.Instance.AddCallbackDynamic<TEvent>(listener, target, priority);
        }
        public delegate void HandlerFunction<TEvent>(TEvent @event);

        /// <summary>
        /// Gets rid of a handler registered through RegisterDynamicHandler.
        /// </summary>
        public static void UnregisterHandler(EventCallback callback)
        {
            EventBus.Instance.RemoveCallback(callback);
        }

        /// <summary>
        /// Gets rid of all handlers of the given type owned by the given object. Use sparingly.
        /// </summary>
        public static void UnregisterHandlersOfType(IListener listener, System.Type eventType)
        {
            EventBus.Instance.RemoveOwnedCallbacksOfType(listener, eventType);
        }

        /// <summary>
        /// Unregisters a listener from ALL events.
        /// 
        /// Should be used when an object goes out of scope, especially for MonoBehaviours which
        /// may become invalid and throw errors if access is attempted after disposed.
        /// </summary>
        public static void UnregisterAllHandlers(IListener listener)
        {
            EventBus.Instance.RemoveAllOwnedCallbacks(listener);
        }

        #endregion

        /// <summary>
        /// Sends an event to all registered listeners.
        /// </summary>
        public static void Dispatch(PubSubEvent @event)
        {
            EventBus.Instance.DispatchImmediately(@event);
        }
    }
}
