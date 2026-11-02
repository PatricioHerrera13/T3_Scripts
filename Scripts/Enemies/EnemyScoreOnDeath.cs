using UnityEngine;

public class EnemyScoreOnDeath : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] private int pointsOnDeath = 100;

    [Header("Floating Popup (estilo Mario)")]
    [SerializeField] private GameObject floatingScorePrefab; // Prefab del número flotante
    [SerializeField] private Vector3 popupOffset = new Vector3(0f, 0.8f, 0f);

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
            health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        // 1. Sumar puntos
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(pointsOnDeath);

        // 2. Spawn del número flotante estilo Mario
        if (floatingScorePrefab != null)
        {
            Vector3 spawnPos = transform.position + popupOffset;
            GameObject popup = Instantiate(floatingScorePrefab, spawnPos, Quaternion.identity);

            FloatingScorePopup floating = popup.GetComponent<FloatingScorePopup>();
            if (floating != null)
                floating.Setup(pointsOnDeath);
        }
    }
}