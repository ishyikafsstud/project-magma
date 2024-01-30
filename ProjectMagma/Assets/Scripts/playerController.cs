using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class playerController : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [Header("Player Health")]
    [SerializeField] float health;
    [SerializeField] float healthRegenRate;
    [SerializeField] bool isInvincible;

    [Header("Walking & Running")]
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;

    [Header("Camera Tilting")]
    [SerializeField] Transform cameraTiltAnchor;
    [SerializeField] float maxCameraTilt;
    [SerializeField] float tiltCameraSpeed;

    [Header("Jumps & Gravity")]
    [Tooltip("The maximum number of jumps the player can perform before hitting the ground.")]
    [SerializeField] int jumpMaxNumber;
    [SerializeField] float jumpStrength;
    [SerializeField] float gravityStrength;
    [SerializeField] float maxVerticalSpeed;

    [Header("Shooting")]
    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;
    [SerializeField] float energy;
    [SerializeField] float energyCostPerShot;
    [SerializeField] float energyRegenRate;

    [Header("Melee")]
    [SerializeField] float meleeRange;
    [SerializeField] int meleeDamage;
    [SerializeField] float meleeRate;
    [SerializeField] float particleDuration;
    public GameObject hitParticlesPrefab;

    [Header("UI")]
    [Tooltip("The duration of screen flash upon receiving damage.")]
    [SerializeField] float damageFlashDuration;


    private float energyOriginal;
    private float healthOriginal;
    private Vector3 horMotionDirection;
    private Vector3 verticalVelocity;
    private float currentTiltAngle;
    private bool isGrounded;
    private int jumpCount;
    private bool sprinting;
    private float currentSpeed;
    private bool isShooting;
    private bool isMeleeActive;

    // Start is called before the first frame update
    void Start()
    {
        healthOriginal = health;
        energyOriginal = energy;
        currentSpeed = walkSpeed;

        updatePlayerUI();
        respawn();
    }

    // Update is called once per frame
    void Update()
    {
        processMovement();

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.green);

        if (!gameManager.instance.isPaused)
        {
            // Left Click - ranged attack
            if (Input.GetButton("Shoot") && !isShooting && energy > energyCostPerShot)
            {
                StartCoroutine(Shoot());
            }
            // Right click - melee attack
            else if (Input.GetButton("Hit") && !isMeleeActive && !isShooting)
            {
                StartCoroutine(MeleeAttack());
            }
        }

        RegenEnergy();
        healthRegen();
    }

    void processMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded)
        {
            jumpCount = 0;
            verticalVelocity.y = -0.0f; // ensure the player stays grounded
        }
        else
        {
            // apply gravity only when not grounded
            verticalVelocity.y += gravityStrength * Time.deltaTime;
            verticalVelocity.y = Mathf.Clamp(verticalVelocity.y, -maxVerticalSpeed, maxVerticalSpeed);
        }

        // Check for sprint
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded)
            enableSprint(true);
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            enableSprint(false);

        // Get horizontal movement direction
        horMotionDirection = Input.GetAxis("Horizontal") * transform.right
             + Input.GetAxis("Vertical") * transform.forward;
        horMotionDirection.Normalize(); // so that diagonal motion is not faster than straight motion

        // Calculate horizontal motion
        Vector3 horMotion = horMotionDirection * currentSpeed * Time.deltaTime;

        // Apply horizontal motion
        controller.Move(horMotion);

        // Call tiltCamera to handle camera tilt based on horizontal motion
        tiltCamera(horMotionDirection);

        // Handle jumping
        if (Input.GetButtonDown("Jump") & jumpCount < jumpMaxNumber)
            jump();

        // Apply vertical motion
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    void tiltCamera(Vector3 motion)
    {
        // Calculate lateral velocity
        float lateralVelocity = Vector3.Dot(motion, transform.right);

        // Calculate expectedTiltAngle based on lateral velocity
        float expectedTiltAngle = Mathf.Clamp(lateralVelocity * maxCameraTilt, -maxCameraTilt, maxCameraTilt);

        // Tilt camera angle towards expected camera angle over desired period of time
        currentTiltAngle = Mathf.Lerp(currentTiltAngle, -expectedTiltAngle, Time.deltaTime * tiltCameraSpeed);

        // Apply to cameraTiltAnchor
        cameraTiltAnchor.localRotation = Quaternion.Euler(0f, 0f, currentTiltAngle);

        // Debug.Log("Lateral Velocity: " + lateralVelocity);
        // Debug.Log("Expected Tilt Angle: " + expectedTiltAngle);
        // Debug.Log("Current Tilt Angle: " + currentTiltAngle);
    }

    void jump()
    {
        verticalVelocity.y = jumpStrength;
        jumpCount++;
    }

    void enableSprint(bool enable)
    {
        sprinting = enable;
        if (enable)
            currentSpeed = sprintSpeed;
        else
            currentSpeed = walkSpeed;
    }
    public void takeDamage(int amount)
    {
        if (!isInvincible)
            health -= amount;

        updatePlayerUI();

        StartCoroutine(flashDamageOnScreen());

        if (health <= 0)
        {
            die();
        }
    }
    IEnumerator Shoot()
    {
        isShooting = true;
        useEnergy(energyCostPerShot);

        if (energy < energyCostPerShot)
        {
            gameManager.instance.ShowHint("Not enough energy to shoot \nKeep Moving!");
            isShooting = false;
            yield break;
        }


        RaycastHit hit;
        // The layer masks of the collision layers we want the raycast to hit: Default, Enemy.
        // Using it specifies the layers we want the raycast to collide with.
        int layerMask = (1 << 0) | (1 << 6);
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDist, layerMask))
        {
            IDamage damagedBody = hit.collider.GetComponent<IDamage>();
            if (damagedBody != null && !hit.collider.CompareTag("Player"))
                damagedBody.takeDamage(shootDamage);
        }

        yield return new WaitForSeconds(shootRate); // Unity Timer

        isShooting = false;
    }

    IEnumerator MeleeAttack()
    {
        isMeleeActive = true;

        RaycastHit hit;
        // The layer masks of the collision layers we want the raycast to hit: Default, Enemy.
        // Using it specifies the layers we want the raycast to collide with.
        int layerMask = (1 << 0) | (1 << 6);
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, meleeRange, layerMask))
        {
            IDamage damagedBody = hit.collider.GetComponent<IDamage>();
            if (damagedBody != null && hit.collider.CompareTag("Enemy"))
            {
                damagedBody.takeDamage(meleeDamage);
            }
            SpawnHitParticles(hit.point);
        }
        // Delay between melee hits
        yield return new WaitForSeconds(meleeRate);

        isMeleeActive = false;
    }
    /// <summary>
    /// Melee Feedback
    /// </summary>
    /// <param name="position">Position of the hit particles.</param>
    void SpawnHitParticles(Vector3 position)
    {
        GameObject hitParticles = Instantiate(hitParticlesPrefab, position, Quaternion.identity);

        Destroy(hitParticles, particleDuration);
    }

    /// <summary>
    /// Health Regen when moving
    /// </summary>
    void healthRegen()
    {
        if (health > 0 && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            health += healthRegenRate * Time.deltaTime;
            health = Mathf.Clamp(health, 0, healthOriginal);
        }
        updatePlayerUI();
    }

    void useEnergy(float amount)
    {
        energy -= amount;
        updatePlayerUI();
    }

    /// <summary>
    /// Energy Regen when moving 
    /// </summary>
    void RegenEnergy()
    {
        if (currentSpeed > 0 && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            energy += energyRegenRate * Time.deltaTime;
            energy = Mathf.Clamp(energy, 0, energyOriginal);
        }
        updatePlayerUI();

    }
    void die()
    {
        gameManager.instance.scenarioPlayerLoses();
    }

    public void respawn()
    {
        health = healthOriginal;
        energy = energyOriginal;
        updatePlayerUI();

        controller.enabled = false;
        if (gameManager.instance.playerSpawnPosition != null)
            transform.position = gameManager.instance.playerSpawnPosition.transform.position;
        controller.enabled = true;
    }

    void updatePlayerUI()
    {
        //health bar update
        gameManager.instance.playerHealthbar.fillAmount = (float)health / healthOriginal;
        if (energyOriginal > 0.0f)
            //energy bar update
            gameManager.instance.playerEnergybar.fillAmount = (float)energy / energyOriginal;
    }

    IEnumerator flashDamageOnScreen()
    {
        gameManager.instance.playerDamageScreenFlash.SetActive(true);
        yield return new WaitForSeconds(damageFlashDuration);
        gameManager.instance.playerDamageScreenFlash.SetActive(false);
    }

    public float GetHealth()
    {
        return health;
    }

    public float GetOriginalHealth()
    {
        return healthOriginal;
    }
}
