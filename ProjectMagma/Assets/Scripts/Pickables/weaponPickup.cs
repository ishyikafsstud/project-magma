using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponPickup : pickableItemBase
{
    [SerializeField] weaponStats weapon;

    public override void Pickup()
    {
        gameManager.instance.playerScript.pickupWeapon(weapon);
        base.Pickup();
    }
}
