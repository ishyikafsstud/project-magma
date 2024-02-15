using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class spawnOnEnemyEvent : MonoBehaviour
{

    [Tooltip("The object to spawn.")]
    [SerializeField] GameObject spawnedObjectPrefab;
    [Tooltip("The spawn positions for the object. The specified object will be spawned at each of them.")]
    [SerializeField] List<GameObject> spawnPositions = new List<GameObject>();

    [Header("---- Spawn Triggering ----")]
    [SerializeField] bool spawnOnDeath;
    [SerializeField] bool spawnOnAttack;

    enemyAI enemyAIComponent;

    // Start is called before the first frame update
    void Start()
    {
        enemyAIComponent = GetComponent<enemyAI>();

        if (spawnOnDeath)
            enemyAIComponent.DeathEvent += SpawnObjectsOnDeath;
        if (spawnOnAttack)
            enemyAIComponent.AttackEvent += SpawnObjectsOnDeath;
    }

    void SpawnObjectsOnDeath(GameObject diedEnemy)
    {
        if (spawnedObjectPrefab == null) return;

        foreach (GameObject spawnPos in spawnPositions)
        {
            GameObject objectInst = Instantiate(spawnedObjectPrefab, spawnPos.transform.position, spawnPos.transform.rotation);
            enemyAI spawnedEnemyScript = objectInst.GetComponent<enemyAI>();
            if (spawnedEnemyScript != null)
            {
                spawnedEnemyScript.IsMinion = true;
                spawnedEnemyScript.BecomeAlerted(gameManager.instance.player.transform.position);
            }
        }
    }
}
