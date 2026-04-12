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

    // NEW: Reference to the Character Controller
    private CharacterController characterController;

    void Start()
    {
        m_Animator = GetComponent<Animator>();

        // Grab the component off the player
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

        m_Animator.SetFloat("Speed", moveDirection.magnitude);

        // --- HORIZONTAL MOVEMENT & ROTATION ---
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // OLD: transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
            // NEW: Use the CharacterController to move. This respects walls!
            characterController.Move(moveDirection * speed * Time.deltaTime);
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