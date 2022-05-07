using System;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// Empty marker for objects that interact with Queries and Messages
    /// </summary>
    public interface IListener
    {

    }

    /// <summary>
    /// MonoBehaviour that automatically manages scope as a Listener, registering static handlers on enable and unregistering ALL handlers on disable.
    /// </summary>
    public abstract class ScopedListener : MonoBehaviour, IListener
    {
        protected virtual void OnEnable() => DoEventRegistration();

        protected virtual void OnDisable() => EventAPI.UnregisterAllHandlers(this);

        protected internal virtual void DoEventRegistration()
        {
            Debug.Log("Registering static events for "+this);
            EventAPI.RegisterStaticHandlers(this);
        }
    }

}