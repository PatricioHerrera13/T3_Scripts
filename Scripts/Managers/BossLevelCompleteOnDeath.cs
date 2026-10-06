using UnityEngine;
using System.Collections;

public class BossLevelCompleteOnDeath : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Tiempo que esperamos para que se vea la animación de muerte del Boss")]
    [SerializeField] private float deathAnimationDuration = 1.4f;

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
        // Esperamos a que termine la animación de muerte antes de cambiar de escena
        StartCoroutine(WaitAndCompleteLevel());
    }

    private IEnumerator WaitAndCompleteLevel()
    {
        yield return new WaitForSeconds(deathAnimationDuration);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteCurrentLevel();
        }
    }
}