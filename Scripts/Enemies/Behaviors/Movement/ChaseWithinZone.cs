using UnityEngine;

public class ChaseWithinZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCore core;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private MovementZone movementZone;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float stopDistance = 0.8f;        // Distancia mínima al player
    [SerializeField] private bool flipSprite = true;

    private void Awake()
    {
        if (core == null) core = GetComponentInParent<EnemyCore>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInParent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (core == null || rb == null) return;

        // Solo actuamos en estado Chase
        if (core.CurrentState != EnemyState.Chase)
        {
            return;
        }

        Vector3 playerPos = core.GetPlayerPosition();
        Vector3 myPos = transform.position;

        float directionX = playerPos.x - myPos.x;

        // Si ya estamos lo suficientemente cerca, frenamos
        if (Mathf.Abs(directionX) <= stopDistance)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // Dirección normalizada solo en X
        float moveDir = Mathf.Sign(directionX);

        // Calculamos la posición deseada
        Vector3 desiredPos = myPos + new Vector3(moveDir * chaseSpeed * Time.fixedDeltaTime, 0f, 0f);

        // La clampamos dentro de la MovementZone
        if (movementZone != null)
        {
            desiredPos = movementZone.ClampPosition(desiredPos);
        }
        else if (core != null)
        {
            desiredPos = core.GetClampedPosition(desiredPos);
        }

        // Aplicamos movimiento
        float velocityX = (desiredPos.x - myPos.x) / Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);

        // Voltear sprite
        if (flipSprite)
        {
            UpdateFacing(moveDir > 0);
        }
    }

    private void UpdateFacing(bool faceRight)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !faceRight;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    // Gizmo de la distancia de parada
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}