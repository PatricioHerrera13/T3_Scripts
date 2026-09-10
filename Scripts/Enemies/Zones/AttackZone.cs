using UnityEngine;

public class AttackZone : MonoBehaviour
{
    [Header("Attack Range")]
    [SerializeField] private float attackRadius = 1.8f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Gizmos")]
    [SerializeField] private Color gizmoColor = new Color(1f, 0.2f, 0.2f, 0.3f);

    private Transform player;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    public bool IsPlayerInAttackRange()
    {
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        return distance <= attackRadius;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, attackRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.7f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}