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
            quitButton.onClick.AddListener(QuitToMenu);
            
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

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        // Intentar cargar por nombre o volver a la escena 0 (que suele ser el menú)
        if (Application.CanStreamedLevelBeLoaded("Main Menu"))
            SceneManager.LoadScene("Main Menu");
        else
            SceneManager.LoadScene(0);

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
