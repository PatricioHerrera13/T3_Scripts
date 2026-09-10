using UnityEngine;

public class PatrolHorizontal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCore core;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private MovementZone movementZone;   // ← Ahora se asigna directo

    [Header("Patrol Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float waitTimeAtEdge = 1.2f;
    [SerializeField] private bool startFacingRight = true;

    [Header("Limits")]
    [Tooltip("Si está activo, usa la MovementZone asignada. Si no, usa los puntos manuales.")]
    [SerializeField] private bool useMovementZoneLimits = true;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    // Estado interno
    private bool movingRight;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private void Awake()
    {
        if (core == null) core = GetComponentInParent<EnemyCore>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInParent<SpriteRenderer>();

        movingRight = startFacingRight;
        UpdateFacing();
    }

    private void OnEnable()
    {
        if (core != null)
            core.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (core != null)
            core.OnStateChanged -= HandleStateChanged;
    }

    private void FixedUpdate()
    {
        if (core == null || rb == null) return;

        // Solo actuamos en Patrol o Idle. En cualquier otro estado no tocamos el Rigidbody.
        if (core.CurrentState != EnemyState.Patrol && core.CurrentState != EnemyState.Idle)
        {
            return;   // ← Importante: ya no ponemos la velocidad en 0
        }

        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                movingRight = !movingRight;
                UpdateFacing();
            }
            return;
        }

        // Movimiento de patrulla
        float direction = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (IsAtEdge())
        {
            isWaiting = true;
            waitTimer = waitTimeAtEdge;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    private bool IsAtEdge()
    {
        float currentX = transform.position.x;

        if (useMovementZoneLimits)
        {
            if (movementZone == null) return false;

            // Pedimos la posición clampada un poco más adelante
            Vector3 nextPosition = transform.position + (movingRight ? Vector3.right : Vector3.left) * 0.2f;
            Vector3 clamped = movementZone.ClampPosition(nextPosition);

            // Si al intentar avanzar la posición no cambia (o cambia muy poco), llegamos al borde
            if (Mathf.Abs(clamped.x - transform.position.x) < 0.05f)
            {
                return true;
            }

            return false;
        }
        else
        {
            // Modo puntos manuales
            if (leftPoint == null || rightPoint == null) return false;

            if (movingRight && currentX >= rightPoint.position.x)
                return true;

            if (!movingRight && currentX <= leftPoint.position.x)
                return true;

            return false;
        }
    }

    private void UpdateFacing()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !movingRight;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = movingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void HandleStateChanged(EnemyState newState)
    {
        if (newState == EnemyState.Patrol || newState == EnemyState.Idle)
        {
            isWaiting = false;
            waitTimer = 0f;
        }
    }

    // ---------- Gizmos ----------
    private void OnDrawGizmosSelected()
    {
        if (useMovementZoneLimits) return;

        Gizmos.color = Color.green;

        if (leftPoint != null && rightPoint != null)
        {
            Gizmos.DrawLine(leftPoint.position, rightPoint.position);
            Gizmos.DrawWireSphere(leftPoint.position, 0.15f);
            Gizmos.DrawWireSphere(rightPoint.position, 0.15f);
        }
    }
}