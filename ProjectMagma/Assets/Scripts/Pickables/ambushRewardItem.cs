using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ambushRewardItem : pickableItemBase
{
    [Tooltip("Check if is a reward for defeating the ambush, uncheck if it is a pickable in a secret room.")]
    [SerializeField] bool isEarnedAmbushReward = true;

    public override void Pickup()
    {
        gameManager.instance.ambushRewardPicked(isEarnedAmbushReward);
        base.Pickup();
    }
}
