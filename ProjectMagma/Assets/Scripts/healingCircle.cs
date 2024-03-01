using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healingCircle : MonoBehaviour
{
    [SerializeField] int healingRate;// Healing rate per second
    private bool isHealing = false;

    private void Update()
    {
        if (isHealing)
        {
            if (gameManager.instance.player != null)
            {
                // Heal the player over time
                float healingAmount = healingRate * Time.deltaTime;
                gameManager.instance.playerScript.Heal(healingAmount);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isHealing = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isHealing = false;
        }
    }
}
