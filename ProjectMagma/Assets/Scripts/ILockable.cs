using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILockable
{
    void SetLock(bool lockValue);

    void Lock();

    void Unlock();
}
