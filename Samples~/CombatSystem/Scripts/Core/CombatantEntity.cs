using System;
using UnityEngine;
using rmMinusR.EventBus;
using System.Collections.Generic;

namespace Combat
{
    /// <summary>
    /// Implements most basic functions needed to participate in combat
    /// </summary>
    public class CombatantEntity : ScopedListener, ICombatTarget, ICombatAffector
    {
        [SerializeField] protected float health;
        [SerializeField] protected float maxHealth;
        [SerializeField] protected bool isAlive = true;

        public float GetHealth() => health;
        protected void SetHealth(float newHealth) => health = Mathf.Clamp(newHealth, 0, GetMaxHealth()); //Note: Does not trip events
        public float GetMaxHealth() => maxHealth;

        public bool IsAlive() => isAlive;

        /// <summary>
        /// Internal method that actually handles damage application. Should ONLY be called after an event has passed through Combat.API
        /// </summary>
        void ICombatTarget.DirectApplyDamage(float damage, ICombatAffector source, ICombatEffect damagingEffect)
        {
            //Decrease health by (post-mitigation) damage
            health -= damage;

            //Try to die if our health is 0 or negative
            if(health <= 0) CombatAPI.Kill(this, source, damagingEffect);
        }

        /// <summary>
        /// Internal method that actually handles damage application. Should ONLY be called after an event has passed through Combat.API
        /// </summary>
        void ICombatTarget.DirectApplyHeal(float heal, ICombatAffector source, ICombatEffect damagingEffect)
        {
            //Increase health by (post-mitigation) healing
            health += heal;

            //Prevent overhealing
            if (health > maxHealth) health = maxHealth;
        }

        /// <summary>
        /// Internal method that actually applies death and handles cleanup. Should ONLY be called after an event has passed through Combat.API
        /// </summary>
        void ICombatTarget.DirectKill(ICombatAffector source, ICombatEffect damagingEffect)
        {
            //Mark as dead
            isAlive = false;

            OnDeath();
        }

        //Override this if you want custom death behavior like animations
        protected virtual void OnDeath() => Destroy(gameObject);

        [Header("Alignment")]
        [SerializeField] protected Group alignment;
        public Group GetAlignment() => alignment;
        //[SerializeField] protected Group allyMask;
        [SerializeField] protected Group hostileMask;

        [Serializable]
        protected struct SentimentOverride
        {
            public CombatantEntity combatant;
            public Sentiment sentiment;
        }
        [SerializeField] protected List<SentimentOverride> overrides;

        public virtual Sentiment GetSentimentTowards(ICombatTarget other)
        {
            if ((other.GetAlignment() & hostileMask) != 0) return Sentiment.Hostile;
            foreach(SentimentOverride i in overrides) if(i.combatant == (UnityEngine.Object)other) return i.sentiment;
            return Sentiment.Passive;
        }
    }

}