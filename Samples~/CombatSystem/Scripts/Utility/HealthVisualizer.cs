using Combat;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small helper class that runs a health bar.
/// </summary>
public sealed class HealthVisualizer : MonoBehaviour
{
    [SerializeField] private CombatantEntity dataSource;
    [SerializeField] private Image fillTarget;

    private void Update()
    {
        //TODO eventify
        fillTarget.fillAmount = dataSource.GetHealth() / dataSource.GetMaxHealth();
    }
}