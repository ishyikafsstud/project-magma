using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyManager : MonoBehaviour
{
    public static enemyManager instance;

    /// <summary>
    /// The count of significant enemies (non-minions).
    /// </summary>
    int enemyCount;
    /// <summary>
    /// A list of all enemies (including minions).
    /// </summary>
    List<GameObject> enemies;

    /// <summary>
    /// The count of significant enemies (i.e. non-minions).
    /// </summary>
    public int EnemyCount
    {
        get => enemyCount;
        set
        {
            if (enemyCount == value) return;

            enemyCount = Mathf.Max(value, 0);
            if (gameManager.instance != null)
            {
                gameManager.instance.UpdateEnemyCountText();
            }
        }
    }

    /// <summary>
    /// The total count of enemies (includes minions).
    /// </summary>
    public int TotalEnemies
    {
        get => enemies.Count;
    }

    void Awake()
    {
        instance = this;

        enemyCount = 0;

        enemies = new List<GameObject>();
    }

    void Start()
    {
        // TODO: Get and save the ambush enemy spawner + disable it using SetActive(false)
    }

    /// <summary>
    /// Alerts enemies around a certain area. Alerts only if there are no obstructions between them
    /// and the object of attention.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="range"></param>
    public void AlertEnemiesWithinRange(Vector3 pos, float range)
    {

        foreach (GameObject enemy in enemies)
        {
            // If the enemy is not alerted yet and within the alert range
            if (!enemy.GetComponent<enemyAI>().IsAlerted
                && Vector3.Distance(pos, enemy.transform.position) < range)
            {
                Vector3 raycastDirection = enemy.transform.position - pos;
                RaycastHit hit;
                // Check for any obstructions between the source and the enemy.
                // If there are none, alert them.
                if (Physics.Raycast(pos, raycastDirection, out hit))
                {
                    if (hit.collider.gameObject == enemy)
                        enemy.GetComponent<enemyAI>().BecomeAlerted(pos);
                }
            }
        }
    }

    public void EnemySpawned(GameObject enemy, bool isMinion)
    {
        enemies.Add(enemy);
        EnemyCount += !isMinion ? 1 : 0; // Increase significant enemy count if not a minion
    }

    public void EnemyDied(GameObject enemy, bool isMinion)
    {
        enemies.Remove(enemy);
        EnemyCount -= !isMinion ? 1 : 0; // Decrease significant enemy count if not a minion
    }

}
