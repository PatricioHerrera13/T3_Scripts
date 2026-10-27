using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button firstSelectedButton; // Botón "Continuar"

    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction; // Acción Pause del mapa Player

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.performed += OnPausePerformed;
            pauseAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        // Solo permitimos pausar si estamos en un nivel de gameplay
        if (!IsInGameplayLevel()) return;

        if (isPaused)
            Resume();
        else
            Pause();
    }

    /// <summary>
    /// Devuelve true solo si la escena activa es un nivel (contiene "_Logic")
    /// </summary>
    private bool IsInGameplayLevel()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        return activeSceneName.Contains("_Logic");
    }

    public void Pause()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        // Seleccionar el primer botón
        if (firstSelectedButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }

    public void Resume()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    /// <summary>
    /// Cierra el menú de pausa de forma inmediata (usado por GameManager)
    /// </summary>
    public void ClosePauseMenuImmediate()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    // === Botones del menú de pausa ===

    public void OnContinueButton()
    {
        Resume();
    }

    public void OnRestartLevelButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartCurrentLevel();
    }

    public void OnReturnToMainMenuButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }
}