using UnityEngine;

public class KeepDistance : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCore core;
    [SerializeField] private Rigidbody2D rb;

    [Header("Distance Settings")]
    [SerializeField] private float preferredDistance = 4.2f;
    [SerializeField] private float distanceTolerance = 0.55f;
    [SerializeField] private float moveSpeed = 3.1f;

    [Header("Strafe Settings")]
    [SerializeField] private bool allowStrafe = true;
    [SerializeField] private float strafeSpeed = 1.1f;
    [SerializeField] private float strafeFrequency = 0.7f;   // Más bajo = menos temblor

    [Header("Ground / Flyer")]
    [SerializeField] private bool constrainToHorizontal = false; // ← Activalo en el Walker

    [Header("Other")]
    [SerializeField] private bool facePlayer = true;

    private float strafeTimer = 0f;

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

        // Decisión principal: acercarse o alejarse
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
            // Distancia correcta → strafe suave (si está activado)
            if (allowStrafe)
            {
                strafeTimer += Time.fixedDeltaTime * strafeFrequency;

                // Movimiento perpendicular más suave
                Vector3 perpendicular = new Vector3(-toPlayer.y, toPlayer.x, 0f).normalized;
                float strafeDir = Mathf.Sin(strafeTimer);

                desiredVelocity = perpendicular * strafeSpeed * strafeDir;
            }
        }

        // Si es enemigo terrestre, eliminamos el movimiento vertical
        if (constrainToHorizontal)
        {
            desiredVelocity.y = 0f;
        }

        // Aplicar movimiento respetando la zona
        Vector3 desiredPos = myPos + desiredVelocity * Time.fixedDeltaTime;
        desiredPos = core.GetClampedPosition(desiredPos);

        Vector2 finalVelocity = (desiredPos - myPos) / Time.fixedDeltaTime;

        // Si es terrestre, preservamos la velocidad Y actual (gravedad)
        if (constrainToHorizontal)
        {
            finalVelocity.y = rb.linearVelocity.y;
        }

        rb.linearVelocity = finalVelocity;

        // Mirar al player
        if (facePlayer)
        {
            core.FaceTowards(playerPos);
        }
    }
}