using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject controlsPanel;

    // Carga el nivel principal por su nombre exacto
    public void PlayGame()
    {
        SceneManager.LoadScene("Level Recuperado");
    }

    public void OpenControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    // Cierra el juego
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
