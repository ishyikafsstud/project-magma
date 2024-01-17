using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class playerController : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;

    [SerializeField] int health;

    [Header("Walking & Running")]
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;

    [Header("Jumps & Gravity")]
    [Tooltip("The maximum number of jumps the player can perform before hitting the ground.")]
    [SerializeField] int jumpMaxNumber;
    [SerializeField] float jumpStrength;
    [SerializeField] float gravityStrength;
    [SerializeField] float maxVerticalSpeed;
    
    [Header("Damage")]
    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;

    private int healthOriginal;
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
        currentSpeed = walkSpeed;

        updatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        processMovement();
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.green);

        if (Input.GetButton("Shoot") && !isShooting)
        {
            StartCoroutine(Shoot());
        }
        
    }

    void processMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded)
        {
            jumpCount = 0;
            verticalVelocity.y = -0.5f; // ensure the player stays grounded
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

        // Calculate horizontal motion
        Vector3 horMotion = horMotionDirection * currentSpeed * Time.deltaTime;

        // Apply horizontal motion
        controller.Move(horMotion);

        // Handle jumping
        if (Input.GetButtonDown("Jump") & jumpCount < jumpMaxNumber)
            jump();

        // Apply vertical motion
        controller.Move(verticalVelocity * Time.deltaTime);

        // Apply vertical motion.
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

        if (health <= 0)
        {
            die();
        }
    }
    IEnumerator Shoot()
    {
        isShooting = true;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDist))
        {
            IDamage damagedBody = hit.collider.GetComponent<IDamage>();
            if (damagedBody != null)
                damagedBody.takeDamage(shootDamage);
        }

        yield return new WaitForSeconds(shootRate); // Unity Timer

        isShooting = false;
    }
    void die()
    {
        gameManager.instance.scenarioPlayerLoses();
    }

    void updatePlayerUI()
    {
        gameManager.instance.playerHealthbar.fillAmount = (float)health / healthOriginal;
    }
}
