using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamage 
{
    float GetHealth();
    float GetOriginalHealth();

    void takeDamage(int amount);
}
