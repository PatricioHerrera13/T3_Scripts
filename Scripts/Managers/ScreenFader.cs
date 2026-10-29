using UnityEngine;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [Header("Referencias")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Configuración")]
    [SerializeField] private float fadeDuration = 0.35f;

    private bool isFading = false;

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

        // Empezamos completamente transparente
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Tapa la pantalla (va a negro)
    /// </summary>
    public IEnumerator FadeOut()
    {
        yield return Fade(1f);
    }

    /// <summary>
    /// Revela la pantalla (sale del negro)
    /// </summary>
    public IEnumerator FadeIn()
    {
        yield return Fade(0f);
    }

    /// <summary>
    /// Tapa la pantalla al instante (sin animación)
    /// </summary>
    public void FadeOutImmediate()
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Revela la pantalla al instante
    /// </summary>
    public void FadeInImmediate()
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (canvasGroup == null || isFading) yield break;

        isFading = true;
        canvasGroup.blocksRaycasts = true; // Bloqueamos input mientras se hace el fade

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime; // Usamos unscaled para que funcione aunque el juego esté pausado
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        // Solo dejamos pasar el input si terminamos transparentes
        canvasGroup.blocksRaycasts = targetAlpha > 0.9f;

        isFading = false;
    }
}