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
    public GameObject PlayerGameObject;
    public GameObject GameplayHUD;

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

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        yield return UnloadSceneIfLoaded(mainMenuScene);

        yield return SceneManager.LoadSceneAsync(tutorialScene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(tutorialScene));

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

    public void RestartCurrentLevel()
    {
        if (isLoading || string.IsNullOrEmpty(currentLevelBaseName)) return;
        StartCoroutine(RestartCurrentLevelRoutine());
    }

    private IEnumerator RestartCurrentLevelRoutine()
    {
        isLoading = true;

        if (PauseMenu.Instance != null)
            PauseMenu.Instance.ClosePauseMenuImmediate();

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        yield return UnloadLevelPair(currentLevelBaseName);

        if (PlayerHealth != null)
        {
            PlayerHealth.SetHealth(levelStartHealth);
            PlayerHealth.ResetDeathState();
        }

        ClearAllKeys();

        // === SCORE ===
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetLevelScore();
        // =============

        yield return LoadLevelPair(currentLevelBaseName);

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeIn();

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

        if (PauseMenu.Instance != null)
            PauseMenu.Instance.ClosePauseMenuImmediate();

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        yield return UnloadAllExceptCore();

        currentLevelIndex = -1;
        currentLevelBaseName = null;

        DeactivatePlayerAndHUD();

        if (PlayerHealth != null)
        {
            PlayerHealth.FullHeal();
            PlayerHealth.ResetDeathState();
        }

        ClearAllKeys();

        // === SCORE ===
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetAllScores();
        // =============

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

        if (PlayerHealth != null)
        {
            levelStartHealth = PlayerHealth.GetCurrentHealth();
        }

        // === SCORE ===
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetLevelScore();
        // =============

        PlacePlayerAtSpawn();

        ActivatePlayerAndHUD();

        // Snap de cámara
        if (CameraFollow.Instance != null)
            CameraFollow.Instance.SnapToTarget();
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