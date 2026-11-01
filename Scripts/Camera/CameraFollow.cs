using UnityEngine;
using System.Collections.Generic;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Header("Target")]
    public Transform target;
    public Vector2 offset = new Vector2(0f, 0.5f);

    [Header("Suavizado base")]
    public float smoothTimeX = 0.15f;
    public float smoothTimeY = 0.15f;

    [Header("Look Ahead")]
    public bool useLookAhead = true;
    public float lookAheadDistance = 1.8f;
    public float lookAheadSmooth = 0.25f;

    [Header("Límites globales (fallback)")]
    public bool useGlobalBounds = false;
    public Vector2 globalMin = new Vector2(-100, -30);
    public Vector2 globalMax = new Vector2(100, 30);

    // Internos
    private Vector3 velocity = Vector3.zero;
    private float currentLookAheadX;
    private float lookAheadVel;

    private CameraZone activeZone;
    private readonly List<CameraZone> overlappingZones = new List<CameraZone>();

    private Camera cam;
    private float halfWidth;
    private float halfHeight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cam = GetComponent<Camera>();
        RecalculateHalfSizes();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        RecalculateHalfSizes();

        Vector3 desired;

        // ---------- 1. Calcular posición deseada según zona activa ----------
        if (activeZone != null)
        {
            desired = activeZone.GetDesiredCameraPosition(target.position, offset);

            // Aplicar Look Ahead solo si no está en Fixed
            if (useLookAhead && activeZone.mode != CameraZone.ZoneMode.FixedPosition)
            {
                float targetLook = 0f;
                var rb = target.GetComponent<Rigidbody2D>();
                if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.15f)
                    targetLook = Mathf.Sign(rb.linearVelocity.x) * lookAheadDistance;

                currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLook, ref lookAheadVel, lookAheadSmooth);
                desired.x += currentLookAheadX;
            }

            // Clamp final solo en modo Bounds
            if (activeZone.mode == CameraZone.ZoneMode.Bounds)
            {
                Bounds b = activeZone.GetWorldBounds();
                desired.x = Mathf.Clamp(desired.x, b.min.x + halfWidth, b.max.x - halfWidth);
                desired.y = Mathf.Clamp(desired.y, b.min.y + halfHeight, b.max.y - halfHeight);
            }
        }
        else
        {
            // Follow libre
            desired = target.position + (Vector3)offset;

            if (useLookAhead)
            {
                float targetLook = 0f;
                var rb = target.GetComponent<Rigidbody2D>();
                if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.15f)
                    targetLook = Mathf.Sign(rb.linearVelocity.x) * lookAheadDistance;

                currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLook, ref lookAheadVel, lookAheadSmooth);
                desired.x += currentLookAheadX;
            }

            if (useGlobalBounds)
            {
                desired.x = Mathf.Clamp(desired.x, globalMin.x + halfWidth, globalMax.x - halfWidth);
                desired.y = Mathf.Clamp(desired.y, globalMin.y + halfHeight, globalMax.y - halfHeight);
            }
        }

        // ---------- 2. Suavizado ----------
        float dampX = smoothTimeX;
        float dampY = smoothTimeY;

        if (activeZone != null && activeZone.overrideDamping)
        {
            dampX = activeZone.dampingX;
            dampY = activeZone.dampingY;
        }

        Vector3 current = transform.position;
        float newX = Mathf.SmoothDamp(current.x, desired.x, ref velocity.x, dampX);
        float newY = Mathf.SmoothDamp(current.y, desired.y, ref velocity.y, dampY);

        transform.position = new Vector3(newX, newY, current.z);
    }

    // ===================== API =====================

    public void EnterZone(CameraZone zone)
    {
        if (!overlappingZones.Contains(zone))
            overlappingZones.Add(zone);

        UpdateActiveZone();
    }

    public void ExitZone(CameraZone zone)
    {
        overlappingZones.Remove(zone);
        UpdateActiveZone();
    }

    private void UpdateActiveZone()
    {
        CameraZone best = null;
        int highest = int.MinValue;

        foreach (var z in overlappingZones)
        {
            if (z.priority >= highest)
            {
                highest = z.priority;
                best = z;
            }
        }

        activeZone = best;
    }

    public void SnapToTarget()
    {
        if (target == null) return;

        Vector3 pos = target.position + (Vector3)offset;
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
        velocity = Vector3.zero;
        currentLookAheadX = 0f;
        lookAheadVel = 0f;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void RecalculateHalfSizes()
    {
        if (cam == null) return;
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!useGlobalBounds) return;

        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((globalMin.x + globalMax.x) * 0.5f, (globalMin.y + globalMax.y) * 0.5f, 0);
        Vector3 size = new Vector3(globalMax.x - globalMin.x, globalMax.y - globalMin.y, 0);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}