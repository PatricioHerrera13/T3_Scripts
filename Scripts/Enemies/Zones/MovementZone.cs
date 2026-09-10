using UnityEngine;

public class MovementZone : MonoBehaviour
{
    public enum ZoneType
    {
        HorizontalLimits,   // Ideal para enemigos terrestres (Left + Right)
        BoxArea             // Ideal para voladores o zonas más libres
    }

    [Header("Tipo de Zona")]
    [SerializeField] private ZoneType zoneType = ZoneType.HorizontalLimits;

    [Header("Horizontal Limits (terrestres)")]
    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;

    [Header("Box Area (voladores)")]
    [SerializeField] private Vector2 boxSize = new Vector2(6f, 4f);
    [SerializeField] private Vector2 boxOffset = Vector2.zero;

    [Header("Gizmos")]
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.8f, 1f, 0.35f);

    // ---------- API pública ----------

    public bool IsInside(Vector3 position)
    {
        if (zoneType == ZoneType.HorizontalLimits)
        {
            if (leftLimit == null || rightLimit == null) return true;
            return position.x >= leftLimit.position.x && position.x <= rightLimit.position.x;
        }
        else
        {
            Bounds bounds = GetBoxBounds();
            return bounds.Contains(position);
        }
    }

    public Vector3 ClampPosition(Vector3 position)
    {
        if (zoneType == ZoneType.HorizontalLimits)
        {
            if (leftLimit == null || rightLimit == null) return position;

            float clampedX = Mathf.Clamp(position.x, leftLimit.position.x, rightLimit.position.x);
            return new Vector3(clampedX, position.y, position.z);
        }
        else
        {
            Bounds bounds = GetBoxBounds();
            float x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
            float y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);
            return new Vector3(x, y, position.z);
        }
    }

    public Vector3 GetCenter()
    {
        if (zoneType == ZoneType.HorizontalLimits)
        {
            if (leftLimit == null || rightLimit == null) return transform.position;
            return (leftLimit.position + rightLimit.position) * 0.5f;
        }
        else
        {
            return transform.position + (Vector3)boxOffset;
        }
    }

    private Bounds GetBoxBounds()
    {
        Vector3 center = transform.position + (Vector3)boxOffset;
        return new Bounds(center, boxSize);
    }

    // ---------- Gizmos ----------
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        if (zoneType == ZoneType.HorizontalLimits)
        {
            if (leftLimit != null && rightLimit != null)
            {
                Vector3 left = leftLimit.position;
                Vector3 right = rightLimit.position;

                // Línea horizontal
                Gizmos.DrawLine(left, right);

                // Marcadores verticales
                Gizmos.DrawLine(left + Vector3.up * 0.5f, left + Vector3.down * 0.5f);
                Gizmos.DrawLine(right + Vector3.up * 0.5f, right + Vector3.down * 0.5f);
            }
        }
        else
        {
            Bounds bounds = GetBoxBounds();
            Gizmos.DrawCube(bounds.center, bounds.size);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.7f);

        if (zoneType == ZoneType.HorizontalLimits)
        {
            if (leftLimit != null && rightLimit != null)
            {
                Gizmos.DrawLine(leftLimit.position, rightLimit.position);
            }
        }
        else
        {
            Bounds bounds = GetBoxBounds();
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}