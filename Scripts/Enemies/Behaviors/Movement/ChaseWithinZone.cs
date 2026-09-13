using UnityEngine;

public class ChaseWithinZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCore core;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private MovementZone movementZone;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float stopDistance = 0.8f;
    [SerializeField] private bool flipOnChase = true;

    private void Awake()
    {
        if (core == null) core = GetComponentInParent<EnemyCore>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (core == null || rb == null) return;

        if (core.CurrentState != EnemyState.Chase)
        {
            return;
        }

        Vector3 playerPos = core.GetPlayerPosition();
        Vector3 myPos = transform.position;

        float directionX = playerPos.x - myPos.x;

        if (Mathf.Abs(directionX) <= stopDistance)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float moveDir = Mathf.Sign(directionX);

        Vector3 desiredPos = myPos + new Vector3(moveDir * chaseSpeed * Time.fixedDeltaTime, 0f, 0f);

        if (movementZone != null)
        {
            desiredPos = movementZone.ClampPosition(desiredPos);
        }
        else
        {
            desiredPos = core.GetClampedPosition(desiredPos);
        }

        float velocityX = (desiredPos.x - myPos.x) / Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);

        // Voltear a través del Core
        if (flipOnChase)
        {
            core.FaceDirection(moveDir);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}