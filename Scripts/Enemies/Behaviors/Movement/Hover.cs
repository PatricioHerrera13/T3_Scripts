using UnityEngine;

public class Hover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCore core;
    [SerializeField] private Rigidbody2D rb;

    [Header("Hover Settings")]
    [SerializeField] private float amplitudeY = 0.22f;      // Qué tanto sube y baja
    [SerializeField] private float amplitudeX = 0.12f;      // Pequeño movimiento lateral
    [SerializeField] private float frequency = 1.6f;        // Velocidad del balanceo
    [SerializeField] private bool lockToStartPosition = true; // Si está activo, no se desplaza del punto inicial

    private Vector3 startPosition;
    private float timer;

    private void Awake()
    {
        if (core == null) core = GetComponentInParent<EnemyCore>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        startPosition = transform.position;
        timer = Random.Range(0f, Mathf.PI * 2f); // Para que no todos empiecen igual
    }

    private void FixedUpdate()
    {
        if (core == null || rb == null) return;

        // Solo actúa en Idle
        if (core.CurrentState != EnemyState.Idle)
            return;

        timer += Time.fixedDeltaTime * frequency;

        float offsetY = Mathf.Sin(timer) * amplitudeY;
        float offsetX = Mathf.Sin(timer * 0.7f) * amplitudeX;

        Vector3 targetPos = startPosition + new Vector3(offsetX, offsetY, 0f);

        // Respetamos siempre la MovementZone
        targetPos = core.GetClampedPosition(targetPos);

        // Movimiento suave
        Vector2 desiredVelocity = (targetPos - transform.position) / Time.fixedDeltaTime;
        rb.linearVelocity = desiredVelocity;
    }

    private void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}