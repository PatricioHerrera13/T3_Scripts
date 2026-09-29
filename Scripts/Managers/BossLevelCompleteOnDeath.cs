using UnityEngine;

/// <summary>
/// Colocar en el mismo GameObject que tiene EnemyHealth / BossCore.
/// Cuando el Boss muere → llama a CompleteCurrentLevel().
/// </summary>
public class BossLevelCompleteOnDeath : MonoBehaviour
{
    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (health == null)
            health = GetComponentInChildren<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDeath += HandleBossDeath;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDeath -= HandleBossDeath;
    }

    private void HandleBossDeath()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteCurrentLevel();
        }
    }
}