using UnityEngine;

public class DetectionZone : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private bool requiresLineOfSight = false;
    [SerializeField] private LayerMask obstacleLayer; // Solo si usas Line of Sight

    [Header("Gizmos")]
    [SerializeField] private Color gizmoColor = new Color(1f, 0.8f, 0.2f, 0.25f);

    private Transform player;

    private void Start()
    {
        // Buscamos al player una vez
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    public bool IsPlayerInRange()
    {
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > detectionRadius) return false;

        if (requiresLineOfSight)
        {
            return HasLineOfSight();
        }

        return true;
    }

    public Vector3 GetPlayerPosition()
    {
        return player != null ? player.position : Vector3.zero;
    }

    private bool HasLineOfSight()
    {
        if (player == null) return false;

        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);

        // Si no golpeamos nada, tenemos línea de visión
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