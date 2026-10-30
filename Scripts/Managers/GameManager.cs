using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Reference")]
    public PlayerMovement Player;
    public PlayerHealth PlayerHealth;
    public GameObject PlayerGameObject;      // El GameObject completo del Player
    public GameObject GameplayHUD;           // El Canvas de la barra de vida

    [Header("Scene Names - Flow")]
    public string mainMenuScene = "MainMenu";
    public string tutorialScene = "Tutorial";
    public string levelCompleteScene = "LevelComplete";
    public string endingScene = "Ending";

    [Header("Level Order (base names)")]
    public string[] levelOrder = { "Level_01", "Level_02", "Level_03" };

    // Estado interno
    private int currentLevelIndex = -1;
    private string currentLevelBaseName;
    private bool isLoading = false;

    // Vida con la que se empezó el nivel actual
    private int levelStartHealth = 3;

    // ==================== SISTEMA DE LLAVES ====================
    private HashSet<string> collectedKeys = new HashSet<string>();

    public void CollectKey(string keyID)
    {
        if (string.IsNullOrEmpty(keyID)) return;

        collectedKeys.Add(keyID);
        Debug.Log($"Llave recogida: {keyID}");
    }

    public bool HasKey(string keyID)
    {
        return collectedKeys.Contains(keyID);
    }

    public void ClearAllKeys()
    {
        collectedKeys.Clear();
    }

    // ==========================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Buscar referencias si no están asignadas
        if (Player == null)
            Player = FindFirstObjectByType<PlayerMovement>(FindObjectsInactive.Include);

        if (PlayerHealth == null && Player != null)
            PlayerHealth = Player.GetComponent<PlayerHealth>();

        if (PlayerGameObject == null && Player != null)
            PlayerGameObject = Player.gameObject;
    }

    // ==================== ACTIVAR / DESACTIVAR PLAYER + HUD ====================

    public void ActivatePlayerAndHUD()
    {
        if (PlayerGameObject != null)
            PlayerGameObject.SetActive(true);

        if (GameplayHUD != null)
            GameplayHUD.SetActive(true);
    }

    public void DeactivatePlayerAndHUD()
    {
        if (PlayerGameObject != null)
            PlayerGameObject.SetActive(false);

        if (GameplayHUD != null)
            GameplayHUD.SetActive(false);
    }

    // ==================== FLUJO PRINCIPAL ====================

    public void StartGame()
    {
        if (isLoading) return;
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        isLoading = true;

        // 1. Tapar pantalla
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        // 2. Descargar menú
        yield return UnloadSceneIfLoaded(mainMenuScene);

        // 3. Cargar Tutorial (solo imagen de controles)
        yield return SceneManager.LoadSceneAsync(tutorialScene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(tutorialScene));

        // IMPORTANTE: NO activamos Player ni HUD aquí.
        // El Tutorial es solo una imagen/gif de controles.
        // El Player aparece recién cuando se carga el Level_01.

        // 4. Revelar pantalla
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeIn();

        isLoading = false;
    }

    public void FinishTutorial()
    {
        if (isLoading) return;
        StartCoroutine(FinishTutorialRoutine());
    }

    private IEnumerator FinishTutorialRoutine()
    {
        isLoading = true;

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        yield return UnloadSceneIfLoaded(tutorialScene);

        currentLevelIndex = 0;
        currentLevelBaseName = levelOrder[0];
        yield return LoadLevelPair(currentLevelBaseName);

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeIn();

        isLoading = false;
    }

    public void CompleteCurrentLevel()
    {
        if (isLoading) return;
        StartCoroutine(CompleteCurrentLevelRoutine());
    }

    private IEnumerator CompleteCurrentLevelRoutine()
    {
        isLoading = true;

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        if (!string.IsNullOrEmpty(currentLevelBaseName))
            yield return UnloadLevelPair(currentLevelBaseName);

        yield return SceneManager.LoadSceneAsync(levelCompleteScene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(levelCompleteScene));

        // En LevelComplete normalmente no queremos al Player visible
        DeactivatePlayerAndHUD();

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeIn();

        isLoading = false;
    }

    public void LoadNextLevel()
    {
        if (isLoading) return;
        StartCoroutine(LoadNextLevelRoutine());
    }

    private IEnumerator LoadNextLevelRoutine()
    {
        isLoading = true;

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        yield return UnloadSceneIfLoaded(levelCompleteScene);

        currentLevelIndex++;

        if (currentLevelIndex >= levelOrder.Length)
        {
            // Ir al Ending
            yield return SceneManager.LoadSceneAsync(endingScene, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(endingScene));
            DeactivatePlayerAndHUD();
        }
        else
        {
            currentLevelBaseName = levelOrder[currentLevelIndex];
            yield return LoadLevelPair(currentLevelBaseName);
        }

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeIn();

        isLoading = false;
    }

    // Reinicio de nivel (muerte o botón del menú de pausa)
    public void RestartCurrentLevel()
    {
        if (isLoading || string.IsNullOrEmpty(currentLevelBaseName)) return;
        StartCoroutine(RestartCurrentLevelRoutine());
    }

    private IEnumerator RestartCurrentLevelRoutine()
    {
        isLoading = true;

        // Cerrar menú de pausa si está abierto
        if (PauseMenu.Instance != null)
            PauseMenu.Instance.ClosePauseMenuImmediate();

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        yield return UnloadLevelPair(currentLevelBaseName);

        // Restaurar vida con la que se empezó el nivel + estado
        if (PlayerHealth != null)
        {
            PlayerHealth.SetHealth(levelStartHealth);
            PlayerHealth.ResetDeathState();
        }

        // Resetear llaves del nivel
        ClearAllKeys();

        yield return LoadLevelPair(currentLevelBaseName);

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeIn();

        isLoading = false;
    }

    // Volver al menú principal = reinicio completo del juego
    public void ReturnToMainMenu()
    {
        if (isLoading) return;
        StartCoroutine(ReturnToMainMenuRoutine());
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        isLoading = true;

        if (PauseMenu.Instance != null)
            PauseMenu.Instance.ClosePauseMenuImmediate();

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        // Descargar todo excepto Core
        yield return UnloadAllExceptCore();

        // Resetear progreso
        currentLevelIndex = -1;
        currentLevelBaseName = null;

        // Apagar Player + HUD y restaurar vida
        DeactivatePlayerAndHUD();

        if (PlayerHealth != null)
        {
            PlayerHealth.FullHeal();
            PlayerHealth.ResetDeathState();
        }

        // Resetear todas las llaves
        ClearAllKeys();

        yield return SceneManager.LoadSceneAsync(mainMenuScene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(mainMenuScene));

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeIn();

        isLoading = false;
    }

    // ==================== MÉTODOS DE CARGA / DESCARGA ====================

    private IEnumerator LoadLevelPair(string baseName)
    {
        string logicName = baseName + "_Logic";
        string artName = baseName + "_Art";

        yield return SceneManager.LoadSceneAsync(logicName, LoadSceneMode.Additive);
        yield return SceneManager.LoadSceneAsync(artName, LoadSceneMode.Additive);

        Scene logicScene = SceneManager.GetSceneByName(logicName);
        if (logicScene.IsValid())
            SceneManager.SetActiveScene(logicScene);

        // Guardar la vida con la que se empieza este nivel
        if (PlayerHealth != null)
        {
            levelStartHealth = PlayerHealth.GetCurrentHealth();
        }

        // Posicionar al Player (todavía puede estar apagado)
        PlacePlayerAtSpawn();

        // Activar Player + HUD
        ActivatePlayerAndHUD();

        // Snap de cámara (opcional – descomentar si se nota el viaje de la cámara)
        // var camFollow = FindFirstObjectByType<CameraFollow>();
        // if (camFollow != null) camFollow.SnapToTarget();
    }

    private IEnumerator UnloadLevelPair(string baseName)
    {
        string logicName = baseName + "_Logic";
        string artName = baseName + "_Art";

        yield return UnloadSceneIfLoaded(logicName);
        yield return UnloadSceneIfLoaded(artName);
    }

    private IEnumerator UnloadSceneIfLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(scene);
        }
    }

    private IEnumerator UnloadAllExceptCore()
    {
        var scenesToUnload = new List<string>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != "Core" && scene.name != "Boot")
            {
                scenesToUnload.Add(scene.name);
            }
        }

        foreach (string sceneName in scenesToUnload)
        {
            yield return UnloadSceneIfLoaded(sceneName);
        }
    }

    private void PlacePlayerAtSpawn()
    {
        if (Player == null) return;

        LevelEntryPoint entry = FindFirstObjectByType<LevelEntryPoint>();
        if (entry != null)
        {
            Player.transform.position = entry.SpawnTransform.position;
        }
        else
        {
            Debug.LogWarning("No se encontró LevelEntryPoint en el nivel actual.");
        }
    }
}