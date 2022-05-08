using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;

namespace Events
{

    internal sealed class EventBus
    {
        #region Singleton

        private static EventBus __instance = null;
        internal static EventBus Instance => __instance != null ? __instance : (__instance = new EventBus()); //NOTE: Not guaranteed valid across scene changes

        #endregion

        #region Listeners

        private Dictionary<System.Type, SimplePriorityQueue<EventCallback, Priority>> buses = new Dictionary<System.Type, SimplePriorityQueue<EventCallback, Priority>>();

        internal void AddCallbackStatic(IListener listener, System.Type eventType, System.Reflection.MethodInfo target, Priority priority)
        {
            SimplePriorityQueue<EventCallback, Priority> pq;
            if(!buses.TryGetValue(eventType, out pq)) buses.Add(eventType, pq = new SimplePriorityQueue<EventCallback, Priority>());

            if(!pq.Any(h => h.owner == listener && h is StaticCallback s && s.target == target)) pq.Enqueue(new StaticCallback(eventType, listener, target), priority);
        }

        internal EventCallback AddCallbackDynamic<TEvent>(IListener listener, EventAPI.HandlerFunction<TEvent> target, Priority priority) where TEvent : Event
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

        internal void RemoveCallback(EventCallback callback)
        {
            buses[callback.eventType].Remove(callback);
        }

        internal void RemoveOwnedCallbacksOfType(IListener listener, System.Type eventType)
        {
            SimplePriorityQueue<EventCallback, Priority> bus = buses[eventType];
            List<EventCallback> toRemove = bus.Where(h => h.owner == listener).ToList();
            foreach (EventCallback h in toRemove) bus.Remove(h);
        }

        internal void RemoveAllOwnedCallbacks(IListener listener)
        {
            foreach (System.Type eventType in buses.Keys) RemoveOwnedCallbacksOfType(listener, eventType);
        }

        #endregion
        
        #region Dispatching

        internal T DispatchImmediately<T>(T @event) where T : Event
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