using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Carga el primer nivel (asegúrate de que sea el índice 1 en Build Settings)
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    // Cierra el juego
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
