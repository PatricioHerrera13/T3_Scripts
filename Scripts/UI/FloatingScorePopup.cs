using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingScorePopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI scoreText; // o TextMeshPro si es World Space

    [Header("Animation")]
    [SerializeField] private float floatSpeed = 1.8f;
    [SerializeField] private float lifetime = 0.9f;
    [SerializeField] private float fadeStartTime = 0.4f;

    private float timer;
    private Color originalColor;
    private Vector3 startPosition;

    private void Awake()
    {
        if (scoreText == null)
            scoreText = GetComponentInChildren<TextMeshProUGUI>();

        if (scoreText != null)
            originalColor = scoreText.color;

        startPosition = transform.position;
    }

    public void Setup(int points)
    {
        if (scoreText != null)
            scoreText.text = $"+{points}";

        timer = 0f;
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        while (timer < lifetime)
        {
            timer += Time.deltaTime;

            // Sube
            transform.position = startPosition + Vector3.up * (floatSpeed * timer);

            // Fade out
            if (timer > fadeStartTime && scoreText != null)
            {
                float t = (timer - fadeStartTime) / (lifetime - fadeStartTime);
                Color c = originalColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                scoreText.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}