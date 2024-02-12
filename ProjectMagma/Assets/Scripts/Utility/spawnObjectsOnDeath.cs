using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnObjectsOnDeath : MonoBehaviour
{

    [Tooltip("The object to spawn.")]
    [SerializeField] GameObject spawnObject;
    [Tooltip("The spawn positions for the object. The specified object will be spawned at each of them.")]
    [SerializeField] List<GameObject> spawnPositions = new List<GameObject>();

    enemyAI parent;

    // Start is called before the first frame update
    void Start()
    {
        parent = GetComponent<enemyAI>();
        parent.OnDied += SpawnObjectsOnDeath;
    }

    void SpawnObjectsOnDeath(GameObject diedEnemy)
    {
        foreach (GameObject spawnPos in spawnPositions)
        {
            Instantiate(spawnObject, spawnPos.transform.position, spawnPos.transform.rotation);
        }
    }
}
