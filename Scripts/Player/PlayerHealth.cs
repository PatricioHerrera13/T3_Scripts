using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityDuration = 1.2f;   // Tiempo de invulnerabilidad después de recibir daño
    [SerializeField] private float flashInterval = 0.1f;             // Velocidad del parpadeo

    [Header("Feedback (opcional)")]
    [SerializeField] private SpriteRenderer spriteRenderer;          // Para el flash
    [SerializeField] private Color flashColor = new Color(1f, 0.4f, 0.4f, 1f);

    private int currentHealth;
    private bool isInvulnerable = false;
    private bool isDead = false;

    private Color originalColor;
    private Animator animator;

    // Evento simple por si más adelante quieres que la UI escuche
    public System.Action<int, int> OnHealthChanged;   // (current, max)
    public System.Action OnPlayerDied;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        // Notificar a la UI al empezar (si la hubiera)
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)   // ahora es público y cumple la interfaz
    {
        if (isInvulnerable || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        float timer = 0f;
        bool flash = false;

        while (timer < invulnerabilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = flash ? flashColor : originalColor;
                flash = !flash;
            }

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        // Restaurar color original
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        isInvulnerable = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Aquí más adelante puedes poner una animación de muerte
        // animator.SetTrigger("Die");

        OnPlayerDied?.Invoke();

        // Desactivar controles del jugador
        var movement = GetComponent<PlayerMovement>();
        var combat = GetComponent<PlayerCombat>();

        if (movement != null) movement.enabled = false;
        if (combat != null) combat.enabled = false;

        // Pequeña espera antes de ir a GameOver (opcional)
        StartCoroutine(GoToGameOverAfterDelay(0.8f));
    }

    private IEnumerator GoToGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameOver();
        }
        else
        {
            Debug.LogWarning("GameManager no encontrado. Cargando GameOver directamente.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
        }
    }

    // --- Métodos públicos útiles ---

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void FullHeal()
    {
        Heal(maxHealth);
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
    public bool IsInvulnerable() => isInvulnerable;
}