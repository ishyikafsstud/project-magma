using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    /// <summary>
    /// Class to hold enemy prefabs and spawn count
    /// to be set in unity and used in spawn method
    /// </summary>
    public GameObject enemyPrefab;
    public int spawnCount;
}

public class AmbushSpawner : MonoBehaviour
{
    // List of enemy prefabs and spawn counts
    [SerializeField] List<EnemySpawnInfo> enemiesToSpawn;
    [SerializeField] int timeBetweenSpawns;
    [SerializeField] Transform[] spawnPos;

    // Method to start the ambush spawning
    public void StartAmbush()
    {
        gameManager.instance.ShowHint("Ambush!\nYou Collected The Key\nEscape though the portal\n or kill them all");
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        if (enemiesToSpawn.Count == 0)
        {
            yield break;
        }

        // Spawns enemies based on spawn counts
        foreach (var enemyInfo in enemiesToSpawn)
        {
            for (int i = 0; i < enemyInfo.spawnCount; i++)
            {
                int arrayPos = Random.Range(0, spawnPos.Length);

                // Instantiates the current enemy in the list
                Instantiate(enemyInfo.enemyPrefab, spawnPos[arrayPos].position, spawnPos[arrayPos].rotation);

                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }

        gameObject.SetActive(false);
    }
}
