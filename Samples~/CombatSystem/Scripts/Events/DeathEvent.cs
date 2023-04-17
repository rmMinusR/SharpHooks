using System;

namespace Combat
{
    [Serializable]
    public class DeathEvent : CombatEvent
    {
        public readonly ICombatTarget whoDied;
        public readonly ICombatEffect damagingEffect;

        internal DeathEvent(ICombatAffector source, ICombatTarget whoDied, ICombatEffect damagingEffect) : base(source)
        {
            this.whoDied = whoDied;
            this.damagingEffect = damagingEffect;
        }
    }
}
