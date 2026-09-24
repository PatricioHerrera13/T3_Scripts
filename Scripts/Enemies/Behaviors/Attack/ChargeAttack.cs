using UnityEngine;
using System.Collections;

public class ChargeAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCore core;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask playerLayer;

    [Header("Timing")]
    [SerializeField] private float windupTime = 0.75f;
    [SerializeField] private float recoveryTime = 0.35f;
    [SerializeField] private float attackCooldown = 1.8f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 9.5f;
    [SerializeField] private float dashDistance = 4.8f;          // Distancia total del dash
    [SerializeField] private float overshoot = 0.6f;             // Cuánto se pasa del player
    [SerializeField] private Vector2 windupOffset = new Vector2(0f, 0.4f); // Offset relativo al player (telegraph)

    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageRadius = 0.85f;
    [SerializeField] private float damageInterval = 0.28f;       // Evita multi-hit spam

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

    private void Awake()
    {
        if (core == null) core = GetComponentInParent<EnemyCore>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
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

        // Seguridad: si se desactiva a mitad de ataque, frenamos
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
            // Si está en cooldown, liberamos el estado Attack de inmediato
            core.FinishAttack();
        }
    }

    private IEnumerator PerformCharge()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        lastDamageTime = -999f;

        // ====================== WINDUP ======================
        // Calculamos dirección hacia el player + un poco de overshoot
        Vector3 playerPos = core.GetPlayerPosition();
        Vector3 myPos = transform.position;

        Vector3 toPlayer = (playerPos - myPos).normalized;
        if (toPlayer.sqrMagnitude < 0.01f)
            toPlayer = core.IsFacingRight ? Vector3.right : Vector3.left;

        // Punto de windup (opcional – telegraph)
        Vector3 windupTarget = playerPos + (Vector3)windupOffset;
        windupTarget = core.GetClampedPosition(windupTarget);

        // Punto final del dash (player + overshoot)
        dashEndPos = playerPos + toPlayer * overshoot;
        dashEndPos = core.GetClampedPosition(dashEndPos);

        dashStartPos = transform.position;
        dashDirection = (dashEndPos - dashStartPos).normalized;

        // Facing al inicio
        if (faceDuringWindup)
            core.FaceTowards(playerPos);

        // Esperamos el tiempo de carga (aquí más adelante se puede poner animación / VFX)
        float windupTimer = 0f;
        while (windupTimer < windupTime)
        {
            // Si el estado ya no es Attack (Hurt / Death), abortamos
            if (core.CurrentState != EnemyState.Attack)
            {
                AbortAttack();
                yield break;
            }

            // Durante el windup nos mantenemos quietos (o nos movemos suavemente al offset si querés)
            rb.linearVelocity = Vector2.zero;

            windupTimer += Time.deltaTime;
            yield return null;
        }

        // ====================== DASH ======================
        float distanceTraveled = 0f;
        Vector3 previousPos = transform.position;

        while (distanceTraveled < dashDistance)
        {
            if (core.CurrentState != EnemyState.Attack)
            {
                AbortAttack();
                yield break;
            }

            // Movimiento
            Vector3 desiredPos = transform.position + dashDirection * dashSpeed * Time.deltaTime;
            desiredPos = core.GetClampedPosition(desiredPos);

            Vector2 velocity = (desiredPos - transform.position) / Time.deltaTime;
            rb.linearVelocity = velocity;

            // Facing
            if (faceDuringDash)
                core.FaceDirection(dashDirection.x);

            // Daño
            TryDealDamage();

            // Acumulamos distancia real recorrida
            distanceTraveled += Vector3.Distance(previousPos, transform.position);
            previousPos = transform.position;

            // Seguridad: si llegamos muy cerca del final, salimos
            if (Vector3.Distance(transform.position, dashEndPos) < 0.15f)
                break;

            yield return null;
        }

        // Frenamos al terminar el dash
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

        // Fin limpio
        isAttacking = false;
        core.FinishAttack();
    }

    private void TryDealDamage()
    {
        if (Time.time < lastDamageTime + damageInterval)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius, playerLayer);

        foreach (var hit in hits)
        {
            // Prioridad a PlayerHealth (para knockback)
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(damage, knockbackDir);
                lastDamageTime = Time.time;
                continue;
            }

            // Fallback
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
        // No llamamos FinishAttack aquí porque el estado ya cambió (Hurt/Death)
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // Radio de daño
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, damageRadius);

        // Si estamos en play y tenemos datos de dash, dibujamos la trayectoria
        if (Application.isPlaying && isAttacking)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(dashStartPos, dashEndPos);
            Gizmos.DrawWireSphere(dashEndPos, 0.2f);
        }
    }
#endif
}