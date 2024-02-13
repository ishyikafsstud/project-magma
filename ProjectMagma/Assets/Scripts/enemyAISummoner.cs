using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyAISummoner : enemyAI
{
    [Header("---- Summoning ----")]
    [SerializeField] int maxAliveMinions;
    [Tooltip("The minion to spawn.")]
    [SerializeField] GameObject minionPrefab;
    [Tooltip("The spawn positions. A minion will be spawned at each of them.")]
    [SerializeField] List<GameObject> spawnPositions = new List<GameObject>();

    int curAliveMinions;

    void MinionDied(GameObject minion)
    {
        curAliveMinions--;
    }

    protected override void AttackAnimationEvent()
    {
        OnAttack();

        foreach (GameObject spawnPos in spawnPositions)
        {
            curAliveMinions++;

            GameObject enemyInst = Instantiate(minionPrefab, spawnPos.transform.position, spawnPos.transform.rotation);
            enemyAI spawnedEnemyScript = enemyInst.GetComponent<enemyAI>();
            spawnedEnemyScript.IsMinion = true;
            spawnedEnemyScript.BecomeAlerted(gameManager.instance.player.transform.position);

            enemyInst.GetComponent<enemyAI>().DeathEvent += MinionDied;
        }
    }

    protected override bool CanAttack()
    {
        return base.CanAttack() && (curAliveMinions + spawnPositions.Count <= maxAliveMinions);
    }
}
