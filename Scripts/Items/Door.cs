using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private string keyID = "Key_01";
    [SerializeField] private float openDistance = 1.8f;          // Distancia para abrir
    [SerializeField] private float closeBehindDistance = 1.2f;   // Distancia detrás para cerrar

    [Header("References")]
    [SerializeField] private Collider2D doorCollider;            // El collider sólido de la puerta
    [SerializeField] private Transform frontPoint;               // Punto delante de la puerta
    [SerializeField] private Transform behindPoint;              // Punto detrás de la puerta

    [Header("Optional")]
    [SerializeField] private Animator animator;                  // Si tenés animación de abrir/cerrar
    [SerializeField] private bool startLocked = true;

    private bool isOpen = false;
    private bool hasBeenPassed = false;
    private Transform player;

    private void Start()
    {
        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();

        // Buscamos al player
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
            player = GameManager.Instance.Player.transform;
    }

    private void Update()
    {
        if (player == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.Player != null)
                player = GameManager.Instance.Player.transform;
            return;
        }

        if (hasBeenPassed) return;

        bool hasKey = GameManager.Instance != null && GameManager.Instance.HasKey(keyID);
        if (!hasKey) return;

        // Determinamos de qué lado está el jugador usando la posición X
        // (asumiendo que el jugador viene de la izquierda hacia la derecha)
        float playerX = player.position.x;
        float doorX = transform.position.x;

        bool isOnFrontSide = playerX < doorX;          // Está a la izquierda de la puerta
        bool isOnBehindSide = playerX > doorX;         // Está a la derecha de la puerta

        float distToDoor = Mathf.Abs(playerX - doorX);

        // Abrir: tiene llave + está en el lado de adelante + cerca
        if (!isOpen && isOnFrontSide && distToDoor <= openDistance)
        {
            OpenDoor();
        }

        // Cerrar: ya está abierta + el jugador pasó completamente al otro lado
        if (isOpen && isOnBehindSide && distToDoor >= closeBehindDistance)
        {
            CloseDoorPermanently();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (animator != null)
            animator.SetBool("IsOpen", true);

        Debug.Log($"Puerta {keyID} abierta");
    }

    private void CloseDoorPermanently()
    {
        isOpen = false;
        hasBeenPassed = true;

        if (doorCollider != null)
            doorCollider.enabled = true;

        if (animator != null)
            animator.SetBool("IsOpen", false);

        Debug.Log($"Puerta {keyID} cerrada (el jugador ya pasó)");
    }

    // Opcional: para reiniciar la puerta si se reinicia el nivel
    public void ResetDoor()
    {
        isOpen = false;
        hasBeenPassed = false;

        if (doorCollider != null)
            doorCollider.enabled = true;

        if (animator != null)
            animator.SetBool("IsOpen", false);
    }
}