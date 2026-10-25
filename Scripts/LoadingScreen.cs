using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI (opcional)")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;

    private void Start()
    {
        Debug.LogWarning("LoadingScreen está presente pero no se usa en el flujo actual (Additive).");
    }
}