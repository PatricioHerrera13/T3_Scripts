using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Si está activo, mata al player ignorando invulnerabilidad")]
    [SerializeField] private bool instantKill = true;

    [Tooltip("Daño que aplica si instantKill está desactivado")]
    [SerializeField] private int damage = 999;

    [Header("Gizmos")]
    [SerializeField] private Color gizmoColor = new Color(1f, 0.1f, 0.1f, 0.4f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Buscamos el PlayerHealth (puede estar en el mismo objeto o en un padre)
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null || playerHealth.IsDead()) return;

        if (instantKill)
        {
            // Matamos directo (ignoramos invulnerabilidad)
            playerHealth.TakeDamage(999);
        }
        else
        {
            playerHealth.TakeDamage(damage);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.offset, box.size);
        }
        else
        {
            Gizmos.DrawCube(transform.position, Vector3.one);
        }
    }
}