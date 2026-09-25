using UnityEngine;
using System.Collections;

public class ProjectileAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCore core;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Attack Settings")]
    [SerializeField] private float attackDelay = 0.25f;
    [SerializeField] private float attackCooldown = 1.4f;
    [SerializeField] private bool showGizmos = true;

    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    private bool IsControlledByBoss => core is BossCore;

    private void Awake()
    {
        if (core == null) core = GetComponentInParent<EnemyCore>();
    }

    private void OnEnable()
    {
        if (core != null && !IsControlledByBoss)
            core.OnAttackRequested += HandleAttackRequested;
    }

    private void OnDisable()
    {
        if (core != null)
            core.OnAttackRequested -= HandleAttackRequested;
    }

    private void HandleAttackRequested()
    {
        TriggerAttack();
    }

    public void TriggerAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }
        else
        {
            if (core != null)
                core.FinishAttack();
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Aquí más adelante: animator.SetTrigger("Attack");

        if (core != null)
        {
            core.FaceTowards(core.GetPlayerPosition());
        }

        yield return new WaitForSeconds(attackDelay);

        Shoot();

        yield return new WaitForSeconds(0.25f);

        isAttacking = false;

        if (core != null)
            core.FinishAttack();
    }

    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name}: Falta asignar el Projectile Prefab", this);
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning($"{name}: Falta asignar el Fire Point (AttackPoint)", this);
            return;
        }

        if (core == null)
        {
            Debug.LogWarning($"{name}: No se encontró EnemyCore", this);
            return;
        }

        Vector2 direction = (core.GetPlayerPosition() - firePoint.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.Initialize(direction, core.gameObject);
            Debug.Log($"<color=orange>{core.name}</color> disparó proyectil");
        }
        else
        {
            Debug.LogError("El prefab de proyectil no tiene el script Projectile", projectile);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || firePoint == null) return;

        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);
        Gizmos.DrawWireSphere(firePoint.position, 0.15f);

        bool facingRight = true;
        if (core != null)
            facingRight = core.IsFacingRight;

        Gizmos.color = Color.yellow;
        Vector3 dir = facingRight ? Vector3.right : Vector3.left;
        Gizmos.DrawLine(firePoint.position, firePoint.position + dir * 1.3f);
    }
}