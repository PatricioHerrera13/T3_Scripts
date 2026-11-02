using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraZone : MonoBehaviour
{
    public enum ZoneMode
    {
        Bounds,          // Restringe la cámara dentro de los límites del collider
        Rail,            // La cámara sigue el rail (proyecta la posición del player)
        FixedPosition    // La cámara se queda fija en un punto
    }

    [Header("Identificación")]
    public string zoneName = "CameraZone";
    public int priority = 0;

    [Header("Modo de la zona")]
    public ZoneMode mode = ZoneMode.Bounds;

    [Header("Rail (solo si mode = Rail)")]
    public CameraRail rail;

    [Header("Fixed Position (solo si mode = FixedPosition)")]
    public Transform fixedPoint;

    [Header("Comportamiento")]
    public bool lockY = false;
    public bool lockX = false;
    public bool disableLookAhead = false;
    public Vector2 cameraOffset = Vector2.zero;

    [Header("Suavizado opcional")]
    public bool overrideDamping = false;
    public float dampingX = 0.2f;
    public float dampingY = 0.2f;

    [Header("Debug")]
    public Color gizmoColor = new Color(0.2f, 0.8f, 1f, 0.25f);

    private BoxCollider2D col;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (CameraFollow.Instance != null)
            CameraFollow.Instance.EnterZone(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (CameraFollow.Instance != null)
            CameraFollow.Instance.ExitZone(this);
    }

    public Bounds GetWorldBounds()
    {
        return col.bounds;
    }

    public Vector3 GetDesiredCameraPosition(Vector3 playerPosition, Vector2 baseOffset)
    {
        Vector3 desired = playerPosition + (Vector3)baseOffset + (Vector3)cameraOffset;

        switch (mode)
        {
            case ZoneMode.Bounds:
                // Se clampa después en CameraFollow
                break;

            case ZoneMode.Rail:
                if (rail != null)
                {
                    Vector3 closest = rail.GetClosestPoint(playerPosition);
                    desired = closest + (Vector3)cameraOffset;

                    if (lockY) desired.y = closest.y + cameraOffset.y;
                    if (lockX) desired.x = closest.x + cameraOffset.x;
                }
                break;

            case ZoneMode.FixedPosition:
                if (fixedPoint != null)
                    desired = fixedPoint.position + (Vector3)cameraOffset;
                break;
        }

        return desired;
    }

    private void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.9f);
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}