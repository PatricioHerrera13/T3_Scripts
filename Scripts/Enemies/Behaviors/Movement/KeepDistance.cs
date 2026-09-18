using UnityEngine;

public class KeepDistance : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCore core;
    [SerializeField] private Rigidbody2D rb;

    [Header("Distance Settings")]
    [SerializeField] private float preferredDistance = 4.5f;
    [SerializeField] private float distanceTolerance = 0.6f;   // Margen de error
    [SerializeField] private float moveSpeed = 3.2f;
    [SerializeField] private float strafeSpeed = 1.8f;         // Movimiento lateral mientras mantiene distancia

    [Header("Behavior")]
    [SerializeField] private bool allowStrafe = true;
    [SerializeField] private bool facePlayer = true;

    private void Awake()
    {
        if (core == null) core = GetComponentInParent<EnemyCore>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (core == null || rb == null) return;

        if (core.CurrentState != EnemyState.Chase)
            return;

        Vector3 playerPos = core.GetPlayerPosition();
        Vector3 myPos = transform.position;

        Vector3 toPlayer = playerPos - myPos;
        float currentDistance = toPlayer.magnitude;

        Vector3 desiredVelocity = Vector3.zero;

        // Decidir si acercarse o alejarse
        if (currentDistance > preferredDistance + distanceTolerance)
        {
            // Muy lejos → acercarse
            desiredVelocity = toPlayer.normalized * moveSpeed;
        }
        else if (currentDistance < preferredDistance - distanceTolerance)
        {
            // Muy cerca → alejarse
            desiredVelocity = -toPlayer.normalized * moveSpeed;
        }
        else
        {
            // Distancia correcta → opcionalmente strafe
            if (allowStrafe)
            {
                // Movimiento perpendicular (strafe)
                Vector3 perpendicular = new Vector3(-toPlayer.y, toPlayer.x, 0f).normalized;
                // Alternamos dirección de vez en cuando o usamos seno
                float strafeDir = Mathf.Sin(Time.time * 1.5f);
                desiredVelocity = perpendicular * strafeSpeed * strafeDir;
            }
        }

        // Aplicar movimiento respetando la zona
        Vector3 desiredPos = myPos + desiredVelocity * Time.fixedDeltaTime;
        desiredPos = core.GetClampedPosition(desiredPos);

        rb.linearVelocity = (desiredPos - myPos) / Time.fixedDeltaTime;

        // Mirar al player
        if (facePlayer)
        {
            core.FaceTowards(playerPos);
        }
    }
}