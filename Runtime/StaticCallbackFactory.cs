﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace rmMinusR.EventBus
{
    internal static class StaticCallbackFactory
    {
        #region Scanning

        private static HashSet<Record> StaticScan(IListener listener)
        {
            HashSet<Record> records = new HashSet<Record>();

            foreach(MethodInfo i in listener.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                EventHandlerAttribute attr = GetAttributeFromMethodOrParents<EventHandlerAttribute>(i);
                if (attr != null)
                {

                    if (i.ReturnType == typeof(void)
                     && i.GetParameters().Length == 1
                     && typeof(Event).IsAssignableFrom(i.GetParameters()[0].ParameterType))
                    {
                        records.Add(new Record
                        {
                            callInfo = i,
                            eventType = i.GetParameters()[0].ParameterType,
                            priority = attr.priority
                        });
                    } else UnityEngine.Debug.LogError("Invalid target for EventHandler: "+listener.GetType().Name+"."+i.Name);

                }
            }

#if EVENTBUS_VERBOSE_MODE
            UnityEngine.Debug.Log("Scanned "+listener.GetType().Name+". Found "+records.Count+" handlers.");
#endif
            return records;
        }
        
        private static T GetAttributeFromMethodOrParents<T>(MethodInfo method) where T : Attribute
        {
            //Null check
            if (method == null) return null;

            //Try to get attribute
            T attr = method.GetCustomAttribute<T>();
            if (attr != null) return attr;

            //Not on self, try to scan parent
            MethodInfo parent = method.GetBaseDefinition();
            if (parent != null && parent != method) return GetAttributeFromMethodOrParents<T>(parent);
            else return null;
        }

#endregion
        
#region Caching

        private static Dictionary<Type, HashSet<Record>> cache = new Dictionary<Type, HashSet<Record>>();

        internal static HashSet<Record> GetHandlersOrScan(IListener listener)
        {
            Type listenerType = listener.GetType();
            
            //Try to fetch existing, if it exists
            HashSet<Record> records = null;
            if(!cache.TryGetValue(listenerType, out records))
            {
                //If it doesn't exist yet, default
                records = StaticScan(listener);
                cache.Add(listenerType, records);
            }
#if EVENTBUS_VERBOSE_MODE
            else UnityEngine.Debug.Log("Using cached static handlers for "+listenerType.Name);
#endif

            return records;
        }

        internal static IEnumerable<Record> GetHandlersOrScan(IListener listener, Event @event)
        {
            HashSet<Record> records = GetHandlersOrScan(listener);

            Type eventType = @event.GetType();
            return records.Where(r => r.eventType.IsAssignableFrom(@eventType));
        }

        public struct Record
        {
            public Type eventType;
            public MethodInfo callInfo;
            public Priority priority;
        }

#endregion
    }
}