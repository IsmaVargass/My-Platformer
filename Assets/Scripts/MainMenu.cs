using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject controlsPanel;

    private void Start()
    {
        // Asegurar que el cursor es visible en el menú principal
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        Debug.Log("MainMenu Update is RUNNING!");
        // Forzamos el cursor cada frame por si hay un bug en Unity o algo más lo está ocultando
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
