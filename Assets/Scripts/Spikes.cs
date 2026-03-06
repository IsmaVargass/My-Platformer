using UnityEngine;

public class Spikes : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Comprobamos si lo que ha tocado el pincho es el jugador
        if (collision.CompareTag("Player"))
        {
            if (GameManager.instance != null)
            {
                // Llamamos directamente a la muerte del jugador
                GameManager.instance.Death();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Por si acaso el collider no es Trigger
        if (collision.gameObject.CompareTag("Player"))
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.Death();
            }
        }
    }
}
