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
        // Importante: NO empezamos la coroutine en este objeto
        // porque EnemyHealth lo va a desactivar y mataría la coroutine.
        // La corremos en el GameManager (que vive en Core y nunca se desactiva).

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartCoroutine(WaitAndCompleteLevel());
        }
        else
        {
            Debug.LogError("BossLevelCompleteOnDeath: GameManager.Instance es null");
        }
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