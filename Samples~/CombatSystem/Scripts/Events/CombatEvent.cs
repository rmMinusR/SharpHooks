using rmMinusR.EventBus;
using System;

namespace Combat
{
    [Serializable]
    public abstract class CombatEvent : Event
    {
        public ICombatAffector source;

        protected internal CombatEvent(ICombatAffector source)
        {
            this.source = source;
        }
    }
}
