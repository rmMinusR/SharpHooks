using System;

namespace Combat
{
    /// <summary>
    /// An entity that deals damage or inflicts statuses: Player, enemy, poison dart trap
    /// </summary>
    public interface ICombatAffector
    {
        /// <summary>
        /// Is the given target friend or foe?
        /// </summary>
        public Sentiment GetSentimentTowards(ICombatTarget other);
    }

    [Flags]
    public enum Faction
    {
        PlayerCharacter   = 1 << 0,
        AttackerCharacter = 1 << 8
    }

    public enum Sentiment
    {
        Passive,  //No interaction
        Friendly, //Can target with buffs
        Hostile   //Can target with damage
    }

}