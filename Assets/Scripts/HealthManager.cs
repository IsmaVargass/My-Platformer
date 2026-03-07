using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public static HealthManager instance;
    public GameObject damageEffect;

    private int MaxHealth = 6;
    public int currentHealth;

    public Image[] hearts;
    public Sprite FullHeartSprite;
    public Sprite HalfHeartSprite;
    public Sprite EmptyHeartSprite;

    private GameObject Player;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Búsqueda más robusta del jugador
        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc == null) pc = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        
        if (pc != null)
        {
            Player = pc.gameObject;
        }
        else
        {
            Debug.LogWarning("HealthManager: Esperando al PlayerController...");
            // Intentar buscar de nuevo en un segundo si falla (opcional, pero ayuda en escenas pesadas)
            Invoke("FindPlayer", 0.5f);
        }
        
        currentHealth = MaxHealth;
        DisplayHearts();
    }

    private void FindPlayer()
    {
        if (Player != null) return;
        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null) Player = pc.gameObject;
    }
   
  

    public void HurtPlayer()
    {
        if (currentHealth > 0)
        {
            // Restamos 2 para quitar un corazón completo (o 1 para medio corazón)
            // Según tu lógica original, currentHealth=6 son 3 corazones.
            currentHealth -= 2; 
            if (currentHealth < 0) currentHealth = 0;
            
            DisplayHearts();

            // Llamamos al efecto visual de parpadeo rojo
            if (UIManager.instance != null)
            {
                UIManager.instance.TriggerDamageFlash();
            }
        }

        if (currentHealth <= 0)
        {
            GameManager.instance.Death();
        }
        
        if (damageEffect != null)
            Instantiate(damageEffect, Player.transform.position, Quaternion.identity);

        // Reproducir sonido de daño
        PlayerController pc = Player.GetComponent<PlayerController>();
        if (pc != null && pc.hurtSound != null)
        {
            pc.hurtSound.Play();
        }
    }

    public void DisplayHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            // Cada corazón representa 2 unidades de vida (0 y 1 para el primero, 2 y 3 para el segundo, etc.)
            // heartIndex * 2 es el umbral para media vida, +1 es para vida completa.
            int heartThreshold = i * 2;

            if (currentHealth >= heartThreshold + 2)
            {
                // Vida suficiente para corazón lleno
                hearts[i].sprite = FullHeartSprite;
            }
            else if (currentHealth >= heartThreshold + 1)
            {
                // Vida suficiente solo para medio corazón
                hearts[i].sprite = HalfHeartSprite;
            }
            else
            {
                // Sin vida para este corazón
                hearts[i].sprite = EmptyHeartSprite;
            }
        }
    }

    

}
