using UnityEngine;

// This line forces Unity to add a CharacterController if you forgot!
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 10.0f;
    public float rotationSpeed = 720.0f;
    public float gravity = -9.81f; // Added gravity

    private float horizontalInput;
    private float verticalInput;
    private Vector3 velocity; // Stores vertical momentum (falling)

    private Animator m_Animator;
    private Transform mainCameraTransform;

    // Reference to the Character Controller
    private CharacterController characterController;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("No camera tagged as 'MainCamera' found!");
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // --- CHECK SPRINTING ---
        // Checks if Left or Right Shift is held, AND if the player is moving forward (Up Arrow or W)
        bool isHoldingShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isMovingForward = verticalInput > 0.1f;

        bool isSprinting = isHoldingShift && isMovingForward;

        // Double the speed if sprinting
        float currentSpeed = isSprinting ? (speed * 2f) : speed;

        // --- CAMERA-RELATIVE MATH ---
        Vector3 camForward = mainCameraTransform.forward;
        Vector3 camRight = mainCameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * verticalInput) + (camRight * horizontalInput);

        if (moveDirection.magnitude > 1)
        {
            moveDirection.Normalize();
        }

        // Multiply the animation speed parameter so the character animates faster when sprinting
        float animationSpeed = moveDirection.magnitude * (isSprinting ? 2f : 1f);
        m_Animator.SetFloat("Speed", animationSpeed);

        // --- HORIZONTAL MOVEMENT & ROTATION ---
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Use currentSpeed instead of the base speed
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        // --- VERTICAL MOVEMENT (GRAVITY) ---
        // If she is touching the ground, stop pushing her down infinitely
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // A small constant downward force keeps her snapped to slopes
        }

        // Apply gravity math
        velocity.y += gravity * Time.deltaTime;

        // Move her down
        characterController.Move(velocity * Time.deltaTime);
    }
}