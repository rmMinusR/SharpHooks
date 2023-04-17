using Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class RepeatingDamageStatus : MonoBehaviour, ICombatEffect
{
    private ICombatTarget target;
    private float timeToNextTick = 0;
    private float timeRemaining;
    private float ticksPerSecond;

    private List<Damage> effectsPerTick;

    private ICombatAffector __source;
    public void SetSource(ICombatAffector source) => __source = source;
    public ICombatAffector GetSource() => __source;
    public void Apply(ICombatTarget target) => target.Damage(this, effectsPerTick);

    private void Update()
    {
        timeToNextTick -= Time.deltaTime;
        if (timeToNextTick <= 0)
        {
            Apply(target);
            timeToNextTick = 1 / ticksPerSecond;
        }

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0) Destroy(gameObject);
    }

    private static RepeatingDamageStatus Apply(ICombatAffector source, ICombatTarget target, float time, float ticksPerSecond, List<Damage> effectsPerTick, GameObject vfx)
    {
        GameObject go = new GameObject(nameof(RepeatingDamageStatus));
        if (target is Component c)
        {
            go.transform.parent = c.transform;
            go.transform.localPosition = Vector3.zero;
            if (vfx != null) Instantiate(vfx, go.transform).transform.localPosition = Vector3.zero;
        }

        RepeatingDamageStatus s = go.AddComponent<RepeatingDamageStatus>();
        s.SetSource(source);
        s.target = target;
        s.timeRemaining = time;
        s.ticksPerSecond = ticksPerSecond;
        s.effectsPerTick = effectsPerTick;

        return s;
    }
}
