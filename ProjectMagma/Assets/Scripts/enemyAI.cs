using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class enemyAI : MonoBehaviour, IDamage, IPushable
{
    [Header("----- Components -----")]
    [Tooltip("Models of the enemy that need to change color on hurt, freeze, etc.")]
    [SerializeField] Renderer[] models;
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
    [SerializeField] Collider primaryCollider;

    [Header("---- Stats ----")]
    [Range(1, 100)][SerializeField] protected float HP;
    [SerializeField] public int restoredHealthValue;
    [Tooltip("Whether the character is summoned by a spawner enemy.\nMinions do not count toward kills.")]
    [SerializeField] bool isMinion;
    [Tooltip("Whether the enemy can drop loot (activator stone, ambush reward, etc.)")]
    [SerializeField] bool canDropLoot = true;
    public bool CanDropLoot { get => canDropLoot; set => canDropLoot = value; }
    [Tooltip("Whether this enemy's death should decrease enemy counter, even if it is not a minion." +
        "\n Do not set to false unless the enemy is not required to be killed to continue the level.")]
    [SerializeField] bool countDeath = true;
    public bool CountDeath { get => countDeath; set => countDeath = value; }

    [Header("---- FOV and animation stats ----")]
    [Tooltip("The maximum distance for spotting the player visually (not attacking).")]
    [SerializeField] protected float detectionRange;
    [Tooltip("The angle that sets enemy field of view (not attacking).")]
    [Range(0, 180)][SerializeField] protected float fieldOfView = 45;
    [Tooltip("The angle that sets enemy field of view (for attacking).")]
    [Range(0, 90)][SerializeField] protected float fieldOfViewAttack = 25;
    [SerializeField] float faceTargetSpeed = 6;
    [Tooltip("For how long the enemy flashes red upon receiving damage, in seconds.")]
    [SerializeField] float damageFlashLength = 0.1f;
    [Tooltip("The speed of transitioning in blend animations.")]
    [SerializeField] float animSpeedTransition = 9;

    [Header("---- Attacking ----")]
    [Tooltip("The damage this enemy's attack deals to the target.")]
    [SerializeField] protected int attackDamage;
    [Tooltip("The minimum duration between two attacks in seconds (for example, the value of 0.5 would mean up to 2 attacks per second).")]
    [SerializeField] protected float attackRate;
    [Tooltip("The maximum distance from which this enemy can attack.")]
    [SerializeField] protected float attackRange;
    [Tooltip("The projectile prefab. Ignore for melee enemies.")]
    [SerializeField] protected GameObject projectile;

    [Header("---- Roaming ----")]
    [SerializeField] bool canRoam = true;
    public bool CanRoam { get => canRoam; set => canRoam = value; }
    [SerializeField] int roamDist = 10;
    [Tooltip("The minimum time before starting to roam again.")]
    [Range(0, 60)][SerializeField] int roamPauseTimeMin = 3;
    [Tooltip("The maximum time before starting to roam again.")]
    [Range(0, 60)][SerializeField] int roamPauseTimeMax = 6;

    [Header("---- Combat Effects ---")]
    [SerializeField] protected int maxFreezeStack = 5;
    [Tooltip("The time for one freeze effect stack to pass.")]
    [SerializeField] float freezeTime = 5.0f;
    [SerializeField] protected float freezeStackStrength = 0.08f;
    [SerializeField] Color freezeColor = new Color(0f, 1f, 1f, 1f);
    [SerializeField] int freezeColorMultiplier = 2;
    /// <summary>
    /// Do not access/set this value directly, use CurrentFreezeStack setter.
    /// </summary>
    protected int currentFreezeStack = 0;

    [Header("----- Audio -----")]
    [SerializeField] public entitySoundManager soundManager;

    [Header("---- Other ----")]
    [SerializeField] bool skipDeathAnimation;

    protected float healthOriginal;
    protected bool isAttacking;

    protected bool playerIsNearby;
    protected bool playerSpotted;
    protected Vector3 distanceToPlayer;
    protected float angleToPlayer;

    bool destinationChosen;
    Vector3 startingPos;
    float origSpeed;
    float stoppingDistOrig;

    bool hasBeenAlerted;
    bool isGlowingHurt;
    bool isDead;
    bool shouldDropLoot;

    public delegate void EnemyAction(GameObject enemy);
    public event EnemyAction AttackEvent;
    public event EnemyAction AlertedEvent;
    public event EnemyAction DeathEvent;

    public delegate void StatsUpdate(float value, float maxValue);
    public event StatsUpdate HealthChanged;

    public float Health
    {
        get => HP;
        set
        {
            value = Mathf.Clamp(value, 0, healthOriginal);
            if (HP != value)
            {
                HP = value;
                HealthChanged?.Invoke(HP, healthOriginal);

                if (HP <= 0)
                {
                    die();
                }
            }
        }
    }

    protected virtual void OnAttack()
    {
        AttackEvent?.Invoke(this.gameObject);
    }
    protected virtual void OnDeath()
    {
        DeathEvent?.Invoke(this.gameObject);
    }

    public bool IsMinion
    {
        get { return isMinion; }
        set { isMinion = value; }
    }

    public bool IsAlerted { get => playerSpotted; }
    public Vector3 GetAlertPosition() { return alertOrigin.transform.position; }

    int CurrentFreezeStack
    {
        get => currentFreezeStack;
        set
        {
            // Make sure the new value is within acceptable range
            int actualNewValue = Mathf.Clamp(value, 0, maxFreezeStack);

            // If the attempted new value is the same as the current value, skip
            if (currentFreezeStack == actualNewValue)
                return;

            currentFreezeStack = actualNewValue;

            UpdateSpeed();
            UpdateModelColor();
        }
    }

    bool IsGlowingHurt
    {
        get => isGlowingHurt;
        set
        {
            if (isGlowingHurt == value)
                return;

            isGlowingHurt = value;
            UpdateModelColor();
        }
    }

    public float GetFreezeEffectStrength()
    {
        return currentFreezeStack * freezeStackStrength;
    }
    public float GetSpeedModifier()
    {
        return Mathf.Max(1.0f - GetFreezeEffectStrength(), 0);
    }


    // Start is called before the first frame update
    void Start()
    {
        healthOriginal = HP;
        origSpeed = agent.speed;
        //enemyManager.instance.EnemySpawned(gameObject, isMinion); // spawners should be responsible for reporting enemies
        stoppingDistOrig = agent.stoppingDistance;
        isDead = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;

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
                if (!isAttacking)
                {
                    ChasePlayer();
                }

                if (CanAttack())
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

        // Lock the enemy rotation during attack
        agent.updateRotation = !isAttacking;

        if (animator != null)
        {
            float targetAnimSpeed = agent.velocity.normalized.magnitude;
            animator.SetFloat("Speed", Mathf.Lerp(animator.GetFloat("Speed"), targetAnimSpeed, animSpeedTransition * Time.deltaTime));
        }
    }

    /// <summary>
    /// Calculate navigation and animation speed 
    /// </summary>
    void UpdateSpeed()
    {
        float speedModifier = GetSpeedModifier();

        agent.speed = origSpeed * speedModifier;
        animator.speed = speedModifier;
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
        if (agent.enabled)
            agent.SetDestination(hit.position);

        destinationChosen = false;
    }

    void ChasePlayer()
    {
        // Run after player
        agent.SetDestination(gameManager.instance.player.transform.position);

        // Continue facing player even if they are within the stopping distance
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

    bool canSeePlayerForAttack()
    {
        RaycastHit hit;
        // Collision layers: Default, Player, Enemy
        int layerMask = (1 << 0) | (1 << 3) | (1 << 6);
        if (Physics.Raycast(transform.position, distanceToPlayer, out hit, attackRange, layerMask))
        {
            return (hit.collider.CompareTag("Player") && angleToPlayer <= fieldOfViewAttack);
        }

        return false;
    }

    public void Push(Vector3 direction, float force)
    {
        transform.Translate(direction * force * Time.deltaTime, Space.World);
    }

    public void takeDamage(int amount)
    {
        if (isDead)
            return;

        StartCoroutine(flashRed());

        Health -= amount;

        soundManager?.PlayHurt();

        // Trigger the enemy to follow player.
        // It is safe because the only way for the enemy to receive damage right now is to be attacked by the player.
        spotPlayer();
    }

    public IEnumerator ApplyFreeze(int stacks)
    {
        int appliedStacks = Mathf.Min(stacks, maxFreezeStack - CurrentFreezeStack);
        CurrentFreezeStack += appliedStacks;

        yield return new WaitForSeconds(freezeTime);

        CurrentFreezeStack -= appliedStacks;
    }

    //this is going to change. this is for test feedback for the player.
    IEnumerator flashRed()
    {
        IsGlowingHurt = true;
        UpdateModelColor();

        yield return new WaitForSeconds(damageFlashLength);

        IsGlowingHurt = false;
        UpdateModelColor();
    }

    void UpdateModelColor()
    {
        // Go through every model in the models array to ensure every specified model changes its color
        foreach (Renderer model in models)
        {
            if (model == null)
                return;

            // If the enemy is supposed to flash because he was hurt, make the color of it red
            if (isGlowingHurt)
            {
                model.material.color = Color.red;
            }
            else
            {
                // Determine how blue the enemy must look based on the freeze effect strength
                model.material.color = Color.Lerp(Color.white, freezeColor, GetFreezeEffectStrength() * freezeColorMultiplier);
            }
        }
    }

    private void spotPlayer()
    {
        StopCoroutine(roam());

        if (agent.enabled)
            agent.stoppingDistance = stoppingDistOrig;

        BecomeAlerted(gameManager.instance.player.transform.position);
        enemyManager.instance.AlertEnemiesWithinRange(GetAlertPosition(), detectionRange);
    }

    public void BecomeAlerted(Vector3 disturbancePos)
    {
        if (!hasBeenAlerted)
        {
            playerSpotted = true;

            AlertedEvent?.Invoke(gameObject);

            hasBeenAlerted = true;
        }
    }

    public void die()
    {
        if (isDead)
            return;

        soundManager.PlayDeathSpatial(transform.position);

        isDead = true;
        agent.enabled = false;
        enemyUI.SetActive(false);
        primaryCollider.enabled = false;

        enemyManager.instance.EnemyDied(gameObject, isMinion, countDeath);
        shouldDropLoot = canDropLoot && enemyManager.instance.EnemyCount == 0;

        if (!skipDeathAnimation && animator.HasState(0, Animator.StringToHash("Death")))
        {
            animator.SetTrigger("DeathTrigger");
        }
        else
        {
            DeathAnimationEnd();
        }
    }

    protected virtual void DeathAnimationEnd()
    {
        if (isDead)
        {
            OnDeath();

            // If it's the last enemy
            if (shouldDropLoot)
            {
                Vector3 lootPos = lootPosition != null ? lootPosition.transform.position : transform.position;

                // If the key was not dropped (i.e. it was the last "required" enemy), drop the key
                if (!gameManager.instance.IsKeyDropped)
                {
                    gameManager.instance.SpawnKey(lootPos);
                }
                // if the key was already dropped then the died enemy was the last ambush enemy, so drop the ambush reward
                else if (gameManager.instance.WasAmbushTriggered && !gameManager.instance.IsAmbushRewardDropped)
                {
                    gameManager.instance.SpawnAmbushReward(lootPos);
                }

            }

            Destroy(gameObject);
        }
    }

    protected virtual bool CanAttack()
    {
        // If player is within the attack range and unless already attacking, attack him
        return !isAttacking && canSeePlayerForAttack();
        //&& enemyManager.instance.attackingEnemiesCount <= enemyManager.instance.maxAttackingEnemies;
    }

    /// <summary>
    /// Start the attack.
    /// </summary>
    /// <returns></returns>
    protected virtual void Attack()
    {
        soundManager?.PlayAttackStart();
        isAttacking = true;

        // Make the enemy stand while attacking
        agent.SetDestination(transform.position);
        // Lock rotation on navigation agent
        agent.updateRotation = false;

        animator.SetTrigger("AttackTrigger");
        //enemyManager.instance.attackingEnemiesCount++;
    }

    /// <summary>
    /// Is called inside of the animation to execute the key attack action.
    /// (e.g., instantiate a bullet, shoot a raycast and do damage, etc.)
    /// <br>This is the method expected to be overriden.</br>
    /// </summary>
    protected virtual void AttackAnimationEvent()
    {
        soundManager?.PlayAttackMiddle();
        Vector3 distanceToPlayerFromShootPos = (gameManager.instance.player.transform.position - attackOrigin.transform.position);
        Quaternion bulletRot = Quaternion.LookRotation(distanceToPlayerFromShootPos);

        GameObject projectileInstance = Instantiate(projectile, attackOrigin.position, bulletRot);
        projectileInstance.GetComponent<projectile>().DamageValue = attackDamage;

        OnAttack();
    }


    protected virtual void AttackAnimationEnd()
    {
        isAttacking = false;

        // Unlock rotation on navigation agent
        agent.updateRotation = true;

        animator.ResetTrigger("AttackTrigger");

        //enemyManager.instance.attackingEnemiesCount--;
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
        return healthOriginal;
    }
}


