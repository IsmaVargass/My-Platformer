using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;


    public TMP_Text coinText;

    public PlayerController playerController;

    private int coinCount = 0;
    private int gemCount = 0;
    private bool isGameOver = false;
    private Vector3 playerPosition;

    //Level Complete

    public GameObject levelCompletePanel;
    public TMP_Text leveCompletePanelTitle;
    public TMP_Text levelCompleteCoins;

    [Header("Death Menu")]
    public GameObject deathMenuPanel;




   
    private int totalCoins = 0;
  



    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        UpdateGUI();
        UIManager.instance.fadeFromBlack = true;
        playerPosition = playerController.transform.position;

        FindTotalPickups();
    }

    public void IncrementCoinCount()
    {
        coinCount++;
        UpdateGUI();
    }
    public void IncrementGemCount()
    {
        gemCount++;
        UpdateGUI();
    }

    public bool HasCollectedAllCoins()
    {
        return coinCount >= totalCoins;
    }

    private void UpdateGUI()
    {
        coinText.text = coinCount.ToString();
  
    }

    public void Death()
    {
        if (!isGameOver)
        {
            // Disable Mobile Controls
            UIManager.instance.DisableMobileControls();
            // Initiate screen fade
            UIManager.instance.fadeToBlack = true;

            // Disable the player object
            playerController.gameObject.SetActive(false);

            // Start death coroutine to wait and then respawn the player
            StartCoroutine(DeathCoroutine());

            // Update game state
            isGameOver = true;

            // Log death message
            Debug.Log("Died");
        }
    }
 
    public void FindTotalPickups()
    {

        pickup[] pickups = Object.FindObjectsByType<pickup>(FindObjectsSortMode.None);

        foreach (pickup pickupObject in pickups)
        {
            if (pickupObject.pt == pickup.pickupType.coin)
            {
                totalCoins += 1;
            }
           
        }


      
    }
    public void LevelComplete()
    {
       


        levelCompletePanel.SetActive(true);
        leveCompletePanelTitle.text = "LEVEL COMPLETE";



        levelCompleteCoins.text = "COINS COLLECTED: "+ coinCount.ToString() +" / " + totalCoins.ToString();
 
    }
   
    public IEnumerator DeathCoroutine()
    {
        // Esperamos a que el fade a negro termine o casi termine
        yield return new WaitForSeconds(1.5f);

        if (deathMenuPanel != null)
        {
            deathMenuPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Death Menu Panel no asignado en GameManager. Reiniciando nivel por defecto.");
            RestartLevel();
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void NextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels!");
            SceneManager.LoadScene(0); // Go to menu if no more levels
        }
    }
}
