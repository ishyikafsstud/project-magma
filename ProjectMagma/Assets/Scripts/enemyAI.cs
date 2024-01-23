using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform shootPos;

    [SerializeField] int HP;
    [Tooltip("For how long the enemy flashes red upon receiving damage.")]
    [SerializeField] float damageFlashLength;
    [SerializeField] int speed;
    [SerializeField] float detectionRange;

    [Header("Damage")]
    //[SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] float meleeRange;
    [SerializeField] int meleeDamage;
    [SerializeField] float meleeRate;
    [SerializeField] float particaleDuration;
    [SerializeField] GameObject hitParticlesPrefab;
    //[SerializeField] int shootDist;
    [SerializeField] GameObject bullet;


    [Header("Other")]
    [SerializeField] bool isMeleeEnemy;
    [SerializeField] GameObject keyPrefab;

   
    bool isMeleeAttacking;
    bool isShooting;
    bool playerIsNearby;
    bool playerSpotted;


    // Start is called before the first frame update
    void Start()
    {
        gameManager.instance.EnemyCount++;
    }

    // Update is called once per frame
    void Update()
    {
        // Pursue player if he has been spotted, try to spot him otherwise.
        // * If the player is nearby but not spotted yet, try to spot him using a raycast.
        // * If the player has been spotted, pursue player even if he left the detection zone.
        if (playerIsNearby || playerSpotted)
        {
            if (playerSpotted)
                ChasePlayer();
            else
            {
                lookForPlayer();
            }
        }
    }

    void ChasePlayer()
    {
        transform.LookAt(gameManager.instance.player.transform.position);
        agent.SetDestination(gameManager.instance.player.transform.position);

        

        if (isMeleeEnemy && !isMeleeAttacking)
        {
            // If player is within melee range, perform melee attack
            StartCoroutine(MeleeAttack());
        }
        else if (!isMeleeEnemy && !isShooting)
        {
            // If not melee enemy, start shooting
            StartCoroutine(Shoot());
            
        }
    }

    /// <summary>
    /// Look for player within the detection range. Spot him if visible.
    /// Supported by a raycast.
    /// </summary>
    private void lookForPlayer()
    {
        Vector3 dirToPlayer = (gameManager.instance.player.transform.position - transform.position).normalized;
        //Debug.DrawRay(transform.position, dirToPlayer * detectionRange, Color.red);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, dirToPlayer, out hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
                spotPlayer();
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashRed());

        // Trigger the enemy to follow player.
        // It is safe because the only way for the enemy to receive damage right now is to be hit by the player.
        spotPlayer();

        if (HP <= 0)
        {
            die();
        }
    }

    private void spotPlayer()
    {
        playerSpotted = true;
    }

    private void die()
    {
        gameManager.instance.DecreaseEnemyCount();

        if (gameManager.instance.EnemyCount == 0)
        {
            gameManager.instance.ShowHint("Enemy Dropped Key Card");
            Instantiate(keyPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    //this is going to change. this is for test feedback for the player.
    IEnumerator flashRed()
    {
        // Remember the old color
        Color oldColor = model.material.color;

        // Flash red for some time
        model.material.color = Color.red;
        yield return new WaitForSeconds(damageFlashLength);

        model.material.color = oldColor;
    }

    IEnumerator Shoot()
    {
        isShooting = true;

        Instantiate(bullet, shootPos.position, transform.rotation);

        yield return new WaitForSeconds(shootRate); // Unity Timer

        isShooting = false;
    }

    IEnumerator MeleeAttack()
    {
       if (isMeleeAttacking)
        {
            yield break;
        }

        isMeleeAttacking = true;
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, meleeRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                IDamage playerDamageable = hit.collider.GetComponent<IDamage>();
                if (playerDamageable != null)
                {
                    playerDamageable.takeDamage(meleeDamage);
                }
                SpawnHitParticles(hit.point);
            }
        }
        yield return new WaitForSeconds(meleeRate);
        isMeleeAttacking = false;

        yield break;

    }
    void SpawnHitParticles(Vector3 position)
    {
        GameObject hitParticles = Instantiate(hitParticlesPrefab, position, Quaternion.identity);

        Destroy(hitParticles, particaleDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = false;
        }
    }
}
