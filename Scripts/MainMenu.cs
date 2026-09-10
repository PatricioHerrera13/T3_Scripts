using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void OnPlayButton()
    {
        GameManager.Instance.StartGame();
    }

    public void OnQuitButton()
    {
        Debug.Log("Salir del juego");
        Application.Quit();

        // Esto es para que funcione también en el Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}