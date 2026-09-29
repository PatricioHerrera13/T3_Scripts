using UnityEngine;
using System.Collections;

public class LevelCompleteManager : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float delayBeforeNextLevel = 2.5f;  // Tiempo que se muestra la pantalla

    private void Start()
    {
        StartCoroutine(GoToNextLevelAfterDelay());
    }

    private IEnumerator GoToNextLevelAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeNextLevel);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadNextLevel();
        }
        else
        {
            Debug.LogError("LevelCompleteManager: No se encontró GameManager.Instance");
        }
    }
}