using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string loadingScene = "Loading";
    public string level01Scene = "Level_01";
    public string level02Scene = "Level_02";
    public string levelCompleteScene = "LevelComplete";
    public string gameOverScene = "GameOver";
    public string endingScene = "Ending";

    private string sceneToLoad;

    private void Awake()
    {
        // Singleton
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
        // Si estamos en la escena Boot, vamos al Main Menu
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
        sceneToLoad = level01Scene;
        SceneManager.LoadScene(loadingScene);
    }

    public void LoadNextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == level01Scene)
        {
            sceneToLoad = level02Scene;
            SceneManager.LoadScene(loadingScene);
        }
        else if (currentScene == level02Scene)
        {
            SceneManager.LoadScene(endingScene);
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
        SceneManager.LoadScene(levelCompleteScene);
    }

    // Este método lo llama la pantalla de Loading
    public string GetSceneToLoad()
    {
        return sceneToLoad;
    }
}