using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    private Rigidbody2D rb;
    private Animator animator;

    private float horizontalInput;
    private bool isGrounded;
    private bool facingRight = true;

    private InputAction moveAction;
    private InputAction jumpAction;

    // Referencia pública para que otros scripts sepan si está mirando a la derecha
    public bool IsFacingRight => facingRight;
    public bool IsGrounded => isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        var playerMap = inputActions.FindActionMap("Player");
        moveAction = playerMap.FindAction("Move");
        jumpAction = playerMap.FindAction("Button1");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        jumpAction.performed += OnJump;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        jumpAction.performed -= OnJump;
    }

    private void Update()
    {
        horizontalInput = moveAction.ReadValue<float>();
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // --- FIX ANIMACIÓN IDLE ---
        // Usamos la velocidad real del Rigidbody + un pequeño threshold
        float currentSpeed = Mathf.Abs(rb.linearVelocity.x);
        if (currentSpeed < 0.1f) currentSpeed = 0f;

        animator.SetFloat("Speed", currentSpeed);
        animator.SetBool("IsGrounded", isGrounded);

        // Volteo
        if (horizontalInput > 0.1f && !facingRight)
            Flip();
        else if (horizontalInput < -0.1f && facingRight)
            Flip();
    }

    private void FixedUpdate()
    {
        // Movimiento horizontal
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}