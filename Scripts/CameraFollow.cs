using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;          // El Player

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 5f;    // Qué tan suave sigue
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f); // Posición relativa

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        transform.position = smoothedPosition;
    }
}