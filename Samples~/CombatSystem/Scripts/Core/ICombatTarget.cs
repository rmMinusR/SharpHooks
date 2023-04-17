using rmMinusR.EventBus;
using System.Collections.Generic;
using System.Linq;

namespace Combat
{

    /// <summary>
    /// Has a health pool: player, enemy, breakable window, tree
    /// </summary>
    public interface ICombatTarget
    {
        /// <summary>
        /// Call this to deal damage. May also do more depending on your implementation, such as crowd control and status effects.
        /// </summary>
        /// <param name="how">What effect triggered this? (Sword strike, arrow, burning status)</param>
        /// <param name="effects">How much damage should this deal?</param>
        public void Damage(ICombatEffect how, IEnumerable<Damage> effects);

        /// <summary>
        /// Call this to heal. May also do more depending on your implementation, such as cleanses or buffs.
        /// </summary>
        /// <param name="how">What effect triggered this? (Medkit, heal spell, regeneration status)</param>
        /// <param name="heal">How much health to restore</param>
        public void Heal(ICombatEffect how, float heal);

        /// <summary>
        /// Call this to instantly kill the target, no matter what its current health is.
        /// </summary>
        /// <param name="how">What killed me?</param>
        public void Kill(ICombatEffect how);

        public float GetHealth();
        public float GetMaxHealth();
        public bool IsAlive();

        public Faction GetFaction();

        /// <summary>
        /// Internal method that actually handles damage application. Should ONLY be called after an event has passed through Combat.API
        /// </summary>
        [EventHandler(priority = Priority.Final)]
        internal void DirectApplyDamage(HitEvent @event);

        /// <summary>
        /// Internal method that actually handles heal application. Should ONLY be called after an event has passed through Combat.API
        /// </summary>
        [EventHandler(priority = Priority.Final)]
        internal void DirectApplyHeal(HealEvent @event);

        /// <summary>
        /// Internal method that actually handles death. Should ONLY be called after an event has passed through Combat.API
        /// </summary>
        [EventHandler(priority = Priority.Final)]
        internal void DirectKill(DeathEvent @event);
    }


    /// <summary>
    /// Default implementations for ICombatTarget's more-involved public methods
    /// </summary>
    public static class ICombatTargetDefaults
    {
        /// <summary>
        /// Call this to deal damage. May also do more depending on your implementation, such as crowd control and status effects.
        /// </summary>
        /// <param name="how">What effect triggered this? (Sword strike, arrow, burning status)</param>
        /// <param name="effects">How much damage should this deal?</param>
        public static void DefaultDamage(this ICombatTarget to, ICombatEffect how, IEnumerable<Damage> effects)
        {
            HitEvent ev = new HitEvent(how.GetSource(), to, how, effects);
            EventBus.Main.DispatchImmediately(ev);
        }

        /// <summary>
        /// Call this to heal. May also do more depending on your implementation, such as cleanses or buffs.
        /// </summary>
        /// <param name="how">What effect triggered this? (Medkit, heal spell, regeneration status)</param>
        /// <param name="heal">How much health to restore</param>
        public static void DefaultHeal(this ICombatTarget to, ICombatEffect how, float heal)
        {
            HealEvent ev = new HealEvent(how.GetSource(), to, how, heal);
            EventBus.Main.DispatchImmediately(ev);
        }

        /// <summary>
        /// Call this to instantly kill the target, no matter what its current health is.
        /// </summary>
        /// <param name="how">What killed me?</param>
        public static void DefaultKill(this ICombatTarget target, ICombatEffect how)
        {
            DeathEvent ev = new DeathEvent(how.GetSource(), target, how);
            EventBus.Main.DispatchImmediately(ev);
        }
    }

}