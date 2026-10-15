using UnityEngine;

public class TutorialTimer : MonoBehaviour
{
    [Header("Duración del Tutorial")]
    [SerializeField] private float duration = 10f;

    private float timer;
    private bool finished;

    private void Start()
    {
        timer = duration;
    }

    private void Update()
    {
        if (finished) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Finish();
        }
    }

    private void Finish()
    {
        if (finished) return;
        finished = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinishTutorial();
        }
        else
        {
            Debug.LogError("GameManager no encontrado. No se puede continuar al Level_01.");
        }
    }
}