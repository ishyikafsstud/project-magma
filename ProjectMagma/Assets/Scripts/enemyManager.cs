using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyManager : MonoBehaviour
{
    public static enemyManager instance;

    int enemyCount;
    int totalEnemies;

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
        get => totalEnemies;
        set { totalEnemies = Mathf.Max(value, 0); }
    }

    void Awake()
    {
        instance = this;

        enemyCount = 0;
        totalEnemies = 0;
    }


    public void IncrementEnemyCount(bool isMinion)
    {
        EnemyCount += !isMinion ? 1 : 0; // Increase significant enemy count if not a minion
        TotalEnemies++;
    }

    public void DecrementEnemyCount(bool isMinion)
    {
        EnemyCount -= !isMinion ? 1 : 0; // Decrease significant enemy count if not a minion
        TotalEnemies++;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
