using UnityEngine;
using System.Collections;

public class MeleeAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCore core;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackRadius = 0.7f;
    [SerializeField] private float attackDelay = 0.25f;
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
            core.FinishAttack();
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // El enemigo mira hacia el player antes de atacar
        if (core != null)
        {
            core.FaceTowards(core.GetPlayerPosition());
        }

        // Aquí más adelante: animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDelay);

        DoDamage();

        yield return new WaitForSeconds(0.3f);

        isAttacking = false;

        if (core != null)
            core.FinishAttack();
    }

    private void DoDamage()
    {
        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (var hit in hits)
        {
            // Priorizamos PlayerHealth para poder aplicar knockback
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDir = (hit.transform.position - core.transform.position).normalized;
                playerHealth.TakeDamage(damage, knockbackDir);
                continue;
            }

            // Fallback para cualquier otro IDamageable
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
    }

    // ---------- Gizmos ----------
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || attackPoint == null) return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.7f);
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}