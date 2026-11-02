using UnityEngine;
using TMPro;
using System.Collections;

public class LevelCompleteManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI levelScoreText;
    [SerializeField] private TextMeshProUGUI totalScoreText;   // opcional
    [SerializeField] private GameObject scorePanel;

    [Header("Timing")]
    [SerializeField] private float displayDuration = 2f;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            if (levelScoreText != null)
                levelScoreText.text = ScoreManager.Instance.CurrentLevelScore.ToString("N0");

            if (totalScoreText != null)
                totalScoreText.text = ScoreManager.Instance.TotalScore.ToString("N0");
        }

        if (scorePanel != null)
            scorePanel.SetActive(true);

        StartCoroutine(GoToNextLevelAfterDelay());
    }

    private IEnumerator GoToNextLevelAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (GameManager.Instance != null)
            GameManager.Instance.LoadNextLevel();
        else
            Debug.LogError("LevelCompleteManager: No se encontró GameManager.Instance");
    }
}