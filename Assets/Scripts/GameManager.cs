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
    public TMP_Text currentLevelTimeText;
    public TMP_Text bestLevelTimeText;

    [Header("Timer HUD")]
    public TMP_Text timerHUDText;
    private float timerTime = 0f;
    private bool timerRunning = false;
    private float bestTime = float.MaxValue;

    [Header("Death Menu")]
    public GameObject deathMenuPanel;

    [Header("Tutorial / Instructions")]
    public GameObject tutorialPanel;

    private int totalCoins = 0;

    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        UpdateGUI();
        if (UIManager.instance != null) UIManager.instance.fadeFromBlack = true;

        if (playerController == null) playerController = Object.FindFirstObjectByType<PlayerController>();
        
        if (playerController != null)
        {
            playerPosition = playerController.transform.position;
        }

        FindTotalPickups();

        // Inicializar Cronómetro
        timerTime = 0f;
        timerRunning = false; // No empezar todavía
        
        // Cargar mejor tiempo
        string sceneName = SceneManager.GetActiveScene().name;
        bestTime = PlayerPrefs.HasKey(sceneName + "_BestTime") ? PlayerPrefs.GetFloat(sceneName + "_BestTime") : float.MaxValue;

        // Mostrar Instrucciones al inicio
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            Time.timeScale = 0f;
            
            // Asegurar que el cursor sea visible para poder clickear el botón
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            Debug.Log("[GameManager] Iniciando con instrucciones. Juego pausado.");
        }
        else
        {
            timerRunning = true;
            Time.timeScale = 1f;

            // Asegurar que el cursor desaparezca al empezar sin tutorial
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void StartGame()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        Time.timeScale = 1f;
        timerRunning = true;

        // Ocultar el cursor para jugar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
        
        Debug.Log("[GameManager] Instrucciones aceptadas. Empezando cronometro.");
    }

    private void Update()
    {
        // FORZAR CURSOR DE FORMA CONSTANTE
        EnforceCursorState();

        if (tutorialPanel != null && tutorialPanel.activeSelf)
        {
            // Detectar click en cualquier lado de la pantalla o la tecla Enter para empezar
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
            {
                StartGame();
            }
            return;
        }

        if (timerRunning)
        {
            timerTime += Time.deltaTime;
            UpdateTimerHUD();
        }
    }

    private void EnforceCursorState()
    {
        // Revisamos si ALGÚN menú está abierto (Pausa, Muerte, Tutorial, Nivel Completado)
        bool anyMenuOpen = false;

        if (tutorialPanel != null && tutorialPanel.activeSelf) anyMenuOpen = true;
        if (deathMenuPanel != null && deathMenuPanel.activeSelf) anyMenuOpen = true;
        if (levelCompletePanel != null && levelCompletePanel.activeSelf) anyMenuOpen = true;
        
        if (UIManager.instance != null && UIManager.instance.pauseMenuPanel != null && UIManager.instance.pauseMenuPanel.activeSelf) 
            anyMenuOpen = true;

        if (anyMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (timerRunning) // Solo si estamos jugando activamente
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void UpdateTimerHUD()
    {
        if (timerHUDText != null)
        {
            timerHUDText.text = FormatTime(timerTime);
        }
    }

    public string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);
        int milliseconds = Mathf.FloorToInt((time * 100F) % 100F);
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    public void IncrementCoinCount()
    {
        coinCount++;
        Debug.Log($"[GameManager] Moneda recogida! Total: {coinCount} / {totalCoins}");
        UpdateGUI();
    }
    public void IncrementGemCount()
    {
        gemCount++;
        UpdateGUI();
    }

    public bool HasCollectedAllCoins()
    {
        bool hasAll = coinCount >= totalCoins;
        Debug.Log($"[GameManager] ¿Tiene todas las monedas? {hasAll} ({coinCount} / {totalCoins})");
        return hasAll;
    }

    private void UpdateGUI()
    {
        if (coinText != null)
        {
            coinText.text = coinCount.ToString();
        }
    }

    public void Death()
    {
        if (!isGameOver)
        {
            // Detener el cronómetro al morir
            timerRunning = false;
            
            // Liberar y mostrar cursor INMEDIATAMENTE al morir (antes de que termine el fade a negro)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

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
        totalCoins = 0; // RESETEAR para evitar duplicados si se llama varias veces
        pickup[] pickups = Object.FindObjectsByType<pickup>(FindObjectsSortMode.None);

        foreach (pickup pickupObject in pickups)
        {
            if (pickupObject.pt == pickup.pickupType.coin)
            {
                totalCoins += 1;
            }
        }
        Debug.Log($"[GameManager] Monedas encontradas en el nivel: {totalCoins}");
    }

    public void LevelComplete()
    {
        Debug.Log($"[GameManager] ¡Nivel completado detectado! Monedas: {coinCount}/{totalCoins}, Tiempo: {timerTime}");
        timerRunning = false;
        
        // Calcular récord local
        string sceneName = SceneManager.GetActiveScene().name;
        bool isNewRecord = false;
        
        if (timerTime < bestTime)
        {
            bestTime = timerTime;
            PlayerPrefs.SetFloat(sceneName + "_BestTime", bestTime);
            PlayerPrefs.Save();
            isNewRecord = true;
        }

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (leveCompletePanelTitle != null) 
                leveCompletePanelTitle.text = isNewRecord ? "¡NUEVO RÉCORD!" : "¡ENHORABUENA!";

            if (levelCompleteCoins != null)
                levelCompleteCoins.text = "MONEDAS: " + coinCount.ToString() + " / " + totalCoins.ToString();
            
            if (currentLevelTimeText != null)
                currentLevelTimeText.text = "TIEMPO: " + FormatTime(timerTime);
            
            if (bestLevelTimeText != null)
                bestLevelTimeText.text = "RECORD: " + FormatTime(bestTime);
        }
    }
   
    public IEnumerator DeathCoroutine()
    {
        // Esperamos a que el fade a negro termine o casi termine
        yield return new WaitForSeconds(1.5f);

        if (deathMenuPanel != null)
        {
            deathMenuPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Time.timeScale = 1f;
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
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
