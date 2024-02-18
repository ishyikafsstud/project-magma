using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawner : MonoBehaviour
{
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] int numToSpawn;
    [SerializeField] int timeBetweenSpawns;
    [SerializeField] Transform[] spawnPos;
    [Tooltip("Whether the enemies spawned by this spawner should roam.")]
    [SerializeField] bool canEnemiesRoam = true;


    int spawnCount;
    bool isSpawning;
    bool startSpawning;

    // Start is called before the first frame update
    void Start()
    {
        // Find all child objects of this spawner and assign them as spawn positions
        spawnPos = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            spawnPos[i] = transform.GetChild(i);
        }

        enemyManager.instance.EnemyCount += numToSpawn;
    }

    // Update is called once per frame
    void Update()
    {
        if (startSpawning && spawnCount < numToSpawn && !isSpawning)
        {
            StartCoroutine(spawn());
        }
    }

    IEnumerator spawn()
    {
        isSpawning = true;
        int arrayPos = Random.Range(0, spawnPos.Length);
        
        GameObject enemy = Instantiate(objectToSpawn, spawnPos[arrayPos].transform.position, spawnPos[arrayPos].transform.rotation);
        enemyManager.instance.EnemySpawned(enemy, false); // Report about enemy spawning
        enemy.GetComponent<IEnemy>().SetCanRoam(canEnemiesRoam);
        spawnCount++;
        
        yield return new WaitForSeconds(timeBetweenSpawns);
        isSpawning = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            startSpawning = true;
        }
    }
}
