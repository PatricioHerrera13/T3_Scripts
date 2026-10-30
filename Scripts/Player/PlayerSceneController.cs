using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneController : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerCombat combat;

    private float originalGravityScale = 3f; // valor por defecto de seguridad

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();

        // Guardamos la gravedad real del prefab (la que configuraste = 3)
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
        // Como ahora el Player se apaga completamente fuera de gameplay,
        // este script se usa menos, pero lo mantenemos seguro.
        bool isGameplayLevel = scene.name.Contains("_Logic");

        if (!isGameplayLevel)
        {
            // Congelar
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
                rb.simulated = false;
            }

            if (movement != null) movement.enabled = false;
            if (combat != null) combat.enabled = false;
        }
        else
        {
            // Reactivar con la gravedad ORIGINAL del prefab
            if (rb != null)
            {
                rb.simulated = true;
                rb.gravityScale = originalGravityScale; // ← aquí estaba el bug (antes forzaba 1f)
            }

            if (movement != null) movement.enabled = true;
            if (combat != null) combat.enabled = true;
        }
    }
}