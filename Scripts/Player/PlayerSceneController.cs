using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneController : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerCombat combat;

    private float originalGravityScale = 3f; // valor de seguridad

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();

        // Guardamos la gravedad real del prefab (la que configuraste)
        if (rb != null)
            originalGravityScale = rb.gravityScale;
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
        // Solo reaccionamos cuando se carga una escena de lógica de nivel.
        // Esto evita que Level_XX_Art (u otras escenas) vuelvan a congelar al player.
        if (!scene.name.Contains("_Logic"))
            return;

        // Reactivar física con la gravedad original del prefab
        if (rb != null)
        {
            rb.simulated = true;
            rb.gravityScale = originalGravityScale;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Aseguramos que los scripts de control estén activos
        if (movement != null)
            movement.enabled = true;

        if (combat != null)
            combat.enabled = true;
    }
}