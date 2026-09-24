using UnityEngine;
using System.Collections;

public class ChargeAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCore core;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask playerLayer;

    [Header("Pattern (opcional)")]
    [Tooltip("Si está vacío usa TowardPlayer (comportamiento actual)")]
    [SerializeField] private MonoBehaviour chargePattern;   // Debe implementar IChargePattern

    [Header("Timing")]
    [SerializeField] private float windupTime = 0.75f;
    [SerializeField] private float recoveryTime = 0.35f;
    [SerializeField] private float attackCooldown = 2.0f;

    [Header("Dash (solo se usan si no hay Pattern)")]
    [SerializeField] private float dashSpeed = 9.8f;
    [SerializeField] private float dashDistance = 4.8f;
    [SerializeField] private float overshoot = 0.65f;
    [SerializeField] private Vector2 windupOffset = new Vector2(0f, 0.4f);

    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageRadius = 0.85f;
    [SerializeField] private float damageInterval = 0.28f;

    [Header("Options")]
    [SerializeField] private bool faceDuringWindup = true;
    [SerializeField] private bool faceDuringDash = true;
    [SerializeField] private bool showGizmos = true;

    // Internals
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private float lastDamageTime = -999f;

    private Vector3 dashStartPos;
    private Vector3 dashEndPos;
    private Vector3 dashDirection;

    private IChargePattern pattern;

    private void Awake()
    {
        if (core == null) core = GetComponentInParent<EnemyCore>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();

        // Intentamos obtener el patrón si está asignado
        if (chargePattern != null)
        {
            pattern = chargePattern as IChargePattern;
            if (pattern == null)
            {
                Debug.LogWarning($"{name}: El objeto asignado en Charge Pattern no implementa IChargePattern", this);
            }
        }
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

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void HandleAttackRequested()
    {
        if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartCoroutine(PerformCharge());
        }
        else
        {
            core.FinishAttack();
        }
    }

    private IEnumerator PerformCharge()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        lastDamageTime = -999f;

        Vector3 playerPos = core.GetPlayerPosition();
        Vector3 myPos = transform.position;

        // ========== CÁLCULO DEL PATH ==========
        if (pattern != null)
        {
            // Usa el patrón modular
            pattern.CalculatePath(core, myPos, playerPos,
                out dashStartPos, out dashEndPos, out dashDirection);
        }
        else
        {
            // Fallback: TowardPlayer (comportamiento actual)
            CalculateTowardPlayerPath(myPos, playerPos);
        }

        // Facing al inicio
        if (faceDuringWindup)
            core.FaceTowards(playerPos);

        // ====================== WINDUP ======================
        float windupTimer = 0f;
        while (windupTimer < windupTime)
        {
            if (core.CurrentState != EnemyState.Attack)
            {
                AbortAttack();
                yield break;
            }

            rb.linearVelocity = Vector2.zero;
            windupTimer += Time.deltaTime;
            yield return null;
        }

        // ====================== DASH ======================
        float distanceTraveled = 0f;
        Vector3 previousPos = transform.position;
        float maxDistance = Vector3.Distance(dashStartPos, dashEndPos);

        // Si el patrón no definió bien la distancia, usamos la del inspector
        if (maxDistance < 0.1f)
            maxDistance = dashDistance;

        while (distanceTraveled < maxDistance)
        {
            if (core.CurrentState != EnemyState.Attack)
            {
                AbortAttack();
                yield break;
            }

            Vector3 desiredPos = transform.position + dashDirection * dashSpeed * Time.deltaTime;
            desiredPos = core.GetClampedPosition(desiredPos);

            rb.linearVelocity = (desiredPos - transform.position) / Time.deltaTime;

            if (faceDuringDash)
                core.FaceDirection(dashDirection.x);

            TryDealDamage();

            distanceTraveled += Vector3.Distance(previousPos, transform.position);
            previousPos = transform.position;

            if (Vector3.Distance(transform.position, dashEndPos) < 0.15f)
                break;

            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        // ====================== RECOVERY ======================
        float recoveryTimer = 0f;
        while (recoveryTimer < recoveryTime)
        {
            if (core.CurrentState != EnemyState.Attack)
            {
                AbortAttack();
                yield break;
            }

            rb.linearVelocity = Vector2.zero;
            recoveryTimer += Time.deltaTime;
            yield return null;
        }

        isAttacking = false;
        core.FinishAttack();
    }

    // ========== Fallback TowardPlayer (el comportamiento actual) ==========
    private void CalculateTowardPlayerPath(Vector3 myPos, Vector3 playerPos)
    {
        Vector3 toPlayer = (playerPos - myPos).normalized;
        if (toPlayer.sqrMagnitude < 0.01f)
            toPlayer = core.IsFacingRight ? Vector3.right : Vector3.left;

        dashEndPos = playerPos + toPlayer * overshoot;
        dashEndPos = core.GetClampedPosition(dashEndPos);

        dashStartPos = myPos;
        dashDirection = (dashEndPos - dashStartPos).normalized;
    }

    private void TryDealDamage()
    {
        if (Time.time < lastDamageTime + damageInterval)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius, playerLayer);

        foreach (var hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(damage, knockbackDir);
                lastDamageTime = Time.time;
                continue;
            }

            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                lastDamageTime = Time.time;
            }
        }
    }

    private void AbortAttack()
    {
        isAttacking = false;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, damageRadius);

        if (Application.isPlaying && isAttacking)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(dashStartPos, dashEndPos);
            Gizmos.DrawWireSphere(dashEndPos, 0.2f);
        }
    }
#endif
}