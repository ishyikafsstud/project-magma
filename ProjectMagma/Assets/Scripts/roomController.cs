using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class roomController : MonoBehaviour
{
    public GameObject[] barriers;

    [SerializeField] private int enemyCount;
    private bool doorsLocked;

    public void DecrementEnemyCount(GameObject enemy)
    {
        enemyCount--;

        if (enemyCount <= 0)
            UnlockDoors();
    }

    public void LockDoors()
    {
        doorsLocked = true;
        foreach (GameObject barrier in barriers)
        {
            barrier.GetComponent<Animator>().SetTrigger("Door Locked");
        }
    }

    public void UnlockDoors()
    {
        doorsLocked = false;
        foreach (GameObject barrier in barriers)
        {
            barrier.GetComponent<Animator>().SetTrigger("Door Open");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name + " entered " + gameObject.name); // Print out the collider's name and the identifier of the room controller
        if (other.CompareTag("Player"))
        {
            if (!doorsLocked)
                LockDoors();
        }
        else if (other.CompareTag("Enemy"))
        {
            enemyCount++;

            other.GetComponent<enemyAI>().DeathEvent += DecrementEnemyCount;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            UnlockDoors();
    }
}
