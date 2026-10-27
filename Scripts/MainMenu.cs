using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button firstSelectedButton;

    private void OnEnable()
    {
        StartCoroutine(SetFirstSelectedNextFrame());
    }

    private System.Collections.IEnumerator SetFirstSelectedNextFrame()
    {
        yield return null;

        if (firstSelectedButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }

    public void OnPlayButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    public void OnQuitButton()
    {
        Debug.Log("Salir del juego");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}