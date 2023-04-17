using rmMinusR.EventBus;
using System.Collections.Generic;
using System.Linq;

namespace Combat
{
    public static class CombatAPI
    {
        /// <summary>
        /// Deals damage, but may do more depending on your implementation, such as crowd control and status effects.
        /// </summary>
        public static void Hit(ICombatAffector from, ICombatTarget to, ICombatEffect how, IEnumerable<Damage> effects)
        {
            HitEvent ev = new HitEvent(from, to, how, effects);
            EventBus.Main.DispatchImmediately(ev,
                onComplete: ev => {
                    if (!ev.isCancelled) to.DirectApplyDamage(ev.effects.Sum(e => e.damageAmount), from, how);
                }
            );
        }

        /// <summary>
        /// Heals the target.
        /// </summary>
        public static void Heal(ICombatAffector from, ICombatTarget to, ICombatEffect how, float heal)
        {
            HealEvent ev = new HealEvent(from, to, how, heal);
            EventBus.Main.DispatchImmediately(ev,
                onComplete: ev => {
                    if (!ev.isCancelled) to.DirectApplyHeal(ev.heal, from, how);
                }
            );
        }

        /// <summary>
        /// Instantly kills the target, no matter what its current health is.
        /// 
        /// This function is also called when a CombatantEntity's health reaches zero,
        /// which allows other effects to be triggered or for death to be cancelled altogether.
        /// </summary>
        public static void Kill(ICombatTarget target, ICombatAffector killer, ICombatEffect how)
        {
            DeathEvent message = new DeathEvent(killer, target, how);
            EventBus.Main.DispatchImmediately(message,
                onComplete: ev => {
                    if (!message.isCancelled) target.DirectKill(killer, how);
                }
            );
        }
    }
}
