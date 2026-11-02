using UnityEngine;
using System.Collections.Generic;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Header("Target")]
    public Transform target;
    public Vector2 offset = new Vector2(0f, 0.5f);

    [Header("Suavizado base")]
    public float smoothTimeX = 0.22f;
    public float smoothTimeY = 0.22f;

    [Header("Look Ahead")]
    public bool useLookAhead = true;
    public float lookAheadDistance = 0.9f;          // ← Bajé bastante el valor
    public float lookAheadSmooth = 0.35f;

    [Header("Transición entre zonas")]
    public float zoneTransitionMultiplier = 3.2f;
    public float zoneTransitionDuration = 0.85f;

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

    private float zoneTransitionTimer = 0f;

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

        if (zoneTransitionTimer > 0f)
            zoneTransitionTimer -= Time.deltaTime;

        Vector3 desired;
        bool canUseLookAhead = useLookAhead;

        // ---------- 1. Calcular posición deseada ----------
        if (activeZone != null)
        {
            desired = activeZone.GetDesiredCameraPosition(target.position, offset);

            // Si la zona tiene desactivado el look ahead, no lo usamos
            if (activeZone.disableLookAhead)
                canUseLookAhead = false;

            if (canUseLookAhead && activeZone.mode != CameraZone.ZoneMode.FixedPosition)
            {
                float targetLook = 0f;
                var rb = target.GetComponent<Rigidbody2D>();
                if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.15f)
                {
                    // Sentido normal de Look Ahead (cámara se adelanta hacia donde corre el player)
                    targetLook = Mathf.Sign(rb.linearVelocity.x) * lookAheadDistance;
                }

                currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLook, ref lookAheadVel, lookAheadSmooth);
                desired.x += currentLookAheadX;
            }
            else
            {
                // Suavemente volvemos el look ahead a 0
                currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, 0f, ref lookAheadVel, lookAheadSmooth);
            }

            // Clamp solo en Bounds
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

            if (canUseLookAhead)
            {
                float targetLook = 0f;
                var rb = target.GetComponent<Rigidbody2D>();
                if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.15f)
                {
                    targetLook = Mathf.Sign(rb.linearVelocity.x) * lookAheadDistance;
                }

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

        if (zoneTransitionTimer > 0f)
        {
            dampX *= zoneTransitionMultiplier;
            dampY *= zoneTransitionMultiplier;
        }
        else if (activeZone != null && activeZone.overrideDamping)
        {
            dampX = activeZone.dampingX;
            dampY = activeZone.dampingY;
        }

        Vector3 current = transform.position;
        float newX = Mathf.SmoothDamp(current.x, desired.x, ref velocity.x, dampX);
        float newY = Mathf.SmoothDamp(current.y, desired.y, ref velocity.y, dampY);

        transform.position = new Vector3(newX, newY, -10f);
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

        if (best != activeZone)
        {
            activeZone = best;
            zoneTransitionTimer = zoneTransitionDuration;
        }
        else
        {
            activeZone = best;
        }
    }

    public void SnapToTarget()
    {
        if (target == null) return;

        Vector3 pos = target.position + (Vector3)offset;
        transform.position = new Vector3(pos.x, pos.y, -10f);
        velocity = Vector3.zero;
        currentLookAheadX = 0f;
        lookAheadVel = 0f;
        zoneTransitionTimer = 0f;
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
}