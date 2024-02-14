using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponPickup : MonoBehaviour
{
    [SerializeField] weaponStats weapon;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        gameManager.instance.playerScript.pickupWeapon(weapon);
        Destroy(gameObject);
    }
}
