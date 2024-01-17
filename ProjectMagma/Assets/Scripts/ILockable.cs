using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILockable
{
    void ToggleLock();

    void Lock();

    void Unlock();
}
