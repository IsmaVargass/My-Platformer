using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickup : MonoBehaviour
{
    public enum pickupType { coin,gem,health}

    public pickupType pt;
    [SerializeField] GameObject PickupEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(pt == pickupType.coin)
        {
            if(collision.gameObject.tag == "Player")
            {
                GameManager.instance.IncrementCoinCount();
           
                Instantiate(PickupEffect, transform.position, Quaternion.identity);

                // Play coin sound from player's AudioSource before destroying
                // Usamos GetComponentInParent por si el collider está en un objeto hijo
                PlayerController player = collision.GetComponentInParent<PlayerController>();
                if (player != null && player.coinSound != null)
                {
                    player.coinSound.Play();
                }

                Destroy(this.gameObject,0.1f);
                
            }
            
        }

        if (pt == pickupType.gem)
        {
            if (collision.gameObject.tag == "Player")
            {
                GameManager.instance.IncrementGemCount();
            
                Instantiate(PickupEffect, transform.position, Quaternion.identity);

                // Play coin/gem sound from player's AudioSource before destroying
                PlayerController player = collision.GetComponentInParent<PlayerController>();
                if (player != null && player.coinSound != null)
                {
                    player.coinSound.Play();
                }

                Destroy(this.gameObject, 0.1f);

            }

        }
    }
}
