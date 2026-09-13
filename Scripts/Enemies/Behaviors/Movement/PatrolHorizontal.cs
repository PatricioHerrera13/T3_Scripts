using UnityEngine;

public class PatrolHorizontal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCore core;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private MovementZone movementZone;

    [Header("Patrol Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float waitTimeAtEdge = 1.2f;
    [SerializeField] private bool startFacingRight = true;

    [Header("Limits")]
    [Tooltip("Si está activo, usa la MovementZone asignada. Si no, usa los puntos manuales.")]
    [SerializeField] private bool useMovementZoneLimits = true;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    private bool movingRight;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private void Awake()
    {
        if (core == null) core = GetComponentInParent<EnemyCore>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();

        movingRight = startFacingRight;

        // Aplicamos el facing inicial a través del Core
        if (core != null)
            core.SetFacing(movingRight);
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

        if (core.CurrentState != EnemyState.Patrol && core.CurrentState != EnemyState.Idle)
        {
            return;
        }

        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                movingRight = !movingRight;
                core.SetFacing(movingRight);          // ← Ahora usa el Core
            }
            return;
        }

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
        if (useMovementZoneLimits)
        {
            if (movementZone == null) return false;

            Vector3 nextPosition = transform.position + (movingRight ? Vector3.right : Vector3.left) * 0.2f;
            Vector3 clamped = movementZone.ClampPosition(nextPosition);

            return Mathf.Abs(clamped.x - transform.position.x) < 0.05f;
        }
        else
        {
            if (leftPoint == null || rightPoint == null) return false;

            float currentX = transform.position.x;

            if (movingRight && currentX >= rightPoint.position.x) return true;
            if (!movingRight && currentX <= leftPoint.position.x) return true;

            return false;
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