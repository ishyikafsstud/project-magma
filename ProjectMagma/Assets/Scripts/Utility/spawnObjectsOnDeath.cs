using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnObjectsOnDeath : MonoBehaviour
{

    [Tooltip("The enemy to spawn.")]
    [SerializeField] GameObject spawnedEnemyPrefab;
    [Tooltip("The spawn positions for the object. The specified object will be spawned at each of them.")]
    [SerializeField] List<GameObject> spawnPositions = new List<GameObject>();

    enemyAI enemyAIComponent;

    // Start is called before the first frame update
    void Start()
    {
        enemyAIComponent = GetComponent<enemyAI>();
        enemyAIComponent.OnDied += SpawnObjectsOnDeath;
    }

    void SpawnObjectsOnDeath(GameObject diedEnemy)
    {
        foreach (GameObject spawnPos in spawnPositions)
        {
            GameObject enemyInst = Instantiate(spawnedEnemyPrefab, spawnPos.transform.position, spawnPos.transform.rotation);
            enemyAI spawnedEnemyScript = enemyInst.GetComponent<enemyAI>();
            spawnedEnemyScript.IsMinion = true;
            spawnedEnemyScript.BecomeAlerted(gameManager.instance.player.transform.position);
        }
    }
}
