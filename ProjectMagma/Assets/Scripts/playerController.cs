using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class playerController : MonoBehaviour
{
    [SerializeField] CharacterController controller;

    [SerializeField] int health;
    [SerializeField] float speed;

    [Header("Jumps & Gravity")]
    [Tooltip("The maximum number of jumps the player can perform before hitting the ground.")]
    [SerializeField] int jumpMaxNumber;
    [SerializeField] float jumpStrength;
    [SerializeField] float gravityStrength;
    [SerializeField] float maxVerticalSpeed;

    private Vector3 horMotionDirection;
    private Vector3 verticalVelocity;
    private bool isGrounded;
    private int jumpCount;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ProcessMovement();
    }

    void ProcessMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded)
        {
            jumpCount = 0;
        }

        // Get horizontal movement direction
        horMotionDirection = Input.GetAxis("Horizontal") * transform.right
            + Input.GetAxis("Vertical") * transform.forward;

        // Calculate horizontal motion
        Vector3 horMotion = horMotionDirection * speed * Time.deltaTime;

        // Apply horizontal motion
        controller.Move(horMotion);

        // Handle jumping
        if (Input.GetButtonDown("Jump") & jumpCount < jumpMaxNumber)
            Jump();

        // Apply gravity
        verticalVelocity.y += gravityStrength * Time.deltaTime;
        verticalVelocity.y = Mathf.Clamp(verticalVelocity.y, -maxVerticalSpeed, maxVerticalSpeed);

        // Apply vertical motion.
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    void Jump()
    {
        verticalVelocity.y = jumpStrength;
        jumpCount++;
    }
}
