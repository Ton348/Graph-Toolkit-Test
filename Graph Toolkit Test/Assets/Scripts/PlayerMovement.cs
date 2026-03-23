using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float runSpeed = 7f;
    public float rotationSpeed = 120f;
    public float gravity = -9.81f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private Vector3 velocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float vertical = Input.GetAxis("Vertical"); // W/S
        float horizontal = Input.GetAxis("Horizontal"); // A/D

        // Rotation (A/D)
        transform.Rotate(Vector3.up * horizontal * rotationSpeed * Time.deltaTime);

        // Forward/back movement (W/S)
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        Vector3 move = transform.forward * vertical;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // Gravity
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(Vector3.up * velocity.y * Time.deltaTime);

        // Animation (bool params: idle/walk)
        if (animator != null)
        {
            bool isMoving = Mathf.Abs(vertical) > 0.01f;
            animator.SetBool("idle", !isMoving);
            animator.SetBool("walk", isMoving);
        }
    }
}
