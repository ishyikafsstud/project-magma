using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healingCircle : MonoBehaviour
{
    [SerializeField] int healingRate;// Healing rate per second
    public playerController playerController;
    private bool isHealing = false;

    private void Update()
    {
        if (isHealing)
        {
            if (gameManager.instance.player != null)
            {

                if (playerController == null)
                {
                    playerController = gameManager.instance.playerScript;
                }

                // Heal the player over time
                int healingAmount = Mathf.RoundToInt(healingRate * Time.deltaTime);
                playerController.Heal(healingAmount);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        isHealing = true;
    }

    private void OnTriggerExit(Collider other)
    {
        isHealing = false;
    }
}
