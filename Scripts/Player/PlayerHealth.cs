using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityDuration = 1.2f;
    [SerializeField] private float flashInterval = 0.1f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackDuration = 0.18f;

    [Header("Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = new Color(1f, 0.4f, 0.4f, 1f);

    private int currentHealth;
    private bool isInvulnerable = false;
    private bool isDead = false;

    private Color originalColor;
    private Animator animator;
    private Rigidbody2D rb;

    // Esto es lo importante para el movimiento
    public bool IsInKnockback { get; private set; }

    public System.Action<int, int> OnHealthChanged;
    public System.Action OnPlayerDied;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Versión básica (cumple la interfaz)
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector2.zero);
    }

    // Versión con knockback
    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isInvulnerable || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Aplicar knockback si hay dirección
        if (knockbackDirection != Vector2.zero && rb != null)
        {
            StartCoroutine(ApplyKnockback(knockbackDirection.normalized));
        }

        StartCoroutine(InvulnerabilityCoroutine());
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        IsInKnockback = true;

        // Aplicamos la fuerza + un pequeño impulso vertical
        rb.linearVelocity = new Vector2(direction.x * knockbackForce, rb.linearVelocity.y + 2.5f);

        yield return new WaitForSeconds(knockbackDuration);

        // Suavizamos la velocidad horizontal
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.35f, rb.linearVelocity.y);
        }

        IsInKnockback = false;
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

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        isInvulnerable = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        OnPlayerDied?.Invoke();

        var movement = GetComponent<PlayerMovement>();
        var combat = GetComponent<PlayerCombat>();

        if (movement != null) movement.enabled = false;
        if (combat != null) combat.enabled = false;

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