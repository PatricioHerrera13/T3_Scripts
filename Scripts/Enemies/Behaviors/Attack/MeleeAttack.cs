using UnityEngine;
using System.Collections;

public class MeleeAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCore core;
    [SerializeField] private Transform attackPoint;          // Punto desde donde sale el golpe
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackRadius = 0.7f;
    [SerializeField] private float attackDelay = 0.25f;      // Tiempo hasta que el golpe hace daño (sincronizar con animación)
    [SerializeField] private float attackCooldown = 1.1f;
    [SerializeField] private bool showGizmos = true;

    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    private void Awake()
    {
        if (core == null) core = GetComponentInParent<EnemyCore>();
    }

    private void OnEnable()
    {
        if (core != null)
            core.OnAttackRequested += HandleAttackRequested;
    }

    private void OnDisable()
    {
        if (core != null)
            core.OnAttackRequested -= HandleAttackRequested;
    }

    private void HandleAttackRequested()
    {
        if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }
        else
        {
            // Si está en cooldown, le decimos al core que termine el estado Attack
            core.FinishAttack();
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Aquí más adelante puedes disparar la animación de ataque
        // Ejemplo: animator.SetTrigger("Attack");

        // Esperamos el delay para sincronizar con la animación
        yield return new WaitForSeconds(attackDelay);

        // Hacemos daño
        DoDamage();

        // Esperamos un poco más para que termine la animación
        yield return new WaitForSeconds(0.3f);

        isAttacking = false;

        // Avisamos al core que el ataque terminó
        if (core != null)
            core.FinishAttack();
    }

    private void DoDamage()
    {
        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Calculamos dirección de knockback (desde el enemigo hacia el player)
                Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;

                // Intentamos usar la versión con knockback si es PlayerHealth o EnemyHealth
                var playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    // Si más adelante quieres knockback en el player, lo agregamos aquí
                }
                else
                {
                    damageable.TakeDamage(damage);
                }
            }
        }
    }

    // ---------- Gizmos ----------
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || attackPoint == null) return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}