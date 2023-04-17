namespace Combat
{

    /// <summary>
    /// Has a health pool: player, enemy, breakable window, tree
    /// </summary>
    public interface ICombatTarget
    {
        public float GetHealth();
        public float GetMaxHealth();
        public bool IsAlive();

        public Group GetAlignment();

        internal void DirectApplyDamage(float damage, ICombatAffector source, ICombatEffect damagingEffect);
        internal void DirectApplyHeal(float heal, ICombatAffector source, ICombatEffect damagingEffect);
        internal void DirectKill(ICombatAffector source, ICombatEffect damagingEffect);
    }

}