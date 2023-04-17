using System;

namespace Combat
{
    //TODO fix coupling: requires on sequential dispatch. Remedied somewhat by API.

    [Serializable]
    public class HealEvent : CombatEvent
    {
        public readonly ICombatTarget target;
        public readonly ICombatEffect healingEffect;

        public readonly float originalHeal;
        public float heal;

        internal HealEvent(ICombatAffector source, ICombatTarget target, ICombatEffect healingEffect, float heal) : base(source)
        {
            this.target = target;
            this.healingEffect = healingEffect;

            this.originalHeal = heal;
            this.heal = heal;
        }
    }
}
