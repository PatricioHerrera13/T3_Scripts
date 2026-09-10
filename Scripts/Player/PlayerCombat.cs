using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 0.35f;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Debug / Gizmos")]
    [SerializeField] private float projectileMaxRange = 8f;   // Debe coincidir con el del Projectile
    [SerializeField] private bool showFireGizmos = true;

    private Animator animator;
    private PlayerMovement playerMovement;

    private float nextFireTime = 0f;

    private InputAction attackLongAction;
    private InputAction specialAction;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

        var playerMap = inputActions.FindActionMap("Player");
        attackLongAction = playerMap.FindAction("Button2");
        specialAction = playerMap.FindAction("Button3");
    }

    private void OnEnable()
    {
        attackLongAction.Enable();
        specialAction.Enable();

        attackLongAction.performed += OnAttackLong;
        specialAction.performed += OnSpecial;
    }

    private void OnDisable()
    {
        attackLongAction.Disable();
        specialAction.Disable();

        attackLongAction.performed -= OnAttackLong;
        specialAction.performed -= OnSpecial;
    }

    private void OnAttackLong(InputAction.CallbackContext context)
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            animator.SetTrigger("Launch");
            nextFireTime = Time.time + fireCooldown;
        }
    }

    private void Shoot()
    {
        Vector2 direction = playerMovement.IsFacingRight ? Vector2.right : Vector2.left;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            // Ahora le pasamos "quién soy yo"
            projScript.Initialize(direction, this.gameObject);
        }
    }

    private void OnSpecial(InputAction.CallbackContext context)
    {
        // Aquí irá el sistema de ítems más adelante
        Debug.Log("Habilidad / Usar Ítem");
    }

    // -------------------- GIZMOS --------------------
    private void OnDrawGizmosSelected()
    {
        if (!showFireGizmos || firePoint == null) return;

        // Punto de aparición del proyectil
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(firePoint.position, 0.12f);

        // Dirección según hacia dónde mira el personaje
        Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;

        // Línea del rango máximo
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(firePoint.position, firePoint.position + direction * projectileMaxRange);

        // Cruz al final del rango
        Vector3 endPoint = firePoint.position + direction * projectileMaxRange;
        Gizmos.DrawLine(endPoint + Vector3.up * 0.15f, endPoint + Vector3.down * 0.15f);
        Gizmos.DrawLine(endPoint + Vector3.left * 0.15f, endPoint + Vector3.right * 0.15f);
    }
}