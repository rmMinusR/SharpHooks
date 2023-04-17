using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Damage
{
    [field: SerializeField, Min(0)] public float damageAmount { get; set; }

    //If you want to add elemental damage to your game, uncomment this section
    /*
    [field: SerializeField] public Element element { get; set; }
    public enum Element
    {
        Neutral = 0,
        Fire,
        Acid,
        Shock
    }
    */
}
