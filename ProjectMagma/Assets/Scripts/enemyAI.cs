using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [Tooltip("The position for projectile spawning.")]
    [SerializeField] Transform shootPos;

    [Header("Stats")]
    [SerializeField] protected int HP;
    [SerializeField] int speed;
    [Tooltip("The maximum distance for spotting the player visually (not attacking).")]
    [SerializeField] protected float detectionRange;
    [Tooltip("The angle that sets enemy field of view (not attacking).")]
    [Range(0,90)][SerializeField] protected float fieldOfView;
    [Tooltip("The angle that sets enemy field of view (for attacking).")]
    [Range(0, 90)][SerializeField] protected float fieldOfViewAttack;
    [SerializeField] float faceTargetSpeed;
    [Tooltip("Whether the character is summoned by a spawner enemy.\nMinions do not count toward kills.")]
    [SerializeField] bool isMinion;
    [Tooltip("For how long the enemy flashes red upon receiving damage, in seconds.")]
    [SerializeField] float damageFlashLength;

    [Header("Attacking")]
    [Tooltip("The damage this enemy's attack deals to the target.")]
    [SerializeField] protected int attackDamage;
    [Tooltip("The minimum duration between two attacks in seconds (for example, the value of 0.5 would mean up to 2 attacks per second).")]
    [SerializeField] protected float attackRate;
    [Tooltip("The maximum distance from which this enemy can attack.")]
    [SerializeField] protected float attackRange;
    [SerializeField] protected GameObject bullet; // TODO: rename to `projectile`. Make sure to update the value correctly.

    [Header("Other")]
    [SerializeField] GameObject keyPrefab;

    protected bool isAttacking;
    bool playerIsNearby;
    bool playerSpotted;
    Vector3 distanceToPlayer;
    float angleToPlayer;

    public bool IsMinion
    {
        get { return isMinion; }
        set { isMinion = value; }
    }

    public bool IsAlerted { get => playerSpotted; }

    // Start is called before the first frame update
    void Start()
    {
        enemyManager.instance.EnemySpawned(gameObject, isMinion);
    }

    // Update is called once per frame
    void Update()
    {
        distanceToPlayer = (gameManager.instance.player.transform.position - transform.position);
        angleToPlayer = Vector3.Angle(new Vector3(distanceToPlayer.x, 0, distanceToPlayer.z), new Vector3(transform.forward.x, 0, transform.forward.z));
        // Pursue player if he has been spotted, try to spot him otherwise.
        // * If the player is nearby but not spotted yet, try to spot him using a raycast.
        // * If the player has been spotted, pursue player even if he left the detection zone.
        if (playerIsNearby || playerSpotted)
        {
            if (playerSpotted)
            {
                ChasePlayer();

                // If player is within the attack range and unless already attacking, attack him
                if (distanceToPlayer.magnitude <= attackRange && !isAttacking && angleToPlayer < fieldOfViewAttack)
                {
                    StartCoroutine(Attack());
                }
            }
            else
            {
                lookForPlayer();
            }
        }
    }

    void ChasePlayer()
    {
        agent.SetDestination(gameManager.instance.player.transform.position);
        if(agent.remainingDistance < agent.stoppingDistance)
            faceTarget();
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(distanceToPlayer.x, 0, distanceToPlayer.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, faceTargetSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Look for player within the detection range. Spot him if visible.
    /// Supported by a raycast.
    /// </summary>
    private void lookForPlayer()
    {

        //Debug.DrawRay(transform.position, dirToPlayer * detectionRange, Color.red);
        //Debug.Log(angleToPlayer);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, distanceToPlayer, out hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= fieldOfView)
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
        BecomeAlerted(gameManager.instance.player.transform.position);
        enemyManager.instance.AlertEnemiesWithinRange(transform.position, detectionRange); // Alert nearby enemies
    }

    public void BecomeAlerted(Vector3 disturbancePos)
    {
        playerSpotted = true;
        //model.material.color = Color.red; // DEBUG PURPOSES - to see who got alerted
    }

    private void die()
    {
        enemyManager.instance.EnemyDied(gameObject, isMinion);

        if (enemyManager.instance.EnemyCount == 0)
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

    /// <summary>
    /// Attack the target by shooting a projectile at it.
    /// </summary>
    /// <returns></returns>
    // can be overridden for melee enemys in the enemyAIMelee script
    protected virtual IEnumerator Attack()
    {
        isAttacking = true;

        Instantiate(bullet, shootPos.position, transform.rotation);

        yield return new WaitForSeconds(attackRate); // Unity Timer

        isAttacking = false;
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
