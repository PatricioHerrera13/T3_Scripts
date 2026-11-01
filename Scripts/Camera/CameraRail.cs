using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraRail : MonoBehaviour
{
    [Header("Puntos del Rail (en orden)")]
    public List<Transform> points = new List<Transform>();

    [Header("Visualización")]
    public Color gizmoColor = new Color(1f, 0.6f, 0.1f, 0.9f);
    public float pointSize = 0.25f;

    /// <summary>
    /// Devuelve el punto más cercano del rail a una posición del mundo.
    /// </summary>
    public Vector3 GetClosestPoint(Vector3 worldPosition)
    {
        if (points == null || points.Count == 0)
            return worldPosition;

        if (points.Count == 1)
            return points[0].position;

        float minDist = float.MaxValue;
        Vector3 closest = points[0].position;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 a = points[i].position;
            Vector3 b = points[i + 1].position;

            Vector3 projected = ProjectPointOnSegment(worldPosition, a, b);
            float dist = Vector3.Distance(worldPosition, projected);

            if (dist < minDist)
            {
                minDist = dist;
                closest = projected;
            }
        }

        return closest;
    }

    /// <summary>
    /// Proyecta un punto sobre un segmento A-B.
    /// </summary>
    private Vector3 ProjectPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float abSqr = ab.sqrMagnitude;

        if (abSqr < 0.0001f)
            return a;

        float t = Vector3.Dot(point - a, ab) / abSqr;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    /// <summary>
    /// Devuelve la posición a lo largo del rail según un valor normalizado 0-1.
    /// </summary>
    public Vector3 GetPointAtNormalized(float t)
    {
        if (points == null || points.Count == 0) return transform.position;
        if (points.Count == 1) return points[0].position;

        t = Mathf.Clamp01(t);
        float totalLength = GetTotalLength();
        float targetDist = t * totalLength;
        float currentDist = 0f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            float segLen = Vector3.Distance(points[i].position, points[i + 1].position);
            if (currentDist + segLen >= targetDist)
            {
                float localT = (targetDist - currentDist) / segLen;
                return Vector3.Lerp(points[i].position, points[i + 1].position, localT);
            }
            currentDist += segLen;
        }

        return points[points.Count - 1].position;
    }

    public float GetTotalLength()
    {
        float len = 0f;
        for (int i = 0; i < points.Count - 1; i++)
            len += Vector3.Distance(points[i].position, points[i + 1].position);
        return len;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (points == null || points.Count == 0) return;

        Gizmos.color = gizmoColor;

        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] == null) continue;

            Gizmos.DrawSphere(points[i].position, pointSize);

            if (i < points.Count - 1 && points[i + 1] != null)
            {
                Gizmos.DrawLine(points[i].position, points[i + 1].position);
            }
        }
    }
#endif
}