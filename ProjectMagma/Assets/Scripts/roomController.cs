using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class roomController : MonoBehaviour
{
    public GameObject[] barriers;
  
    private int enemyCount;
    private bool doorsLocked;

    public void DecreaseEnemyCount(GameObject enemy)
    {
        enemyCount--;
        
        if(enemyCount <= 0)
        {
            UnlockDoors();
            doorsLocked = false;
        }
    }

    public void LockDoors()
    {
        foreach(GameObject barrier in barriers)
        {
            barrier.GetComponent<Animator>().SetTrigger("Door Locked");
        }
    }

    public void UnlockDoors()
    {
        foreach (GameObject barrier in barriers)
        {
            barrier.GetComponent<Animator>().SetTrigger("Door Open");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!doorsLocked)
            {
                LockDoors();
                doorsLocked = true;
            }
        }
        if(other.CompareTag("Enemy"))
        {
            enemyCount++;

            enemyAI.OnDied += DecreaseEnemyCount;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            UnlockDoors();
            doorsLocked = false;
        }
    }
}
