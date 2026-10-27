using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Reference")]
    public PlayerMovement Player;
    public PlayerHealth PlayerHealth;

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

        if (Player == null)
            Player = FindFirstObjectByType<PlayerMovement>();

        if (PlayerHealth == null && Player != null)
            PlayerHealth = Player.GetComponent<PlayerHealth>();
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

        yield return UnloadSceneIfLoaded(mainMenuScene);

        // Cargar Tutorial
        yield return SceneManager.LoadSceneAsync(tutorialScene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(tutorialScene));

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

        yield return UnloadSceneIfLoaded(tutorialScene);

        currentLevelIndex = 0;
        currentLevelBaseName = levelOrder[0];
        yield return LoadLevelPair(currentLevelBaseName);

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

        if (!string.IsNullOrEmpty(currentLevelBaseName))
            yield return UnloadLevelPair(currentLevelBaseName);

        yield return SceneManager.LoadSceneAsync(levelCompleteScene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(levelCompleteScene));

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

        yield return UnloadSceneIfLoaded(levelCompleteScene);

        currentLevelIndex++;

        if (currentLevelIndex >= levelOrder.Length)
        {
            yield return SceneManager.LoadSceneAsync(endingScene, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(endingScene));
        }
        else
        {
            currentLevelBaseName = levelOrder[currentLevelIndex];
            yield return LoadLevelPair(currentLevelBaseName);
        }

        isLoading = false;
    }

    // Reinicio de nivel (muerte o botón del menú de pausa)
    // Restaura la vida que tenía al empezar el nivel
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

        yield return UnloadLevelPair(currentLevelBaseName);

        // Restaurar vida con la que se empezó el nivel + estado
        if (PlayerHealth != null)
        {
            PlayerHealth.SetHealth(levelStartHealth);
            PlayerHealth.ResetDeathState();
        }

        yield return LoadLevelPair(currentLevelBaseName);

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

        // Cerrar menú de pausa si está abierto
        if (PauseMenu.Instance != null)
            PauseMenu.Instance.ClosePauseMenuImmediate();

        // Descargar todo excepto Core
        yield return UnloadAllExceptCore();

        // Resetear progreso
        currentLevelIndex = -1;
        currentLevelBaseName = null;

        // Restaurar vida completa al volver al menú
        if (PlayerHealth != null)
        {
            PlayerHealth.FullHeal();
            PlayerHealth.ResetDeathState();
        }

        yield return SceneManager.LoadSceneAsync(mainMenuScene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(mainMenuScene));

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

        PlacePlayerAtSpawn();
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
        // Hacemos una copia de los nombres porque no se puede modificar mientras iteramos
        var scenesToUnload = new System.Collections.Generic.List<string>();

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