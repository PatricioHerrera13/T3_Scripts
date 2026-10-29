using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;          // El Player

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);

    private bool isFollowing = true;

    private void LateUpdate()
    {
        if (target == null || !isFollowing) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// Teletransporta la cámara instantáneamente al Player (sin suavizado)
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;

        transform.position = target.position + offset;
    }

    /// <summary>
    /// Activa o desactiva el seguimiento suave
    /// </summary>
    public void SetFollowing(bool value)
    {
        isFollowing = value;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}