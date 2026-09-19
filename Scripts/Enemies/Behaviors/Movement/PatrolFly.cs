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

    [Header("Smooth Flying (estilo hover mientras viaja)")]
    [SerializeField] private float bobAmplitude = 0.18f;     // Qué tanto sube y baja mientras viaja
    [SerializeField] private float bobFrequency = 2.2f;      // Velocidad del balanceo

    [Header("Waypoints (obligatorios)")]
    [Tooltip("Si está vacío, el enemigo no se moverá")]
    [SerializeField] private Transform[] waypoints;

    private int currentIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private float bobTimer = 0f;

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

        // Actúa en Idle y en Patrol
        if (core.CurrentState != EnemyState.Patrol && core.CurrentState != EnemyState.Idle)
            return;

        if (!HasValidWaypoints())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        bobTimer += Time.fixedDeltaTime;

        if (isWaiting)
        {
            // Mientras espera también hace un poco de bobbing suave
            waitTimer -= Time.fixedDeltaTime;

            float bobOffset = Mathf.Sin(bobTimer * bobFrequency) * bobAmplitude * 0.6f;
            Vector3 waitPos = waypoints[currentIndex].position + Vector3.up * bobOffset;
            waitPos = core.GetClampedPosition(waitPos);

            rb.linearVelocity = (waitPos - transform.position) / Time.fixedDeltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                currentIndex = (currentIndex + 1) % waypoints.Length;
            }
            return;
        }

        // Movimiento hacia el waypoint + bobbing
        Transform target = waypoints[currentIndex];
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;

        // Posición base hacia el waypoint
        Vector3 desiredPos = transform.position + direction * moveSpeed * Time.fixedDeltaTime;

        // Le sumamos el movimiento vertical suave
        float bobOffsetY = Mathf.Sin(bobTimer * bobFrequency) * bobAmplitude;
        desiredPos.y += bobOffsetY * Time.fixedDeltaTime * 8f; // suavizado

        // Respetamos la zona
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