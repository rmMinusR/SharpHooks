using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;
using System;

namespace rmMinusR.EventBus
{

    public sealed class EventBus : System.IDisposable
    {
        //Default main instance
        public static EventBus Main => __mainInstance != null ? __mainInstance : (__mainInstance = new EventBus("Main")); //NOTE: Listeners not guaranteed valid across scene changes
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

        //For debugging
        public string Name { get => __name; private set => __name = value; }
        private string __name;
        public override string ToString() => "EventBus '" + Name + "'";

        public EventBus(string name)
        {
            Name = name;
            __AllInstances.Add(new WeakReference<EventBus>(this));
            Debug.Log("Created new EventBus '"+Name+"'");
        }

        ~EventBus()
        {
            __AllInstances.RemoveAll(i => i.TryGetTarget(out EventBus bus) && bus == this);
        }

        public void Dispose()
        {
            if (Main == this) throw new System.InvalidOperationException("Cannot manually destroy bus '"+Name+"'");
            
            //Invalidate state and allow GC cleanup
            buses = null;

            __AllInstances.RemoveAll(i => i.TryGetTarget(out EventBus bus) && bus == this);
        }

        #region Listeners

        private Dictionary<System.Type, SimplePriorityQueue<EventCallback, Priority>> buses = new Dictionary<System.Type, SimplePriorityQueue<EventCallback, Priority>>();

        /// <summary>
        /// Registers any functions marked with the [QueryHandler] or
        /// [MessageHandler] annotations. ScopedListener runs this by default.
        /// </summary>
        public void RegisterStaticHandlers(IListener listener)
        {
            foreach (StaticCallbackFactory.Record i in StaticCallbackFactory.GetHandlersOrScan(listener)) //Retrieve all valid methods marked as handlers
            {
                //Add the listener

                SimplePriorityQueue<EventCallback, Priority> pq;
                if(!buses.TryGetValue(i.eventType, out pq)) buses.Add(i.eventType, pq = new SimplePriorityQueue<EventCallback, Priority>());

                if(!pq.Any(h => h.owner == listener && h is StaticCallback s && s.target == ((StaticCallback)h).target)) pq.Enqueue(new StaticCallback(i.eventType, listener, i.callInfo), i.priority);
            }
        }

        /// <summary>
        /// Registers a lambda or function not marked with the Handler annotations.
        /// Scripts that register using this are responsible for either using UnregisterAllHandlers
        /// or using the returned callback handle to unregister only that handler.
        /// </summary>
        public EventCallback RegisterDynamicHandler<TEvent>(IListener listener, DynamicCallback<TEvent>.Func target, Priority priority) where TEvent : Event
        {
            SimplePriorityQueue<EventCallback, Priority> pq;
            if(!buses.TryGetValue(typeof(TEvent), out pq)) buses.Add(typeof(TEvent), pq = new SimplePriorityQueue<EventCallback, Priority>());

            if (!pq.Any(h => h.owner == listener && h is DynamicCallback<TEvent> d && d.target == target))
            {
                EventCallback callback = new DynamicCallback<TEvent>(listener, target);
                pq.Enqueue(callback, priority);
                return callback;
            }
            return null;
        }

        /// <summary>
        /// Gets rid of a handler registered through RegisterDynamicHandler.
        /// </summary>
        public void UnregisterCallback(EventCallback callback)
        {
            buses[callback.eventType].Remove(callback);
        }

        /// <summary>
        /// Gets rid of all handlers of the given type owned by the given object. Use sparingly.
        /// </summary>
        public void UnregisterHandlersOfType(IListener listener, System.Type eventType)
        {
            SimplePriorityQueue<EventCallback, Priority> bus = buses[eventType];
            List<EventCallback> toRemove = bus.Where(h => h.owner == listener).ToList();
            foreach (EventCallback h in toRemove) bus.Remove(h);
        }

        /// <summary>
        /// Unregisters a listener from ALL events.
        /// 
        /// Should be used when an object goes out of scope, especially for MonoBehaviours which
        /// may become invalid and throw errors if access is attempted after disposed.
        /// </summary>
        public void UnregisterAllHandlers(IListener listener)
        {
            foreach (System.Type eventType in buses.Keys) UnregisterHandlersOfType(listener, eventType);
        }

        #endregion

        #region Dispatching

        /// <summary>
        /// Sends an event to all registered listeners.
        /// </summary>
        public T DispatchImmediately<T>(T @event) where T : Event
        {
            if(@event.HasBeenDispatched) throw new System.InvalidOperationException("Event has already been dispatched!");

            @event.HasBeenDispatched = true;

            foreach (KeyValuePair<System.Type, SimplePriorityQueue<EventCallback, Priority>> pair in buses)
            {
                if (pair.Key.IsAssignableFrom(@event.GetType()))
                {
                    foreach (EventCallback i in pair.Value)
                    {
                        try
                        {
                            i.Dispatch(@event);
                        }
                        catch (System.Exception exc)
                        {
                            Debug.LogException(exc);
                        }
                    }
                }
            }

            return @event;
        }

        #endregion
    }

}