using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;
    [Tooltip("The position for projectile spawning or melee attack raycast origin.")]
    [SerializeField] protected Transform attackOrigin;
    [Tooltip("The origin of an outgoing alert and where an incoming alert will be received. " +
        "Does not require its own object, any object with a fitting position can be used for it.")]
    [SerializeField] protected Transform alertOrigin;
    [Tooltip("Where this enemy's loot spawns.")]
    [SerializeField] protected Transform lootPosition;
    [SerializeField] GameObject enemyUI;

    [Header("---- Stats ----")]
    [Range(1, 20)][SerializeField] protected int HP;
    [SerializeField] float speed;
    [Tooltip("The maximum distance for spotting the player visually (not attacking).")]
    [SerializeField] protected float detectionRange;
    [Tooltip("The angle that sets enemy field of view (not attacking).")]
    [Range(0, 90)][SerializeField] protected float fieldOfView;
    [Tooltip("The angle that sets enemy field of view (for attacking).")]
    [Range(0, 90)][SerializeField] protected float fieldOfViewAttack;
    [SerializeField] float faceTargetSpeed;
    [Tooltip("Whether the character is summoned by a spawner enemy.\nMinions do not count toward kills.")]
    [SerializeField] bool isMinion;
    [Tooltip("For how long the enemy flashes red upon receiving damage, in seconds.")]
    [SerializeField] float damageFlashLength;
    [Tooltip("The speed of transitioning in blend animations.")]
    [SerializeField] float animSpeedTransition;
    [SerializeField] bool canRoam;
    [SerializeField] int roamDist;
    [Tooltip("The minimum time before starting to roam again.")]
    [Range(0.0f, 60.0f)][SerializeField] int roamPauseTimeMin;
    [Tooltip("The maximum time before starting to roam again.")]
    [Range(0.0f, 60.0f)][SerializeField] int roamPauseTimeMax;

    [Header("---- Attacking ----")]
    [Tooltip("The damage this enemy's attack deals to the target.")]
    [SerializeField] protected int attackDamage;
    [Tooltip("The minimum duration between two attacks in seconds (for example, the value of 0.5 would mean up to 2 attacks per second).")]
    [SerializeField] protected float attackRate;
    [Tooltip("The maximum distance from which this enemy can attack.")]
    [SerializeField] protected float attackRange;
    [Tooltip("The projectile prefab. Ignore for melee enemies.")]
    [SerializeField] protected GameObject bullet; // TODO: rename to `projectile`. Make sure to update the value correctly.

    [Header("---- Other ----")]
    [SerializeField] GameObject keyPrefab;

    protected int origHP;
    protected bool isAttacking;
    protected bool playerIsNearby;
    protected bool playerSpotted;
    protected Vector3 distanceToPlayer;
    protected float angleToPlayer;
    bool destinationChosen;
    Vector3 startingPos;
    float stoppingDistOrig;

    public bool IsMinion
    {
        get { return isMinion; }
        set { isMinion = value; }
    }

    public bool IsAlerted { get => playerSpotted; }
    public Vector3 GetAlertPosition() { return alertOrigin.transform.position; }

    // Start is called before the first frame update
    void Start()
    {
        origHP = HP;
        //enemyManager.instance.EnemySpawned(gameObject, isMinion); // spawners should be responsible for reporting enemies
        stoppingDistOrig = agent.stoppingDistance;
    }

    // Update is called once per frame
    void Update()
    {
        distanceToPlayer = (gameManager.instance.player.transform.position - transform.position);
        angleToPlayer = Vector3.Angle(new Vector3(distanceToPlayer.x, 0, distanceToPlayer.z),
            new Vector3(transform.forward.x, 0, transform.forward.z));

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
                    Attack();
                }
            }
            else
            {
                if (canRoam)
                    StartCoroutine(roam());
                lookForPlayer();
            }
        }

        if (animator != null)
        {
            float targetAnimSpeed = agent.velocity.normalized.magnitude;
            animator.SetFloat("Speed", Mathf.Lerp(animator.GetFloat("Speed"), targetAnimSpeed, animSpeedTransition * Time.deltaTime));
        }
    }

    IEnumerator roam()
    {
        if (agent.remainingDistance > 0.05f || destinationChosen)
            yield break;

        destinationChosen = true;
        agent.stoppingDistance = 0;
        yield return new WaitForSeconds(Random.Range(roamPauseTimeMin, roamPauseTimeMax));

        // new position is inside the sphere
        Vector3 roamPos = Random.insideUnitSphere * roamDist;
        roamPos += startingPos;

        // check if chosen position is on navmesh, adjust if needed
        NavMeshHit hit;
        NavMesh.SamplePosition(roamPos, out hit, roamDist, 1);
        agent.SetDestination(hit.position);

        destinationChosen = false;
    }

    void ChasePlayer()
    {
        agent.SetDestination(gameManager.instance.player.transform.position);
        if (agent.remainingDistance < agent.stoppingDistance)
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

        if (enemyUI != null)
        {
            enemyUI.GetComponent<enemyUI>().UpdateHealthbar(HP, origHP);
        }

        if (HP <= 0)
        {
            die();
        }
    }

    private void spotPlayer()
    {
        StopCoroutine(roam());

        agent.stoppingDistance = stoppingDistOrig;

        BecomeAlerted(gameManager.instance.player.transform.position);
        enemyManager.instance.AlertEnemiesWithinRange(GetAlertPosition(), detectionRange);
    }

    public void BecomeAlerted(Vector3 disturbancePos)
    {
        playerSpotted = true;
        //model.material.color = Color.red; // DEBUG PURPOSES - to see who got alerted
    }

    private void die()
    {
        enemyManager.instance.EnemyDied(gameObject, isMinion);

        if (enemyManager.instance.TotalEnemies == 0 && !gameManager.instance.IsKeyPicked)
        {
            gameManager.instance.ShowHint("Enemy Dropped Key Card");

            Vector3 lootPos = lootPosition != null ? lootPosition.transform.position : transform.position;

            if (keyPrefab != null)
                Instantiate(keyPrefab, lootPos, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    //this is going to change. this is for test feedback for the player.
    IEnumerator flashRed()
    {
        if (model == null)
            yield break;

        // Remember the old color
        Color oldColor = model.material.color;

        // Flash red for some time
        model.material.color = Color.red;
        yield return new WaitForSeconds(damageFlashLength);

        model.material.color = oldColor;
    }

    /// <summary>
    /// Start the attack.
    /// </summary>
    /// <returns></returns>
    protected virtual void Attack()
    {
        isAttacking = true;

        animator.SetTrigger("AttackTrigger");
    }

    /// <summary>
    /// Is called inside of the animation to execute the key attack action.
    /// (e.g., instantiate a bullet, shoot a raycast and do damage, etc.)
    /// <br>This is the method expected to be overriden.</br>
    /// </summary>
    protected virtual void AttackAnimationEvent()
    {
        Vector3 distanceToPlayerFromShootPos = (gameManager.instance.player.transform.position - attackOrigin.transform.position);
        Quaternion bulletRot = Quaternion.LookRotation(distanceToPlayerFromShootPos);

        GameObject bulletInstance = Instantiate(bullet, attackOrigin.position, bulletRot);
        bulletInstance.GetComponent<bullet>().DamageValue = attackDamage;
    }


    protected virtual void AttackAnimationEnd()
    {
        isAttacking = false;
        animator.ResetTrigger("AttackTrigger");

        //yield return new WaitForSeconds(attackRate);
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

    public float GetHealth()
    {
        return HP;
    }

    public float GetOriginalHealth()
    {
        return origHP;
    }
}


