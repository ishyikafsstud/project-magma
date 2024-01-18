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

    [Header("Walking & Running")]
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;

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

    [Header("UI")]
    [Tooltip("The duration of screen flash upon receiving damage.")]
    [SerializeField] float damageFlashDuration;

    private float energyOriginal;
    private float healthOriginal;
    private Vector3 horMotionDirection;
    private Vector3 verticalVelocity;
    private bool isGrounded;
    private int jumpCount;
    private bool sprinting;
    private float currentSpeed;
    private bool isShooting;

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

        if (Input.GetButton("Shoot") && !isShooting && energy > energyCostPerShot && !gameManager.instance.isPaused)
        {
            StartCoroutine(Shoot());
        }
        else
        {
            RegenEnergy();
            healthRegen();
        }

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


        // Get horizontal movement direction
        horMotionDirection = Input.GetAxis("Horizontal") * transform.right
             + Input.GetAxis("Vertical") * transform.forward;
        horMotionDirection.Normalize(); // so that diagonal motion is not faster than straight motion

        // Calculate horizontal motion
        Vector3 horMotion = horMotionDirection * currentSpeed * Time.deltaTime;

        // Apply horizontal motion
        controller.Move(horMotion);

        // Handle jumping
        if (Input.GetButtonDown("Jump") & jumpCount < jumpMaxNumber)
            jump();

        // Apply vertical motion
        controller.Move(verticalVelocity * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            sprint(true);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            sprint(false);
        }
    }

    void jump()
    {
        verticalVelocity.y = jumpStrength;
        jumpCount++;
    }

    void sprint(bool enable)
    {
        sprinting = !sprinting;
        if (enable && isGrounded)
            currentSpeed = sprintSpeed;
        else
            currentSpeed = walkSpeed;
    }
    public void takeDamage(int amount)
    {
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
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDist))
        {
            IDamage damagedBody = hit.collider.GetComponent<IDamage>();
            if (damagedBody != null && !hit.collider.CompareTag("Player"))
                damagedBody.takeDamage(shootDamage);
        }

        yield return new WaitForSeconds(shootRate); // Unity Timer

        isShooting = false;
    }

    void healthRegen()
    {
        if(health > 0 && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
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
}
