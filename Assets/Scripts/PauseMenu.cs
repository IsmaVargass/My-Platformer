using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button resumeButton;
    public Button controlsButton;
    public Button quitButton;

    [Header("Panels")]
    public GameObject controlsPanel;

    private void Start()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(OpenControls);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
            
        // Buscar el panel de controles si no está asignado
        if (controlsPanel == null)
        {
            Transform canvas = transform.parent;
            if (canvas != null)
            {
                Transform cp = canvas.Find("ControlsPanel");
                if (cp != null) controlsPanel = cp.gameObject;
            }
        }
    }

    public void Resume()
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.ResumeGame();
        }
    }

    public void OpenControls()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego desde el Menú de Pausa...");
        Time.timeScale = 1f;

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
