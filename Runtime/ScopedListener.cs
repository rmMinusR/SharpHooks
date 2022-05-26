using System;
using UnityEngine;

namespace rmMinusR.EventBus
{
    /// <summary>
    /// MonoBehaviour that automatically manages scope as a Listener, registering static handlers on enable and unregistering ALL handlers on disable.
    /// </summary>
    public abstract class ScopedListener : MonoBehaviour, IListener
    {
        protected virtual void OnEnable() => DoEventRegistration();

        protected virtual void OnDisable() => EventBus.Main.UnregisterAllHandlers(this);

        protected internal virtual void DoEventRegistration()
        {
#if EVENTBUS_VERBOSE_MODE
            Debug.Log("Registering static events for "+this+" on bus '"+EventBus.Main.Name+"'");
#endif
            EventBus.Main.RegisterStaticHandlers(this);
        }

        public virtual string GetDebugName() => name;
    }

}