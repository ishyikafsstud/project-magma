using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class playerController : MonoBehaviour, IDamage
{
    [Header("----- Components -----")]
    [SerializeField] CharacterController controller;
    
    public Collider altAttackCollider;
    
    [Header("----- Player Health -----")]
    [Tooltip("Do not directly access private health - use the public Health property instead.")]
    [SerializeField] float health;
    //[SerializeField] float healthRegenRate;
    [SerializeField] bool isInvincible;
    [SerializeField] bool hasInfiniteEnergy;

    [Header("----- Walking & Running -----")]
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;

    [Header("----- Camera Tilting -----")]
    [SerializeField] Transform cameraTiltAnchor;
    [SerializeField] float maxCameraTilt;
    [SerializeField] float tiltCameraSpeed;
    [SerializeField] private bool canTilt = true;

    [Header("----- Jumps & Gravity -----")]
    [Tooltip("The maximum number of jumps the player can perform before hitting the ground.")]
    [SerializeField] int jumpMaxNumber;
    [SerializeField] float jumpStrength;
    [SerializeField] float gravityStrength;
    [SerializeField] float maxVerticalSpeed;

    [Header("----- Shooting -----")]
    [SerializeField] List<weaponStats> weaponList = new List<weaponStats>();
    /// <summary>
    /// Return read-only version of the weapon list.
    /// </summary>
    public System.Collections.ObjectModel.ReadOnlyCollection<weaponStats> GetWeaponList() { return weaponList.AsReadOnly(); }

    [SerializeField] GameObject weaponPosition;
    [SerializeField] GameObject shootPosition;
    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;
    [SerializeField] float energy;
    [SerializeField] float energyCostPerShot;
    [SerializeField] float energyRegenRate;
    [SerializeField] float energyIncreasePerAmbush;

    [Header("----- UI -----")]
    [Tooltip("The duration of screen flash upon receiving damage.")]
    [SerializeField] float damageFlashDuration;

    [Header("----- Audio -----")]
    [SerializeField] soundManager soundManager;
    [SerializeField] AudioSource weaponAudioSource;

    private float energyOriginal;
    private float energyRegenerated;
    private float healthOriginal;
    private Vector3 horMotionDirection;
    private Vector3 verticalVelocity;
    private float currentTiltAngle;
    private bool isGrounded;
    private int jumpCount;
    private bool sprinting;
    private float currentSpeed;
    private float walkToSprintSpeedRatio;
    private int selectedWeapon;
    private bool isShooting;
    private bool isAltActive;

    public float Health
    {
        get => health;
        set => health = Mathf.Clamp(value, 0, healthOriginal);
    }

    public delegate void PlayerAction();
    public event PlayerAction PlayerSpawnedEvent;

    /// <summary>
    /// Apply ambush defeat reward powerup.
    /// </summary>
    /// <param name="stacks">How many stacks of the powerup to apply.</param>
    public void ApplyAmbushDefeatPowerup(int stacks = 1)
    {
        energyOriginal += energyIncreasePerAmbush * stacks;
    }

    // Start is called before the first frame update
    void Start()
    {
        healthOriginal = health;
        energyOriginal = energy;
        currentSpeed = walkSpeed;
        walkToSprintSpeedRatio = walkSpeed / sprintSpeed;

        PlayerSpawnedEvent?.Invoke();
        
        updatePlayerUI();
        //respawn();
    }

    // Update is called once per frame
    void Update()
    {
        processMovement();
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.green);

        if (!gameManager.instance.isPaused)
        {
            if (weaponList.Count > 0)
            {
                selectWeapon();
                gameManager.instance.playerEnergybar.gameObject.SetActive(true);
            }
            else
            {
                gameManager.instance.playerEnergybar.gameObject.SetActive(false);
            }
            // Left Click - ranged attack
            if (Input.GetButton("Shoot") && weaponList.Count > 0 && !isShooting)
            {
                if (energy >= energyCostPerShot)
                    StartCoroutine(Shoot());
                else
                {
                    gameManager.instance.ShowHint("Not enough energy to shoot \nKeep Moving!");
                }
            }
            // Right click - alt attack
            else if (Input.GetButton("Hit") && !isAltActive && !isShooting)
            {
                StartCoroutine(AltAttack());
            }

            if(Input.GetKeyDown(KeyCode.X))
            {
                dropWeapon(selectedWeapon);
            }
        }

        RegenEnergy();
        //healthRegenOnMovement();
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
        if(canTilt)
        {
            tiltCamera(horMotionDirection);
        }

        // Handle jumping
        if (Input.GetButtonDown("Jump") & jumpCount < jumpMaxNumber)
            jump();

        // Apply vertical motion
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    /// <summary>
    /// Tilt camera based on the lateral (sideways) velocity.
    /// </summary>
    /// <param name="motion">Horizontal motion.</param>
    void tiltCamera(Vector3 motion)
    {
        // Calculate lateral velocity
        float lateralVelocity = Vector3.Dot(motion, transform.right);

        // Camera tilt for running should be more significant than for walking
        if (currentSpeed == walkSpeed)
            lateralVelocity *= walkToSprintSpeedRatio;

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

    public void EnableTilt(bool enable)
    {
        canTilt = enable;
        if(!canTilt)
        {
            cameraTiltAnchor.localRotation = Quaternion.identity;
        }
    }

    void jump()
    {
        verticalVelocity.y = jumpStrength;
        soundManager?.PlayJump();
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
            Health -= amount;

        updatePlayerUI();

        StartCoroutine(flashDamageOnScreen());

        if (Health <= 0)
        {
            die();
        }
        soundManager?.PlayHurt();
    }

    public IEnumerator ApplyFreeze(int stacks)
    {
        yield break;
    }

    IEnumerator Shoot()
    {
        isShooting = true;

        weaponAudioSource.PlayOneShot(weaponList[selectedWeapon].shootSound);
        soundManager?.PlayAttackStart();

        if (!hasInfiniteEnergy)
            useEnergy(energyCostPerShot);

        switch (weaponList[selectedWeapon].weaponType)
        {
            case weaponStats.WeaponTypes.Raycast:
                ShootRaycast();
                break;

            case weaponStats.WeaponTypes.Projectile:
                ShootProjectile();
                break;
        }

        yield return new WaitForSeconds(shootRate);

        isShooting = false;
    }

    void ShootRaycast()
    {
        RaycastHit hit;
        // The layer masks of the collision layers we want the raycast to hit: Default, Enemy.
        // Using it specifies the layers we want the raycast to collide with.
        int layerMask = (1 << 0) | (1 << 6);
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDist, layerMask))
        {
            IDamage damagedBody = hit.collider.GetComponent<IDamage>();
            if (damagedBody != null && !hit.collider.CompareTag("Player"))
            {
                damagedBody.takeDamage(shootDamage);
            }
            Instantiate(weaponList[selectedWeapon].hitEffect, hit.point, weaponList[selectedWeapon].hitEffect.transform.rotation);
        }
    }

    // will possibly need aditional setup. not in use yet.
    void ShootProjectile()
    {
        if (weaponList[selectedWeapon].projectilePrefab == null)
            return;

        // Instantiate Projectile
        GameObject projectileInstance = Instantiate(weaponList[selectedWeapon].projectilePrefab, shootPosition.transform.position, Camera.main.transform.rotation);

        // Access the Projectile script attached to the instantiated projectile GameObject
        projectile projectileScript = projectileInstance.GetComponent<projectile>();

        // Set projectile properties
        projectileScript.DamageValue = weaponList[selectedWeapon].shootDamage;
    }

    IEnumerator AltAttack()
    {
        isAltActive = true;
        altAttackCollider.enabled = true;

        RaycastHit hit;
        // The layer masks of the collision layers we want the raycast to hit: Default, Enemy.
        // Using it specifies the layers we want the raycast to collide with.
        int layerMask = (1 << 0) | (1 << 6);
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, weaponList[selectedWeapon].altRange, layerMask))
        {
            IDamage damagedBody = hit.collider.GetComponent<IDamage>();
            if (damagedBody != null && hit.collider.CompareTag("Enemy"))
            {
                damagedBody.takeDamage(weaponList[selectedWeapon].altDamage);

                // Calculates the direction from the player to the enemy
                Vector3 direction = (hit.collider.transform.position - transform.position).normalized;

                // Accesses the push force from the WeaponStats of the selected weapon
                float force = weaponList[selectedWeapon].pushForce;

                // Calls the Push method on the enemy object
                hit.collider.GetComponent<IPushable>()?.Push(direction, force);
            }
            SpawnHitParticles(hit.point);
        }
        // Delay between melee hits
        yield return new WaitForSeconds(weaponList[selectedWeapon].altRate);
        
        isAltActive = false;
        altAttackCollider.enabled = false;
    }
    /// <summary>
    /// Melee Feedback
    /// </summary>
    /// <param name="position">Position of the hit particles.</param>
    void SpawnHitParticles(Vector3 position)
    {
        GameObject hitParticles = Instantiate(weaponList[selectedWeapon].hitParticlePrefab, position, Quaternion.identity);

        Destroy(hitParticles, weaponList[selectedWeapon].particleDuration);
    }

    ///// <summary>
    ///// Health Regen when moving
    ///// </summary>
    //void healthRegenOnMovement()
    //{
    //    if (health > 0 && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
    //    {
    //        health += healthRegenRate * Time.deltaTime;
    //        health = Mathf.Clamp(health, 0, healthOriginal);
    //    }
    //    updatePlayerUI();
    //}

    public void Heal(int value)
    {
        Health += value;
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
        // Calculate the base energy regenerated based on speed
        float baseEnergyRegenerated = Mathf.Clamp(((currentSpeed / sprintSpeed) + (verticalVelocity.y / jumpStrength)) * energyRegenRate, energyRegenRate, 0);

        // Adjust energy regeneration if sprinting
        float adjustedEnergyRegenerated = sprinting ? baseEnergyRegenerated * 2f : baseEnergyRegenerated;

        if (currentSpeed > 0 && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            energy += adjustedEnergyRegenerated * Time.deltaTime;
            energy = Mathf.Clamp(energy, 0, energyOriginal);
        }
        updatePlayerUI();

    }
    void die()
    {
        gameManager.instance.scenarioPlayerLoses();
        soundManager?.PlayDeath();
    }

    //public void respawn()
    //{
    //    health = healthOriginal;
    //    energy = energyOriginal;
    //    updatePlayerUI();

    //    controller.enabled = false;
    //    if (gameManager.instance.playerSpawnPosition != null)
    //        transform.position = gameManager.instance.playerSpawnPosition.transform.position;
    //    controller.enabled = true;
    //}

    public void updatePlayerUI()
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

    void selectWeapon()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && selectedWeapon < weaponList.Count - 1)
        {
            selectedWeapon++;
            changeWeapon();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedWeapon > 0)
        {
            selectedWeapon--;
            changeWeapon();
        }
    }

    void changeWeapon()
    {
        weaponStats currentWeapon = weaponList[selectedWeapon];
        
        shootDamage = weaponList[selectedWeapon].shootDamage;
        shootDist = weaponList[selectedWeapon].shootDist;
        shootRate = weaponList[selectedWeapon].shootRate;
        energyCostPerShot = weaponList[selectedWeapon].energyCostPerShot;
        
        weaponPosition.GetComponent<MeshFilter>().sharedMesh = weaponList[selectedWeapon].model.GetComponent<MeshFilter>().sharedMesh;
        weaponPosition.GetComponent<MeshRenderer>().sharedMaterial = weaponList[selectedWeapon].model.GetComponent<MeshRenderer>().sharedMaterial;
    }

    public void pickupWeapon(weaponStats weapon)
    {
        if (weapon == null)
            return;

        int newWeaponIndex = weaponList.Count;

        // If the inventory is full, drop the current selected weapon and remember to insert the new weapon in
        // place of the dropped weapon
        if (weaponList.Count == 2)
        {
            dropWeapon(selectedWeapon);
            newWeaponIndex = selectedWeapon;
        }

        weaponList.Insert(newWeaponIndex, weapon);

        shootDamage = weapon.shootDamage;
        shootDist = weapon.shootDist;
        shootRate = weapon.shootRate;
        energyCostPerShot = weapon.energyCostPerShot;

        weaponPosition.GetComponent<MeshFilter>().sharedMesh = weapon.model.GetComponent<MeshFilter>().sharedMesh;
        weaponPosition.GetComponent<MeshRenderer>().sharedMaterial = weapon.model.GetComponent<MeshRenderer>().sharedMaterial;

        selectedWeapon = weaponList.Count - 1;
    }

    void dropWeapon(int weaponIndex)
    {
        weaponStats.WandType wandType = weaponList[weaponIndex].wandType;
        GameObject correctWandItem = gameManager.instance.Helper.weaponItemsList[(int)wandType];

        if (weaponIndex == selectedWeapon)
        {
            shootDamage = 0;
            shootDist = 0;
            shootRate = 0;
            energyCostPerShot = 0;
            weaponPosition.GetComponent<MeshFilter>().sharedMesh = null;
            weaponPosition.GetComponent<MeshRenderer>().sharedMaterial = null;
        }

        weaponList.RemoveAt(weaponIndex);

        // Drop offset will change once button to pickup wands is implemented.
        // For now, the wand drops behind the player so that the player doesnt keep colliding with it
        int dropDistance = -2;
        Vector3 dropPosition = transform.position + transform.forward * dropDistance;

        Instantiate(correctWandItem, dropPosition, Quaternion.identity);
    }
}
