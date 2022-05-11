using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;
using System;

namespace rmMinusR.EventBus
{
    /// <summary>
    /// Handles the dispatching of Events to listeners.
    /// In most cases EventBus.Main should be used, but multiple EventBuses may be created for cases like unit testing.
    /// </summary>
    public sealed class EventBus : IDisposable
    {
        #region Lifecycle

        //Default main instance
        public static EventBus Main => __mainInstance != null ? __mainInstance : (__mainInstance = new EventBus("Main")); //NOTE: Listeners may not be guaranteed valid across scene changes
        private static EventBus __mainInstance = null;

        //For EventBusDebugger
        private static List<WeakReference<EventBus>> __AllInstances = new List<WeakReference<EventBus>>();
        public static List<EventBus> AllInstances
        {
            get
            {
                //Ensure main is valid
                _ = Main;

                //Remove GC'ed buses
                __AllInstances.RemoveAll(i => !i.TryGetTarget(out _));

                List<EventBus> @out = new List<EventBus>();
                foreach(WeakReference<EventBus> i in __AllInstances) if (i.TryGetTarget(out EventBus bus)) @out.Add(bus);
                return @out;
            }
        }

        public EventBus(string name)
        {
            Name = name;
            __AllInstances.Add(new WeakReference<EventBus>(this));
#if EVENTBUS_VERBOSE_MODE
            Debug.Log("Created new EventBus '"+Name+"'");
#endif
        }

        ~EventBus()
        {
            __AllInstances.RemoveAll(i => i.TryGetTarget(out EventBus bus) && bus == this);
        }

        public void Dispose()
        {
            if (Main == this) throw new InvalidOperationException("Cannot manually destroy bus '"+Name+"'");
            
            //Invalidate state and allow GC cleanup
            bus = null;

            __AllInstances.RemoveAll(i => i.TryGetTarget(out EventBus bus) && bus == this);
        }

        #endregion

        //For debugging
        public string Name { get => __name; private set => __name = value; }
        private string __name;
        public override string ToString() => "EventBus '" + Name + "'";

        #region Listeners

        private SimplePriorityQueue<EventCallback, Priority> bus = new SimplePriorityQueue<EventCallback, Priority>();

        /// <summary>
        /// Registers any functions marked with the [QueryHandler] or
        /// [MessageHandler] annotations. ScopedListener runs this by default.
        /// </summary>
        public void RegisterStaticHandlers(IListener listener)
        {
            foreach (StaticCallbackFactory.Record i in StaticCallbackFactory.GetHandlersOrScan(listener)) //Retrieve all valid methods marked as handlers
            {
                //Add the listener
                if (!bus.Any(h => h.owner == listener && h is StaticCallback s && s.target == ((StaticCallback)h).target))
                {
                    bus.Enqueue(new StaticCallback(i.eventType, listener, i.callInfo), i.priority);
                }
            }
        }

        /// <summary>
        /// Registers a lambda or function not marked with the Handler annotations.
        /// Scripts that register using this are responsible for either using UnregisterAllHandlers
        /// or using the returned callback handle to unregister only that handler.
        /// </summary>
        public EventCallback RegisterDynamicHandler<TEvent>(IListener listener, DynamicCallback<TEvent>.Func target, Priority priority) where TEvent : Event
        {
            //Add the listener
            if (!bus.Any(h => h.owner == listener && h is DynamicCallback<TEvent> d && d.target == target))
            {
                EventCallback callback = new DynamicCallback<TEvent>(listener, target);
                bus.Enqueue(callback, priority);
                return callback;
            }

            //Callback has already been registered, return null instead
            return null;
        }

        /// <summary>
        /// Gets rid of a handler registered through RegisterDynamicHandler.
        /// </summary>
        public void UnregisterCallback(EventCallback callback)
        {
            bus.Remove(callback);
        }

        /// <summary>
        /// Gets rid of all handlers of the given type owned by the given object. Use sparingly.
        /// </summary>
        public void UnregisterHandlersOfType(IListener listener, Type eventType)
        {
            List<EventCallback> toRemove = bus.Where(h => h.owner == listener && h.eventType.IsAssignableFrom(eventType)).ToList();
            foreach (EventCallback h in toRemove) bus.Remove(h);
        }

        /// <summary>
        /// Unregisters a listener from ALL events.
        /// </summary>
        /// <details>
        /// Should be used when an object goes out of scope, especially for MonoBehaviours which
        /// may become invalid and throw errors if access is attempted after disposed.
        /// </details>
        public void UnregisterAllHandlers(IListener listener)
        {
            List<EventCallback> toRemove = bus.Where(h => h.owner == listener).ToList();
            foreach (EventCallback h in toRemove) bus.Remove(h);
        }

        #endregion

        #region Dispatching

        /// <summary>
        /// Sends an event to all registered listeners.
        /// </summary>
        public T DispatchImmediately<T>(T @event) where T : Event
        {
            if(@event.HasBeenDispatched) throw new InvalidOperationException("Event has already been dispatched!");

            @event.HasBeenDispatched = true;

            foreach (EventCallback i in bus)
            {
                if (i.eventType.IsAssignableFrom(@event.GetType())) //Slow?
                {
                    try
                    {
                        i.Dispatch(@event);
                    }
                    catch (Exception exc)
                    {
                        Debug.LogException(exc);
                    }
                }
            }
            
            return @event;
        }

        #endregion
    }

}