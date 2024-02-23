using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ambushRewardItem : pickableItemBase
{
    public override void Pickup()
    {
        gameManager.instance.ambushRewardPicked();
        base.Pickup();
    }
}
