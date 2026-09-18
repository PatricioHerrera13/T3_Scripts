using UnityEngine;

public class PatrolFly : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCore core;
    [SerializeField] private Rigidbody2D rb;

    [Header("Patrol Settings")]
    [SerializeField] private float moveSpeed = 2.4f;
    [SerializeField] private float waitTimeAtPoint = 1.3f;
    [SerializeField] private float arriveDistance = 0.18f;

    [Header("Waypoints (obligatorios)")]
    [Tooltip("Si está vacío, el enemigo no se moverá")]
    [SerializeField] private Transform[] waypoints;

    private int currentIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private void Awake()
    {
        if (core == null) core = GetComponentInParent<EnemyCore>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (core != null)
            core.OnStateChanged += HandleStateChanged;

        if (HasValidWaypoints())
        {
            currentIndex = 0;
            isWaiting = false;
        }
    }

    private void OnDisable()
    {
        if (core != null)
            core.OnStateChanged -= HandleStateChanged;
    }

    private void FixedUpdate()
    {
        if (core == null || rb == null) return;

        // Ahora actúa en Idle Y en Patrol (igual que PatrolHorizontal)
        if (core.CurrentState != EnemyState.Patrol && core.CurrentState != EnemyState.Idle)
            return;

        // Si no hay waypoints → no nos movemos
        if (!HasValidWaypoints())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = Vector2.zero;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                currentIndex = (currentIndex + 1) % waypoints.Length;
            }
            return;
        }

        Transform target = waypoints[currentIndex];
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        Vector3 desiredPos = transform.position + direction * moveSpeed * Time.fixedDeltaTime;

        // Respetamos la MovementZone
        desiredPos = core.GetClampedPosition(desiredPos);

        rb.linearVelocity = (desiredPos - transform.position) / Time.fixedDeltaTime;

        // Facing
        if (Mathf.Abs(direction.x) > 0.08f)
            core.FaceDirection(direction.x);

        // ¿Llegamos?
        if (Vector3.Distance(transform.position, target.position) <= arriveDistance)
        {
            isWaiting = true;
            waitTimer = waitTimeAtPoint;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private bool HasValidWaypoints()
    {
        return waypoints != null && waypoints.Length > 0;
    }

    private void HandleStateChanged(EnemyState newState)
    {
        if (newState == EnemyState.Patrol || newState == EnemyState.Idle)
        {
            isWaiting = false;
            waitTimer = 0f;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!HasValidWaypoints()) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawWireSphere(waypoints[i].position, 0.15f);

            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }
    }
#endif
}