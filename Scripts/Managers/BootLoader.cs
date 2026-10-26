using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BootLoader : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string coreSceneName = "Core";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        StartCoroutine(LoadCoreAndMenu());
    }

    private IEnumerator LoadCoreAndMenu()
    {
        // 1. Cargar Core (Additive)
        yield return SceneManager.LoadSceneAsync(coreSceneName, LoadSceneMode.Additive);

        // 2. Poner Core como escena activa
        Scene coreScene = SceneManager.GetSceneByName(coreSceneName);
        if (coreScene.IsValid())
            SceneManager.SetActiveScene(coreScene);

        // 3. Cargar MainMenu (Additive)
        yield return SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Additive);

        // 4. (Opcional) Descargar Boot si querés dejarla limpia
        // SceneManager.UnloadSceneAsync("Boot");
    }
}