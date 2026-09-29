using UnityEngine;

/// <summary>
/// Zona de salida del nivel.
/// Colocar al final de Level_01 y Level_02.
/// El Collider2D debe tener Is Trigger = true.
/// </summary>
public class GoalZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool disableAfterTrigger = true;

    private bool alreadyTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyTriggered) return;

        // Solo reaccionamos al Player
        if (!other.CompareTag("Player")) return;

        alreadyTriggered = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteCurrentLevel();
        }
        else
        {
            Debug.LogError("GoalZone: No se encontró GameManager.Instance");
        }

        if (disableAfterTrigger)
        {
            gameObject.SetActive(false);
        }
    }
}