using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamage 
{
    float GetHealth();
    float GetOriginalHealth();

    void takeDamage(int amount);

    /// <summary>
    /// Apply a number of Freeze effect stacks.
    /// </summary>
    /// <param name="stacks">The number of Freeze effect stacks.</param>
    IEnumerator ApplyFreeze(int stacks);
}
