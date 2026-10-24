using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private PlayerHealth playerHealth;   // Opcional: si lo dejas vacío lo busca solo

    [Header("Options")]
    [SerializeField] private bool setMaxValueFromPlayer = true;  // Pone el Max Value del Slider = maxHealth del player

    private void Awake()
    {
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += UpdateBar;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateBar;
    }

    private void Start()
    {
        // Por si el evento se disparó antes de que este script estuviera listo
        if (playerHealth != null)
        {
            UpdateBar(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        }
    }

    private void UpdateBar(int current, int max)
    {
        if (healthSlider == null) return;

        if (setMaxValueFromPlayer)
            healthSlider.maxValue = max;

        healthSlider.value = current;
    }
}