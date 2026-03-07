using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitTrigger : MonoBehaviour
{
    //public Animator anim;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log($"[ExitTrigger] Jugador intenta salir. ¿Coleccionó todo? {GameManager.instance.HasCollectedAllCoins()}");
            
            if (GameManager.instance.HasCollectedAllCoins())
            {
                StartCoroutine("LevelExit");
            }
            else
            {
                Debug.Log($"[ExitTrigger] BLOQUEADO: Faltan monedas. Llamando a aviso UI...");
                UIManager.instance.ShowCoinWarning();
            }
        }
    }

    IEnumerator LevelExit()
    {
        //anim.SetTrigger("Exit");
        yield return new WaitForSeconds(0.1f);

        UIManager.instance.fadeToBlack = true;

        yield return new WaitForSeconds(2f);
        // Do something after flag anim
        GameManager.instance.LevelComplete();
    }
}
