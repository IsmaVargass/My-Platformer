using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class PlayFromMenu
{
    static PlayFromMenu()
    {
        // Ruta de la escena del menú
        string menuPath = "Assets/Scenes/Menu.unity";
        
        // Cargamos el asset de la escena
        SceneAsset menuScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(menuPath);

        if (menuScene != null)
        {
            // Forzamos a Unity a que use siempre esta escena al darle al Play en el editor
            EditorSceneManager.playModeStartScene = menuScene;
            Debug.Log("<color=green>Punto de inicio configurado automáticamente: " + menuPath + "</color>");
        }
        else
        {
            Debug.LogWarning("No se encontró la escena de Menú en: " + menuPath);
        }
    }
}
