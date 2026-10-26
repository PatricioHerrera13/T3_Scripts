using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneController : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerCombat combat;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameplayLevel = scene.name.Contains("_Logic") || scene.name.Contains("Level_");

        // Si no es un nivel de juego, congelamos al player
        if (!isGameplayLevel)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
                rb.simulated = false; // opcional, más agresivo
            }

            if (movement != null) movement.enabled = false;
            if (combat != null) combat.enabled = false;
        }
        else
        {
            // Volvemos a activar todo
            if (rb != null)
            {
                rb.simulated = true;
                rb.gravityScale = 1f; // o el valor que uses normalmente
            }

            if (movement != null) movement.enabled = true;
            if (combat != null) combat.enabled = true;
        }
    }
}