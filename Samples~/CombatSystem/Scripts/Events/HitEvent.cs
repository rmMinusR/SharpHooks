using System;
using System.Collections.Generic;
using System.Linq;

namespace Combat
{
    [Serializable]
    public class HitEvent : CombatEvent
    {
        public readonly ICombatTarget target;
        public readonly ICombatEffect damagingEffect;

        public readonly IReadOnlyList<Damage> originalEffects;
        public List<Damage> effects;

        internal HitEvent(ICombatAffector source, ICombatTarget target, ICombatEffect damagingEffect, IEnumerable<Damage> effects) : base(source)
        {
            this.target = target;
            this.damagingEffect = damagingEffect;

            this.effects = new List<Damage>(effects);
            this.originalEffects = new List<Damage>(this.effects);
        }

        public float GetTotalDamage() => effects.Sum(e => e.damageAmount);
    }
}
