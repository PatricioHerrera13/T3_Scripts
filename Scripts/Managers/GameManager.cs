using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Reference")]
    public PlayerMovement Player;               // Se asigna en Core o se busca automáticamente
    public PlayerHealth PlayerHealth;           // Referencia rápida

    [Header("Scene Names - Flow")]
    public string mainMenuScene = "MainMenu";
    public string loadingScene = "Loading";
    public string tutorialScene = "Tutorial";
    public string levelCompleteScene = "LevelComplete";
    public string gameOverScene = "GameOver";
    public string endingScene = "Ending";

    [Header("Level Order (base names)")]
    public string[] levelOrder = { "Level_01", "Level_02", "Level_03" };

    // Estado interno
    private int currentLevelIndex = -1;
    private string currentLevelBaseName;
    private bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Ya no necesitamos DontDestroyOnLoad porque vivirá en Core
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Buscar referencias del Player si no están asignadas
        if (Player == null)
            Player = FindFirstObjectByType<PlayerMovement>();

        if (PlayerHealth == null && Player != null)
            PlayerHealth = Player.GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        // Si arrancamos desde Boot, cargamos Core y luego MainMenu
        if (SceneManager.GetActiveScene().name == "Boot")
        {
            StartCoroutine(BootSequence());
        }
    }

    // ==================== FLUJO PRINCIPAL ====================

    private IEnumerator BootSequence()
    {
        // Cargar Core (persistente)
        yield return SceneManager.LoadSceneAsync("Core", LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Core"));

        // Ahora sí cargar MainMenu
        yield return SceneManager.LoadSceneAsync(mainMenuScene, LoadSceneMode.Additive);
    }

    public void StartGame()
    {
        if (isLoading) return;
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        isLoading = true;

        // Descargar MainMenu si está cargado
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

        // Cargar primer nivel
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

        // Descargar el nivel actual
        if (!string.IsNullOrEmpty(currentLevelBaseName))
            yield return UnloadLevelPair(currentLevelBaseName);

        // Cargar pantalla de LevelComplete
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
            // Terminó el juego
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

    public void RestartCurrentLevel()
    {
        if (isLoading || string.IsNullOrEmpty(currentLevelBaseName)) return;
        StartCoroutine(RestartCurrentLevelRoutine());
    }

    private IEnumerator RestartCurrentLevelRoutine()
    {
        isLoading = true;

        // Descargar nivel actual
        yield return UnloadLevelPair(currentLevelBaseName);

        // Restaurar vida completa + estado
        if (PlayerHealth != null)
        {
            PlayerHealth.FullHeal();
            PlayerHealth.ResetDeathState();   // ← esta línea es la importante
        }

        // Volver a cargar el mismo nivel
        yield return LoadLevelPair(currentLevelBaseName);

        isLoading = false;
    }

    public void ReturnToMainMenu()
    {
        if (isLoading) return;
        StartCoroutine(ReturnToMainMenuRoutine());
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        isLoading = true;

        // Descargar todo excepto Core
        yield return UnloadAllExceptCore();

        yield return SceneManager.LoadSceneAsync(mainMenuScene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(mainMenuScene));

        isLoading = false;
    }

    // ==================== MÉTODOS DE CARGA / DESCARGA ====================

    private IEnumerator LoadLevelPair(string baseName)
    {
        string logicName = baseName + "_Logic";
        string artName = baseName + "_Art";

        // Cargar Logic
        yield return SceneManager.LoadSceneAsync(logicName, LoadSceneMode.Additive);

        // Cargar Art
        yield return SceneManager.LoadSceneAsync(artName, LoadSceneMode.Additive);

        // Poner la de Logic como activa
        Scene logicScene = SceneManager.GetSceneByName(logicName);
        if (logicScene.IsValid())
            SceneManager.SetActiveScene(logicScene);

        // Posicionar al Player
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
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != "Core" && scene.name != "Boot")
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }
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

    // ==================== HELPERS ====================

    public void LoadGameOver()
    {
        // Por ahora lo dejamos, pero el flujo principal usa RestartCurrentLevel
        SceneManager.LoadScene(gameOverScene);
    }
}