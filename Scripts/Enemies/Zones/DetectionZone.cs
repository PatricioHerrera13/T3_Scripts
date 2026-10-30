using UnityEngine;

public class DetectionZone : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private bool requiresLineOfSight = false;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Gizmos")]
    [SerializeField] private Color gizmoColor = new Color(1f, 0.8f, 0.2f, 0.25f);

    private Transform player;

    private void Start()
    {
        TryFindPlayer();
    }

    private void TryFindPlayer()
    {
        // Preferimos GameManager (más robusto con Additive + Player apagado/encendido)
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            player = GameManager.Instance.Player.transform;
            return;
        }

        // Fallback clásico
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    public bool IsPlayerInRange()
    {
        if (player == null)
        {
            TryFindPlayer();
            if (player == null) return false;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > detectionRadius) return false;

        if (requiresLineOfSight)
            return HasLineOfSight();

        return true;
    }

    public Vector3 GetPlayerPosition()
    {
        if (player == null)
        {
            TryFindPlayer();
            if (player == null) return Vector3.zero;
        }
        return player.position;
    }

    private bool HasLineOfSight()
    {
        if (player == null) return false;

        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);
        return hit.collider == null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, detectionRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.6f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}