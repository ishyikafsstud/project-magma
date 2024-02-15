using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ambushRewardItem : MonoBehaviour, IPickable
{
    public void Pickup()
    {
        gameManager.instance.ambushRewardPicked();
        Destroy(gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Pickup();
        }
    }
}
