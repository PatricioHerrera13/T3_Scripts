using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI (opcional)")]
    [SerializeField] private Slider progressBar;      // Opcional
    [SerializeField] private Text progressText;       // Opcional

    private void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        string sceneName = GameManager.Instance.GetSceneToLoad();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
                progressBar.value = progress;

            if (progressText != null)
                progressText.text = (progress * 100f).ToString("F0") + "%";

            // Cuando llega al 90% (casi cargado), esperamos un poquito y activamos
            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f); // Tiempo mínimo de pantalla de carga
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}