using UnityEngine;

public class LevelEntryPoint : MonoBehaviour
{
    [Header("Optional")]
    [Tooltip("Por si más adelante queremos varios puntos de entrada")]
    public string entryId = "Default";

    public Transform SpawnTransform => transform;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Esfera
        Gizmos.color = new Color(0.2f, 1f, 0.3f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.35f);

        // Línea hacia arriba para que se vea bien
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.4f);

        // Pequeña cruz
        Gizmos.DrawLine(transform.position + Vector3.left * 0.25f, transform.position + Vector3.right * 0.25f);
        Gizmos.DrawLine(transform.position + Vector3.forward * 0.25f, transform.position + Vector3.back * 0.25f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.45f);
    }
#endif
}