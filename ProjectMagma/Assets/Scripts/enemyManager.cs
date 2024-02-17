using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyManager : MonoBehaviour
{
    public static enemyManager instance;
    public playerController playerController;
    /// <summary>
    /// The count of significant enemies (non-minions).
    /// </summary>
    int enemyCount;
    /// <summary>
    /// A list of all enemies (including minions).
    /// </summary>
    List<GameObject> enemies;
    /// <summary>
    /// Ambush spawner game object
    /// </summary>
    public GameObject ambushSpawnerObject;
   /// <summary>
   /// AmbushSpawner script
   /// </summary>
    public AmbushSpawner ambushSpawner;
    /// <summary>
    /// Queue to hold enemies currently attacking.
    /// </summary>
    Queue<GameObject> attackQueue = new Queue<GameObject>();
    /// <summary>
    /// Maximum number of enemies allowed to attack simultaneously.
    /// </summary>
    [SerializeField] public int maxAttackingEnemies = 6;
    /// <summary>
    /// Number of enemies currently attacking.
    /// </summary>
    public int attackingEnemiesCount;
    /// <summary>
    /// Indicates whether enemies are currently attacking.
    /// </summary>
    bool isAttacking = false;
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
        get => enemies.Count;
    }

    void Awake()
    {
        instance = this;

        enemyCount = 0;

        enemies = new List<GameObject>();

        playerController = FindObjectOfType<playerController>();
    }

    void Start()
    {
        // Finds the GameObject with the tag "Ambush Spawner"
        ambushSpawnerObject = GameObject.FindGameObjectWithTag("Ambush Spawner");

        // Checks if the GameObject is found before trying to get the AmbushSpawner component
        if (ambushSpawnerObject != null)
        {
            // Gets the AmbushSpawner component
            ambushSpawner = ambushSpawnerObject.GetComponent<AmbushSpawner>();

            // Checks if the AmbushSpawner component is found before setting it to inactive
            if (ambushSpawner != null)
                ambushSpawner.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Alerts enemies around a certain area. Alerts only if there are no obstructions between them
    /// and the object of attention.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="range"></param>
    public void AlertEnemiesWithinRange(Vector3 alertOrigin, float range)
    {
        foreach (GameObject enemy in enemies)
        {
            // If the enemy is not alerted yet and within the alert range
            if (!enemy.GetComponent<enemyAI>().IsAlerted
                && Vector3.Distance(alertOrigin, enemy.transform.position) < range)
            {
                Vector3 raycastDirection = enemy.GetComponent<enemyAI>().GetAlertPosition() - alertOrigin;
                RaycastHit hit;
                // Check for any obstructions between the source and the enemy.
                // If there are none, alert them.
                if (Physics.Raycast(alertOrigin, raycastDirection, out hit))
                {
                    if (hit.collider.gameObject == enemy)
                        enemy.GetComponent<enemyAI>().BecomeAlerted(alertOrigin);
                }
            }
        }
    }

    public void EnemySpawned(GameObject enemy, bool isMinion)
    {
        enemies.Add(enemy);
        StartCoroutine(EnqueueAttack(enemy));
        //EnemyCount += !isMinion ? 1 : 0; // Increase significant enemy count if not a minion
    }

    public void EnemyDied(GameObject enemy, bool isMinion)
    {
        enemies.Remove(enemy);
        EnemyCount -= !isMinion ? 1 : 0; // Decrease significant enemy count if not a minion
        if (!isMinion)
        {
            int restoredHealthValue = enemy.GetComponent<enemyAI>().restoredHealthValue;
            playerController.health += restoredHealthValue;
            playerController.updatePlayerUI();
        }
    }

    IEnumerator EnqueueAttack(GameObject enemy)
    {
        attackQueue.Enqueue(enemy);
        //Debug.Log("Enemy " + enemy.name + " added to attack queue. Queue count: " + attackQueue.Count);

        if (!isAttacking)
        {
            isAttacking = true;
            while (attackQueue.Count > 0)
            {
                if(attackQueue.Count <= maxAttackingEnemies)
                {
                    GameObject nextEnemy = attackQueue.Dequeue();
                    //Debug.Log("Enemy " + nextEnemy.name + " attacking.");

                    yield return new WaitForSeconds(1f);
                    //Debug.Log("Enemy " + nextEnemy.name + " finished attacking.");
                }
                yield return null;
            }
            isAttacking = false;
            Debug.Log("All enemies finished attacking.");
        }
    }

}
