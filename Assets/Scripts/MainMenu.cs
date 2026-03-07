using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject controlsPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseControls();
        }
    }

    // Carga el nivel principal por su nombre exacto
    public void PlayGame()
    {
        if (controlsPanel != null && controlsPanel.activeSelf) return;
        SceneManager.LoadScene("Level Recuperado");
    }

    public void OpenControls()
    {
        if (controlsPanel != null && controlsPanel.activeSelf) return;
        
        if (controlsPanel != null) 
            controlsPanel.SetActive(true);
        else
            Debug.LogError("MainMenu: No se ha asignado el 'controlsPanel' en el inspector.");
    }

    public void CloseControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    // Cierra el juego
    public void QuitGame()
    {
        if (controlsPanel != null && controlsPanel.activeSelf) return;

        Debug.Log("Saliendo del juego...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
