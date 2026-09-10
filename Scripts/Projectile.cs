using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float maxRange = 8f;
    [SerializeField] private int maxBounces = 4;
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;
    private Vector2 startPosition;
    private int currentBounces = 0;
    private bool hasBeenInitialized = false;
    private bool hasHit = false;

    // Quién lanzó este proyectil
    private GameObject owner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Ahora recibe también al dueño
    public void Initialize(Vector2 direction, GameObject owner)
    {
        this.owner = owner;
        startPosition = transform.position;
        rb.linearVelocity = direction.normalized * speed;
        hasBeenInitialized = true;
        hasHit = false;

        // Ignorar colisión física con el dueño (muy importante)
        Collider2D[] ownerColliders = owner.GetComponentsInChildren<Collider2D>();
        Collider2D projectileCollider = GetComponent<Collider2D>();

        foreach (var col in ownerColliders)
        {
            if (col != null && projectileCollider != null)
            {
                Physics2D.IgnoreCollision(projectileCollider, col);
            }
        }
    }

    private void Update()
    {
        if (!hasBeenInitialized) return;

        float distanceTraveled = Vector2.Distance(startPosition, transform.position);
        if (distanceTraveled >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;

        // No hacer daño al dueño
        if (collision.gameObject == owner || collision.transform.IsChildOf(owner.transform))
            return;

        TryDealDamage(collision.gameObject);

        currentBounces++;

        if (currentBounces >= maxBounces)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (other.gameObject == owner || other.transform.IsChildOf(owner.transform))
            return;

        TryDealDamage(other.gameObject);
    }

    private void TryDealDamage(GameObject target)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            hasHit = true;
            Destroy(gameObject);
        }
    }
}