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
    [Tooltip("If false, the enemies spawned by this spawner will not count toward the activator stone " +
        "or the ambush reward.")]
    [SerializeField] bool requiredToKill = true;
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

        // For required non-ambush spawners, the enemy count should be reported on spawn
        if (requiredToKill && !isAmbushSpawner)
            foreach (EnemySpawnInfo info in enemiesToSpawn)
            {
                if (IsInfoValid(info))
                    enemyManager.instance.EnemyCount += info.count;
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

        // For required ambush spawners, the enemy count should be reported upon activation
        if (isAmbushSpawner && requiredToKill)
            foreach (EnemySpawnInfo info in enemiesToSpawn)
            {
                if (IsInfoValid(info))
                    enemyManager.instance.EnemyCount += info.count;
            }

        // Go through every enemy info and spawn the specified number of enemies
        foreach (EnemySpawnInfo enemyInfo in enemiesToSpawn)
        {
            // Skip this enemyInfo if it is invalid (e.g., the enemyInfo is empty, the enemy
            // prefab is not set, the spawn position list is empty)
            if (!IsInfoValid(enemyInfo))
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

                // Integrate other settings
                enemyAI enemyScript = enemy.GetComponent<enemyAI>();
                enemyScript.SetCanRoam(canEnemiesRoam);
                enemyScript.CountDeath = requiredToKill;

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

    bool IsInfoValid(EnemySpawnInfo info)
    {
        return (info != null && info.enemy != null && info.positionList.Length > 0);
    }
}
