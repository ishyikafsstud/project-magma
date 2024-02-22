using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemySpawner : MonoBehaviour, IActivate
{
    [System.Serializable]
    public class EnemySpawnInfo
    {
        [Tooltip("The prefab of the enemy to spawn.")]
        public GameObject enemy;
        [Tooltip("How many enemies of this kind to spawn.")]
        public int count;
        [Tooltip("The positions that can be used for spawning this type of enemies. They can be " +
            "shared with other enemies.")]
        public Transform[] positionList;
    }

    [Header("---- Spawn settings ----")]
    [SerializeField] List<EnemySpawnInfo> enemiesToSpawn;
    [SerializeField] float timeBetweenSpawns;
    [Tooltip("Whether the enemies spawned by this spawner should roam.")]
    [SerializeField] bool canEnemiesRoam = true;

    [Header("---- Trigger condition ----")]
    [Tooltip("Deactivate the trigger area and activate the spawner when the activator stone is picked.")]
    [SerializeField] bool isAmbushSpawner = false;
    [Tooltip("Deactivate the trigger area. Useful for custom trigger scenarios, e.g. spawner is activated " +
        "when a button on the level is pressed. \n\nTrigger area is automatically disabled for ambush " +
        "spawners.")]
    [SerializeField] bool deactivateTrigger = false;


    bool isTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        if (isAmbushSpawner)
        {
            DeactivateTriggerAreas();
            gameManager.AmbushStarted += ProxyActivate;
        }
        else if (deactivateTrigger)
        {
            DeactivateTriggerAreas();
        }
    }

    /// <summary>
    /// Set off the spawner.
    /// </summary>
    /// <returns></returns>
    public IEnumerator Activate()
    {
        gameObject.SetActive(true);
        isTriggered = true;

        // Go through every enemy info and spawn the specified number of enemies
        foreach (EnemySpawnInfo enemyInfo in enemiesToSpawn)
        {
            // Skip this enemyInfo if it is invalid (e.g., the enemyInfo is empty, the enemy
            // prefab is not set, the spawn position list is empty)
            if (enemyInfo == null || enemyInfo.enemy == null || enemyInfo.positionList.Length == 0)
                continue;

            // Spawn the specified number of enemies
            for (int i = 0; i < enemyInfo.count; i++)
            {
                // Pick the spawn position
                int arrayPos = Random.Range(0, enemyInfo.positionList.Length);
                Transform spawnTransform = enemyInfo.positionList[arrayPos];

                // Spawn the enemy at the chosen position
                GameObject enemy = Instantiate(enemyInfo.enemy, spawnTransform.position, spawnTransform.rotation);
                enemyManager.instance.EnemySpawned(enemy, false); // Report about enemy spawning

                // Incorporate other settings
                enemy.GetComponent<enemyAI>().SetCanRoam(canEnemiesRoam);

                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
            StartCoroutine(Activate());
    }

    /// <summary>
    /// A proxy method for calling the Activate() coroutine. Useful for events.
    /// </summary>
    void ProxyActivate()
    {
        if (!isTriggered)
            StartCoroutine(Activate());
    }

    void DeactivateTriggerAreas()
    {
        foreach (Collider collider in GetComponents<Collider>())
        {
            collider.enabled = false;
        }
    }
}
