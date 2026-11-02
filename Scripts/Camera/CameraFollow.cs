using UnityEngine;
using System.Collections.Generic;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Header("Target")]
    public Transform target;
    public Vector2 offset = new Vector2(0f, 0.5f);

    [Header("Suavizado base")]
    public float smoothTimeX = 0.2f;
    public float smoothTimeY = 0.2f;

    [Header("Look Ahead")]
    public bool useLookAhead = false;               // Lo dejamos desactivado por defecto
    public float lookAheadDistance = 0.8f;
    public float lookAheadSmooth = 0.35f;

    [Header("Soft Bounds (importante)")]
    [Tooltip("Qué tan suave frena la cámara al llegar al borde. Más alto = más suave")]
    public float softBoundStrength = 3.5f;

    [Header("Transición entre zonas")]
    public float zoneTransitionMultiplier = 2.8f;
    public float zoneTransitionDuration = 0.7f;

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

        Vector3 desired = target.position + (Vector3)offset;
        bool canUseLookAhead = useLookAhead;

        if (activeZone != null)
        {
            desired = activeZone.GetDesiredCameraPosition(target.position, offset);

            if (activeZone.disableLookAhead)
                canUseLookAhead = false;
        }

        // Look Ahead (solo si está activado)
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
        else
        {
            currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, 0f, ref lookAheadVel, lookAheadSmooth);
        }

        // ---------- SOFT CLAMP (esto elimina el movimiento de mierda) ----------
        if (activeZone != null && activeZone.mode == CameraZone.ZoneMode.Bounds)
        {
            Bounds b = activeZone.GetWorldBounds();

            float minX = b.min.x + halfWidth;
            float maxX = b.max.x - halfWidth;
            float minY = b.min.y + halfHeight;
            float maxY = b.max.y - halfHeight;

            desired.x = SoftClamp(desired.x, minX, maxX, softBoundStrength);
            desired.y = SoftClamp(desired.y, minY, maxY, softBoundStrength);
        }
        else if (useGlobalBounds)
        {
            desired.x = SoftClamp(desired.x, globalMin.x + halfWidth, globalMax.x - halfWidth, softBoundStrength);
            desired.y = SoftClamp(desired.y, globalMin.y + halfHeight, globalMax.y - halfHeight, softBoundStrength);
        }

        // ---------- Suavizado ----------
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

    // Soft Clamp: en vez de cortar de golpe, frena suavemente
    private float SoftClamp(float value, float min, float max, float strength)
    {
        if (value < min)
        {
            float diff = min - value;
            return value + diff * (1f - Mathf.Exp(-diff * strength * Time.deltaTime * 60f));
        }
        else if (value > max)
        {
            float diff = value - max;
            return value - diff * (1f - Mathf.Exp(-diff * strength * Time.deltaTime * 60f));
        }
        return value;
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