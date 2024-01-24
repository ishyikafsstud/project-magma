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
