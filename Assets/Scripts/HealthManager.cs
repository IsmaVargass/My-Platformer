using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public static HealthManager instance;
    public GameObject damageEffect;

    private int MaxHealth = 6;
    public int currentHealth;

    [SerializeField] private Image[] hearts;
    [SerializeField] private Sprite FullHeartSprite;
    [SerializeField] private Sprite HalfHeartSprite;
    [SerializeField] private Sprite EmptyHeartSprite;

    private GameObject Player;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Player = Object.FindFirstObjectByType<PlayerController>().gameObject;
        currentHealth = MaxHealth;
        DisplayHearts();
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
    }

    public void DisplayHearts()
    {
        int fullHeartsCount = currentHealth / 2; // Calculate the number of full hearts
        bool hasHalfHeart = (currentHealth % 2) == 1; // Check if there's a half heart needed

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < fullHeartsCount)
            {
                // Heart should be full
                hearts[i].sprite = FullHeartSprite;
            }
            else if (hasHalfHeart && i == fullHeartsCount)
            {
                // Heart should be half
                hearts[i].sprite = HalfHeartSprite;
            }
            else
            {
                // Heart should be empty
                hearts[i].sprite = EmptyHeartSprite;
            }
        }
    }

    

}
