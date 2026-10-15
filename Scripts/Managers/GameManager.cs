using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string loadingScene = "Loading";
    public string tutorialScene = "Tutorial";          // ← NUEVO
    public string level01Scene = "Level_01";
    public string level02Scene = "Level_02";
    public string level03Scene = "Level_03";
    public string levelCompleteScene = "LevelComplete";
    public string gameOverScene = "GameOver";
    public string endingScene = "Ending";

    private string sceneToLoad;
    private string lastCompletedLevel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Boot")
        {
            LoadMainMenu();
        }
    }

    // ---------- MÉTODOS PÚBLICOS ----------

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void StartGame()
    {
        // Primero va al Tutorial
        sceneToLoad = tutorialScene;
        SceneManager.LoadScene(loadingScene);
    }

    /// <summary>
    /// Llamado desde la escena Tutorial cuando termina el timer.
    /// </summary>
    public void FinishTutorial()
    {
        sceneToLoad = level01Scene;
        SceneManager.LoadScene(loadingScene);
    }

    public void CompleteCurrentLevel()
    {
        lastCompletedLevel = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(levelCompleteScene);
    }

    public void LoadNextLevel()
    {
        if (lastCompletedLevel == level01Scene)
        {
            sceneToLoad = level02Scene;
            SceneManager.LoadScene(loadingScene);
        }
        else if (lastCompletedLevel == level02Scene)
        {
            sceneToLoad = level03Scene;
            SceneManager.LoadScene(loadingScene);
        }
        else if (lastCompletedLevel == level03Scene)
        {
            SceneManager.LoadScene(endingScene);
        }
        else
        {
            Debug.LogWarning("LoadNextLevel: lastCompletedLevel desconocido → MainMenu");
            LoadMainMenu();
        }
    }

    public void RestartLevel()
    {
        sceneToLoad = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(loadingScene);
    }

    public void LoadGameOver()
    {
        SceneManager.LoadScene(gameOverScene);
    }

    public void LoadLevelComplete()
    {
        CompleteCurrentLevel();
    }

    public string GetSceneToLoad()
    {
        return sceneToLoad;
    }
}