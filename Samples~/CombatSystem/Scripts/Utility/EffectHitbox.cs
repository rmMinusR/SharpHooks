using System;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Data-oriented class for ICombatEffects relying on hitboxes
    /// 
    /// Needs revision.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class EffectHitbox : MonoBehaviour, ICombatEffect
    {
        //NOTE: Funky serialization hack, not guaranteed safe but too bad
        [SerializeField] [Tooltip("MUST be ICombatAffector!")] private MonoBehaviour __source;

        public void SetSource(ICombatAffector source) => __source = (MonoBehaviour) source;
        public ICombatAffector GetSource() => __source as ICombatAffector;

        [Header("Whitelisting")]
        [SerializeField] private bool enableWhitelisting = true;
        [SerializeField] private bool resetHitRecordOnEnable = true;
        [SerializeField] private List<GameObject> hitRecord;

        [Header("Effects")]
        [SerializeField] private List<Damage> effects;

        private void OnEnable()
        {
            if(resetHitRecordOnEnable) hitRecord.Clear();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            ICombatTarget t = other.GetComponent<ICombatTarget>();
            if (t != null && t != GetSource())
            {
                if(!enableWhitelisting || !hitRecord.Contains(other.gameObject))
                {
                    Apply(t);
                    hitRecord.Add(other.gameObject);
                }
            }
        }

        public void Apply(ICombatTarget target) => CombatAPI.Hit(GetSource(), target, this, effects);
    }
}
