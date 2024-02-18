using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class portalKey : pickableItemBase
{
    public override void Pickup()
    {
        gameManager.instance.keyPicked();
        Destroy(gameObject);
    }
}
