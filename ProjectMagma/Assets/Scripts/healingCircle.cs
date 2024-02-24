using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healingCircle : MonoBehaviour
{
    [SerializeField] int healingRate;// Healing rate per second
    public playerController playerController;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (gameManager.instance.player != null)
            {
                
                if (playerController == null)
                {
                    playerController = gameManager.instance.player.GetComponent<playerController>();
                }

                // Heal the player over time
                int healingAmount = Mathf.RoundToInt(healingRate * Time.deltaTime);
                playerController.Heal(healingAmount);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameManager.instance.player != null)
            {

                if (playerController == null)
                {
                    playerController = gameManager.instance.player.GetComponent<playerController>();
                }

                // Heal the player over time
                int healingAmount = Mathf.RoundToInt(healingRate * Time.deltaTime);
                playerController.Heal(healingAmount);
            }
        }
    }
}
