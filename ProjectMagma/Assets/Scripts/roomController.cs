using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class roomController : MonoBehaviour
{
    public GameObject[] barriers;

    [SerializeField] bool unlockDoorsOnStart;
    [SerializeField] bool unlockDoorsOnPlayerLeave;

    [SerializeField] private int enemyCount;
    private bool doorsLocked;

    private void Start()
    {
        if (unlockDoorsOnStart)
            UnlockDoors();
    }

    public void DecrementEnemyCount(GameObject enemy)
    {
        enemyCount--;

        if (enemyCount <= 0)
        {
            gameManager.instance.EnterGameState(gameManager.GameStates.Calm);
            UnlockDoors();

            gameObject.SetActive(false);
        }
    }

    public void LockDoors()
    {
        doorsLocked = true;
        if (barriers.Length > 0)
        {
            foreach (GameObject barrier in barriers)
            {
                if (barrier != null)
                {
                    if (barrier.GetComponent<ILockable>() != null)
                    {
                        barrier.GetComponent<ILockable>().Lock();
                    }
                    else
                        barrier.GetComponent<Animator>().SetTrigger("Door Locked");
                }
            }
        }
    }

    public void UnlockDoors()
    {
        doorsLocked = false;
        if (barriers.Length > 0)
        {
            foreach (GameObject barrier in barriers)
            {
                if (barrier != null)
                {
                    if (barrier.GetComponent<ILockable>() != null)
                    {
                        barrier.GetComponent<ILockable>().Unlock();
                    }
                    else
                        barrier.GetComponent<Animator>().SetTrigger("Door Open");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            //Debug.Log("Trigger entered: " + other.name);
            if (!doorsLocked)
            {
                gameManager.instance.EnterGameState(gameManager.GameStates.Combat);
                LockDoors();
            }

        }
        else if (other.CompareTag("Enemy"))
        {
            //Debug.Log("Trigger entered: " + other.name);
            enemyCount++;

            other.GetComponentInParent<enemyAI>().DeathEvent += DecrementEnemyCount;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && unlockDoorsOnPlayerLeave)
        {
            //Debug.Log("Trigger exited: " + other.name);
            UnlockDoors();
        }
    }
}
