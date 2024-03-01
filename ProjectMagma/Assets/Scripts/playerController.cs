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

    [Header("----- Primary Stats -----")]
    [Tooltip("Do not directly access private health - use the public Health property instead.")]
    [SerializeField] float health;
    //[SerializeField] float healthRegenRate;
    [SerializeField] bool isInvincible;
    [SerializeField] float energy;
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

    [Header("----- Primary Attack -----")]
    [SerializeField] List<weaponStats> weaponList = new List<weaponStats>();
    /// <summary>
    /// Return read-only version of the weapon list.
    /// </summary>
    public System.Collections.ObjectModel.ReadOnlyCollection<weaponStats> GetWeaponList() { return weaponList.AsReadOnly(); }

    [SerializeField] GameObject weaponPosition;
    [SerializeField] GameObject shootPosition;
    [SerializeField] GameObject vfxPosition;
    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;
    [SerializeField] float energyCostPerShot;
    [SerializeField] float energyRegenRate;
    [SerializeField] float energyIncreasePerAmbush;

    [Header("---- Alternative Attack ----")]
    [Tooltip("Meant to be toggled as a quick way to enable/disable the feature.")]
    [SerializeField] bool allowAltAttack = false;

    [Header("----- UI -----")]
    [Tooltip("The duration of screen flash upon receiving damage.")]
    [SerializeField] float damageFlashDuration;

    [Header("----- Audio -----")]
    [SerializeField] entitySoundManager soundManager;
    [SerializeField] inventorySoundManager inventorySoundManager;
    [Tooltip("The SFX of the weapon that is in player's hands. Sound clip updates automatically on player pickup.")]
    [SerializeField] AudioSource weaponAudioSource;


    private int powerupsApplied;
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
    
    private GameObject currentVFX;
    private int selectedWeapon;

    public int SelectedWeapon
    {
        get => weaponList.Count > 0 ? selectedWeapon : -1;
    }
    public weaponStats SelectedWeaponStats
    {
        get => weaponList.Count > 0 ? weaponList[selectedWeapon] : null;
    }
    private bool isShooting;
    private bool isAltActive;

    public delegate void PlayerAction();
    public event PlayerAction SpawnedEvent;
    public event PlayerAction Hurt;
    public event PlayerAction Healed;

    public delegate void WeaponAction(weaponStats weapon);
    public event WeaponAction WeaponSwitched;

    public delegate void StatsUpdate(float value, float maxValue);
    public event StatsUpdate EnergyChanged;
    public event StatsUpdate HealthChanged;

    public float Health
    {
        get => health;
        set
        {
            value = Mathf.Clamp(value, 0, healthOriginal);
            if (health != value)
            {
                health = value;
                HealthChanged?.Invoke(health, healthOriginal);
            }
        }
    }
    public float Energy
    {
        get => energy;
        set
        {
            value = Mathf.Clamp(value, 0, energyOriginal);
            if (energy != value)
            {
                energy = value;
                EnergyChanged?.Invoke(energy, energyOriginal);
            }
        }
    }
    public int PowerupsApplied { get => powerupsApplied; }


    /// <summary>
    /// Apply ambush defeat reward powerup.
    /// </summary>
    /// <param name="stacks">How many stacks of the powerup to apply.</param>
    public void ApplyAmbushDefeatPowerup(int stacks = 1)
    {
        powerupsApplied += stacks;
        float energyIncrease = energyIncreasePerAmbush * stacks;

        energyOriginal += energyIncrease;
        Energy += energyIncrease;
    }

    // Start is called before the first frame update
    void Start()
    {
        healthOriginal = health;
        energyOriginal = energy;
        currentSpeed = walkSpeed;
        walkToSprintSpeedRatio = walkSpeed / sprintSpeed;

        saveSystem.TiltSet += EnableTilt;

        SpawnedEvent?.Invoke();
        HealthChanged?.Invoke(health, healthOriginal); // Force-update all listeners
        EnergyChanged?.Invoke(energy, energyOriginal); // Force-update all listeners
        // Force-update all listeners with the currently selected weapon
        WeaponSwitched?.Invoke(weaponList.Count > 0 ? weaponList[selectedWeapon] : null);

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
            }

            // Left Click - ranged attack
            if (Input.GetButton("Shoot") && weaponList.Count > 0 && !isShooting)
            {
                if (Energy >= energyCostPerShot)
                    StartCoroutine(Shoot());
                else
                {
                    gameManager.instance.ShowHint("Not enough energy to shoot \nKeep Moving!");
                }
            }
            //Right click -alt attack
            else if (allowAltAttack && Input.GetButton("Hit") && !isAltActive && !isShooting)
            {
                StartCoroutine(AltAttack());
            }

            //if (Input.GetKeyDown(KeyCode.X))
            //{
            //    dropWeapon(selectedWeapon);
            //}
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
        if (canTilt)
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
        if (!canTilt)
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

        Hurt?.Invoke();

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
        //soundManager?.PlayAttackStart();

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

        gameManager.instance.reloadHUD.Reload(shootRate);

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
        else
        {
            Vector3 viewportCenter = new Vector3(0.5f, 0.5f, shootDist);
            Vector3 raycastEndPos = Camera.main.ViewportToWorldPoint(viewportCenter) + Camera.main.transform.forward * shootDist;
            Instantiate(weaponList[selectedWeapon].hitEffect, raycastEndPos, weaponList[selectedWeapon].hitEffect.transform.rotation);
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
        if (selectedWeapon < 0 || selectedWeapon >= weaponList.Count)
        {
            yield break;
        }
        isAltActive = true;
        altAttackCollider.enabled = true;

        RaycastHit hit;
        // The layer masks of the collision layers we want the raycast to hit: Default, Enemy.
        // Using it specifies the layers we want the raycast to collide with.
        int layerMask = (1 << 0) | (1 << 6);
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, weaponList[selectedWeapon].altRange, layerMask))
        {
            // Applys damage and push force to the hit enemy
            ApplyDamageAndPush(hit.collider);
            SpawnHitParticles(hit.point);

            // Gets all colliders within a radius around the hit point
            Collider[] colliders = Physics.OverlapSphere(hit.point, weaponList[selectedWeapon].pushRadius, layerMask);
            foreach (Collider collider in colliders)
            {
                if (collider != hit.collider) // Skips the collider that was hit by the raycast
                {
                    // Applys damage and push force to each nearby enemy
                    ApplyDamageAndPush(collider);
                }
            }
        }
        else
        {
            // If the raycast doesn't hit anything,this calculates a point based on the ray's direction and length
            Vector3 rayDirection = Camera.main.transform.forward;
            Vector3 rayEndPoint = Camera.main.transform.position + (rayDirection * weaponList[selectedWeapon].altRange);
            // Applys damage and push force to all enemies within the calculated range
            Collider[] colliders = Physics.OverlapSphere(rayEndPoint, weaponList[selectedWeapon].pushRadius, layerMask);
            foreach (Collider collider in colliders)
            {
                ApplyDamageAndPush(collider);
            }
            // Spawns hit particles at the calculated point
            SpawnHitParticles(rayEndPoint);
        }
        // Delay between hits
        Debug.Log("Waiting for " + weaponList[selectedWeapon].altRate + " seconds");
        yield return new WaitForSeconds(weaponList[selectedWeapon].altRate);
        Debug.Log("Delay completed");

        isAltActive = false;
        altAttackCollider.enabled = false;
    }
    void ApplyDamageAndPush(Collider collider)
    {
        IDamage damagedBody = collider.GetComponent<IDamage>();
        if (damagedBody != null && collider.CompareTag("Enemy"))
        {
            damagedBody.takeDamage(weaponList[selectedWeapon].altDamage);

            Vector3 direction = (collider.transform.position - transform.position).normalized;
            float force = weaponList[selectedWeapon].pushForce;

            collider.GetComponent<IPushable>()?.Push(direction, force);
        }
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

    public void Heal(float value)
    {
        Health += value;
        Healed?.Invoke();
    }

    void useEnergy(float amount)
    {
        Energy -= amount;
    }

    /// <summary>
    /// Energy Regen when moving 
    /// </summary>
    void RegenEnergy()
    {
        // Calculate the base energy regenerated based on speed
        float baseEnergyRegenerated = Mathf.Clamp((currentSpeed / sprintSpeed) * energyRegenRate, 0, energyRegenRate);

        // Adjust energy regeneration if sprinting
        float adjustedEnergyRegenerated = sprinting ? baseEnergyRegenerated * 2f : baseEnergyRegenerated;

        if (currentSpeed > 0 && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            Energy += adjustedEnergyRegenerated * Time.deltaTime;
        }
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

    public float GetHealth()
    {
        return Health;
    }

    public float GetOriginalHealth()
    {
        return healthOriginal;
    }

    void selectWeapon()
    {
        // Switch weapons using number keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (weaponList.Count > 0 && selectedWeapon != 0)
            {
                EquipWeaponFromSlot(0);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (weaponList.Count > 1 && selectedWeapon != 1)
            {
                EquipWeaponFromSlot(1);
            }
        }

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

    public void EquipWeaponFromSlot(int slot, bool playSound = true)
    {
        if (weaponList.Count == 0)
            return;

        selectedWeapon = Mathf.Clamp(slot, 0, weaponList.Count - 1);
        changeWeapon(playSound);
    }

    void changeWeapon(bool playSound = true)
    {
        weaponStats currentWeapon = weaponList[selectedWeapon];

        // Set weapon stats
        shootDamage = weaponList[selectedWeapon].shootDamage;
        shootDist = weaponList[selectedWeapon].shootDist;
        shootRate = weaponList[selectedWeapon].shootRate;
        energyCostPerShot = weaponList[selectedWeapon].energyCostPerShot;

        // Update weapon mesh
        MeshFilter weaponMeshFilter = currentWeapon.model.GetComponentInChildren<MeshFilter>();
        weaponPosition.GetComponent<MeshFilter>().sharedMesh = weaponMeshFilter.sharedMesh;
        MeshRenderer weaponMeshRenderer = currentWeapon.model.GetComponentInChildren<MeshRenderer>();
        weaponPosition.GetComponent<MeshRenderer>().sharedMaterial = weaponMeshRenderer.sharedMaterial;

        // Update wand VFX
        if (currentVFX != null)
        {
            Destroy(currentVFX);
        }
        currentVFX = Instantiate(currentWeapon.staffVFX, vfxPosition.transform.position, vfxPosition.transform.rotation);
        currentVFX.transform.parent = vfxPosition.transform;

        // Extra
        WeaponSwitched?.Invoke(weaponList[selectedWeapon]);
        if (playSound)
            inventorySoundManager.PlayWeaponSwitched();
    }

    public void pickupWeapon(weaponStats weapon, bool playSound = true)
    {
        if (weapon == null)
            return;

        int newWeaponIndex = Mathf.Clamp(weaponList.Count, 0, 1);

        // If the inventory is full, drop the current selected weapon and remember to insert the new weapon in
        // place of the dropped weapon
        if (weaponList.Count == 2)
        {
            dropWeapon(selectedWeapon);
            newWeaponIndex = selectedWeapon;
            inventorySoundManager.PlayWeaponSwitched();
        }

        weaponList.Insert(newWeaponIndex, weapon);

        // Set weapon stats
        shootDamage = weapon.shootDamage;
        shootDist = weapon.shootDist;
        shootRate = weapon.shootRate;
        energyCostPerShot = weapon.energyCostPerShot;

        // Update weapon mesh
        MeshFilter weaponMeshFilter = weapon.model.GetComponentInChildren<MeshFilter>();
        weaponPosition.GetComponent<MeshFilter>().sharedMesh = weaponMeshFilter.sharedMesh;
        MeshRenderer weaponMeshRenderer = weapon.model.GetComponentInChildren<MeshRenderer>();
        weaponPosition.GetComponent<MeshRenderer>().sharedMaterial = weaponMeshRenderer.sharedMaterial;

        // Update wand VFX
        if (currentVFX != null)
        {
            Destroy(currentVFX);
        }
        currentVFX = Instantiate(weapon.staffVFX, vfxPosition.transform.position, vfxPosition.transform.rotation);
        currentVFX.transform.parent = vfxPosition.transform;

        // Extra
        selectedWeapon = newWeaponIndex;
        WeaponSwitched?.Invoke(weaponList[selectedWeapon]);
        if (playSound)
            inventorySoundManager.PlayWeaponPicked();
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

        if (currentVFX != null)
        {
            Destroy(currentVFX);
        }

        // For now, the wand drops behind the player so that the player doesnt keep colliding with it
        float dropDistance = 0.5f;
        Vector3 dropPosition = transform.position + transform.forward * dropDistance;

        Instantiate(correctWandItem, dropPosition, Quaternion.identity);
        inventorySoundManager.PlayWeaponDroppedSpatial(dropPosition);
    }
}
