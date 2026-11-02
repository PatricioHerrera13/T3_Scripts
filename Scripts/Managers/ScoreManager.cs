using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool logScoreChanges = true;

    public int CurrentLevelScore { get; private set; }
    public int TotalScore { get; private set; }

    public event Action<int> OnLevelScoreChanged;
    public event Action<int> OnTotalScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddScore(int points)
    {
        if (points <= 0) return;

        CurrentLevelScore += points;
        TotalScore += points;

        if (logScoreChanges)
            Debug.Log($"<color=green>+{points}</color> | Nivel: {CurrentLevelScore} | Total: {TotalScore}");

        OnLevelScoreChanged?.Invoke(CurrentLevelScore);
        OnTotalScoreChanged?.Invoke(TotalScore);
    }

    public void ResetLevelScore()
    {
        CurrentLevelScore = 0;
        OnLevelScoreChanged?.Invoke(0);

        if (logScoreChanges)
            Debug.Log("Score del nivel reseteado a 0");
    }

    public void ResetAllScores()
    {
        CurrentLevelScore = 0;
        TotalScore = 0;
        OnLevelScoreChanged?.Invoke(0);
        OnTotalScoreChanged?.Invoke(0);
    }
}