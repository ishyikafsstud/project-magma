using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class portalKey : MonoBehaviour, IPickable
{
    public void Pickup()
    {
        gameManager.instance.keyPicked();
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
