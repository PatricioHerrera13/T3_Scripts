using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityDuration = 0.4f;

    [Header("Knockback")]
    [SerializeField] private bool canReceiveKnockback = true;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = new Color(1f, 0.4f, 0.4f, 1f);

    [Header("Death")]
    [Tooltip("Tiempo que espera antes de desactivar el GameObject (para que se vea la animación de muerte)")]
    [SerializeField] private float deathDisableDelay = 0.8f;

    private int currentHealth;
    private bool isInvulnerable = false;
    private bool isDead = false;

    private Color originalColor;
    private Rigidbody2D rb;
    private EnemyCore enemyCore;

    // Eventos
    public System.Action OnDeath;
    public System.Action<int, int> OnHealthChanged; // current, max

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        enemyCore = GetComponent<EnemyCore>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage)
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

        // Feedback de daño
        StartCoroutine(InvulnerabilityCoroutine());

        // Knockback (si está permitido)
        if (canReceiveKnockback && rb != null)
        {
            // El knockback se aplicará desde quien haga daño (dirección)
        }
    }

    // Versión con dirección de knockback
    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        TakeDamage(damage);

        if (canReceiveKnockback && rb != null && !isDead)
        {
            StartCoroutine(ApplyKnockback(knockbackDirection.normalized));
        }
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        rb.linearVelocity = direction * knockbackForce;
        yield return new WaitForSeconds(knockbackDuration);

        if (rb != null)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.3f, rb.linearVelocity.y);
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

            yield return new WaitForSeconds(0.07f);
            timer += 0.07f;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        isInvulnerable = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"<color=red>[{gameObject.name}] DIE() llamado — vida llegó a 0</color>");

        OnDeath?.Invoke();

        StartCoroutine(DisableAfterDeathAnimation());
    }

    private IEnumerator DisableAfterDeathAnimation()
    {
        yield return new WaitForSeconds(deathDisableDelay);
        gameObject.SetActive(false);
    }

    // --- Métodos públicos útiles ---
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
    public bool IsInvulnerable() => isInvulnerable;
}