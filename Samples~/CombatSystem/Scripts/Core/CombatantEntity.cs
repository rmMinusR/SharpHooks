using System;
using UnityEngine;
using rmMinusR.EventBus;
using System.Collections.Generic;

namespace Combat
{
    /// <summary>
    /// Implements most basic functions needed to participate in combat. Most of the time you will be using this.
    /// </summary>
    public class CombatantEntity : ScopedListener, ICombatTarget, ICombatAffector
    {
        #region Health

        [SerializeField] protected float health;
        [SerializeField] protected float maxHealth;
        [SerializeField] protected bool isAlive = true;


        public float GetHealth() => health;
        public float GetMaxHealth() => maxHealth;

        public bool IsAlive() => isAlive;

        #endregion

        #region Damage and healing

        //Use default implementations
        public void Damage(ICombatEffect how, IEnumerable<Damage> effects) => this.DefaultDamage(how, effects);
        public void Heal(ICombatEffect how, float heal) => this.DefaultHeal(how, heal);
        public void Kill(ICombatEffect how) => this.DefaultKill(how);

        void ICombatTarget.DirectApplyDamage(HitEvent @event)
        {
            health -= @event.GetTotalDamage(); //Decrease health by (post-mitigation) damage
            if (health <= 0) this.Kill(@event.damagingEffect); //Try to die if we're out of health
        }

        void ICombatTarget.DirectApplyHeal(HealEvent @event)
        {
            health += @event.heal; //Increase health by (post-mitigation) healing
            if (health > maxHealth) health = maxHealth; //Prevent overhealing
        }

        void ICombatTarget.DirectKill(DeathEvent @event)
        {
            isAlive = false; //Mark as dead
            HandleDeath();
        }

        // Override this if you want stuff like death animations
        protected virtual void HandleDeath() => Destroy(gameObject);

        #endregion

        #region Determining friend and foe

        [Header("Alignment")]
        [SerializeField] protected Faction faction;
        public Faction GetFaction() => faction;
        //[SerializeField] protected Faction allyMask;
        [SerializeField] protected Faction hostileMask;

        [Serializable]
        protected struct SentimentOverride
        {
            public CombatantEntity combatant;
            public Sentiment sentiment;
        }
        [SerializeField] protected List<SentimentOverride> overrides;

        public virtual Sentiment GetSentimentTowards(ICombatTarget other)
        {
            if (hostileMask.HasFlag(other.GetFaction())) return Sentiment.Hostile;
            foreach(SentimentOverride i in overrides) if (i.combatant == (UnityEngine.Object)other) return i.sentiment;
            return Sentiment.Passive;
        }

        #endregion
    }

}