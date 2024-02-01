using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawner : MonoBehaviour
{
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] int numToSpawn;
    [SerializeField] int timeBetweenSpawns;
    [SerializeField] Transform[] spawnPos;


    int spawnCount;
    bool isSpawning;
    bool startSpawning;

    // Start is called before the first frame update
    void Start()
    {
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
