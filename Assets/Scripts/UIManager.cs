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

    private void Awake()
    {
        instance = this;
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
            if(playerController.controlmode == Controls.mobile)
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
}
