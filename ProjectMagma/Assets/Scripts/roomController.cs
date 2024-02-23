using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class roomController : MonoBehaviour
{
    [Tooltip("Objects activated by this room controller. They must have a script that inherits from IActivate.")]
    public GameObject[] activatedObjects;

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
        if (activatedObjects.Length > 0)
        {
            foreach (GameObject obj in activatedObjects)
            {
                if (obj != null && obj.GetComponent<IActivate>() != null)
                {
                    StartCoroutine(obj.GetComponent<IActivate>().Activate());
                }
            }
        }
    }

    public void UnlockDoors()
    {
        doorsLocked = false;
        if (activatedObjects.Length > 0)
        {
            foreach (GameObject obj in activatedObjects)
            {
                if (obj != null && obj.GetComponent<IActivate>() != null)
                {
                    StartCoroutine(obj.GetComponent<IActivate>().Activate());
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
