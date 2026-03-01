using UnityEngine;

public class Level2Builder : MonoBehaviour
{
    [Header("Configuración de Fondo")]
    public Sprite backgroundSprite;
    
    void Start()
    {
        // 1. Configurar el fondo si existe un SpriteRenderer de fondo
        GameObject bgObj = GameObject.Find("Background") ?? GameObject.Find("Fondo");
        if (bgObj != null)
        {
            SpriteRenderer sr = bgObj.GetComponent<SpriteRenderer>();
            if (sr != null && backgroundSprite != null)
            {
                sr.sprite = backgroundSprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(100, 100); // Tamaño grande para el fondo
            }
        }

        // 2. Asegurarse de que el jugador está en su sitio
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0, 0, 0); // O la posición inicial deseada
        }

        Debug.Log("Nivel 2 configurado automáticamente por Level2Builder");
    }
}
