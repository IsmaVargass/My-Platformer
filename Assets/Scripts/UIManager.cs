using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject mobileControls;

    public bool fadeToBlack, fadeFromBlack;
    public Image blackScreen;
    public float fadeSpeed = 2f;

    //player reference

    public PlayerController playerController;
    public Slider jumpSlider;
    public GameObject coinWarning;

    [Header("Damage Effect")]
    public Image damageFlashImage;
    public float flashDuration = 0.2f;
    public Color flashColor = new Color(1, 0, 0, 0.4f);

    public GameObject pauseMenuPanel;
    public GameObject controlsPanel;

    private void Awake()
    {
        instance = this;
    }

    public void ResumeGame()
    {
        if (playerController != null)
        {
            playerController.TogglePause();
        }
    }

    public void TogglePauseMenu(bool show)
    {
        Debug.Log($"[UIManager] TogglePauseMenu: {show}");

        if (pauseMenuPanel == null)
        {
            Debug.LogWarning("[UIManager] pauseMenuPanel no asignado. Buscando...");
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Transform found = canvas.transform.Find("PauseMenu");
                if (found != null) pauseMenuPanel = found.gameObject;
            }
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(show);
            Debug.Log("[UIManager] PauseMenuPanel activado: " + show);
            
            // Si cerramos la pausa, cerramos TAMBIÉN los controles por si acaso
            if (!show && controlsPanel != null)
            {
                controlsPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("[UIManager] ¡ERROR! No se pudo encontrar el PauseMenuPanel en el Canvas.");
        }
    }

    public void ShowCoinWarning()
    {
        if (coinWarning != null)
        {
            StopCoroutine("HideCoinWarning");
            coinWarning.SetActive(true);
            StartCoroutine("HideCoinWarning");
        }
    }

    private IEnumerator HideCoinWarning()
    {
        yield return new WaitForSeconds(2.5f);
        if (coinWarning != null) coinWarning.SetActive(false);
    }

    public void DisableMobileControls()
    {
        mobileControls.SetActive(false);
    }
    public void EnableMobileControls()
    {
        mobileControls.SetActive(true);
    }

    private void Update()
    {
        UpdateFade();
    }

    private void UpdateFade()
    {
        if (fadeToBlack)
        {
            FadeToBlack();
        }
        else if (fadeFromBlack)
        {
            FadeFromBlack();
        }
    }

    private void FadeToBlack()
    {
        FadeScreen(1f);

        if (blackScreen.color.a >= 1f)
        {
            fadeToBlack = false;
        }
    }

    private void FadeFromBlack()
    {
        FadeScreen(0f);

        if (blackScreen.color.a <= 0f)
        {
            if(playerController != null && playerController.controlmode == Controls.mobile)
            {
                EnableMobileControls();
            }
            fadeFromBlack = false;
        }
    }

    private void FadeScreen(float targetAlpha)
    {
        Color currentColor = blackScreen.color;
        float newAlpha = Mathf.MoveTowards(currentColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
        blackScreen.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
    }

    public void TriggerDamageFlash()
    {
        if (damageFlashImage != null)
        {
            StopCoroutine("DamageFlashRoutine");
            StartCoroutine("DamageFlashRoutine");
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        damageFlashImage.color = flashColor;
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(flashColor.a, 0f, elapsed / flashDuration);
            damageFlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }
        damageFlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
    }
}
