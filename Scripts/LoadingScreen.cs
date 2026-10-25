using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Versión temporal / desactivada.
/// El nuevo flujo Additive del GameManager ya no usa esta pantalla de carga.
/// La dejamos compilando para no romper el proyecto.
/// Más adelante se puede reactivar con un sistema de loading más sofisticado.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Header("UI (opcional)")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;

    private void Start()
    {
        // Temporalmente desactivado.
        // El GameManager ahora maneja todas las cargas Additive directamente.
        Debug.LogWarning("LoadingScreen está presente pero no se usa en el flujo actual (Additive). Se puede borrar o reactivar más adelante.");
        
        // Si por alguna razón se carga esta escena sola, simplemente la dejamos quieta
        // o podrías forzar un retorno al MainMenu:
        // if (GameManager.Instance != null)
        //     GameManager.Instance.ReturnToMainMenu();
    }
}