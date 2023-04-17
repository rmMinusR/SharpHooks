using System;

namespace Combat
{
    /// <summary>
    /// The action or ability that causes a state change, such as the projectile of Magic Missile.
    /// </summary>
    public interface ICombatEffect
    {
        public void SetSource(ICombatAffector source);
        public ICombatAffector GetSource();
        public void Apply(ICombatTarget target);
    }
    
}