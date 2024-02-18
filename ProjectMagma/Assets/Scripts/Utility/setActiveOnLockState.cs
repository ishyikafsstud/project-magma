using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setActiveOnLockState : MonoBehaviour, ILockable
{
    [Tooltip("If true, activates the game object on lock and deactives on unlock. Setting to " +
        "false has the opposite effect.")]
    [SerializeField] private bool isActiveOnLock = true;
    public void SetLock(bool lockValue)
    {
        if (lockValue)
            Lock();
        else
            Unlock();
    }

    public void Lock()
    {
        gameObject.SetActive(isActiveOnLock);
    }

    public void Unlock()
    {
        gameObject.SetActive(!isActiveOnLock);
    }
}
