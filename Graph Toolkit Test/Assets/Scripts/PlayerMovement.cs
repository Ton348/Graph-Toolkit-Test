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
    [SerializeField] private Animator m_animator;

    private CharacterController m_controller;
    private Vector3 m_velocity;

    private void Awake()
    {
        m_controller = GetComponent<CharacterController>();
        if (m_animator == null)
        {
            m_animator = GetComponentInChildren<Animator>();
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

        m_controller.Move(move * currentSpeed * Time.deltaTime);

        // Gravity
        if (m_controller.isGrounded && m_velocity.y < 0f)
        {
            m_velocity.y = -2f;
        }

        m_velocity.y += gravity * Time.deltaTime;
        m_controller.Move(Vector3.up * m_velocity.y * Time.deltaTime);

        // Animation (bool params: idle/walk)
        if (m_animator != null)
        {
            bool isMoving = Mathf.Abs(vertical) > 0.01f;
            m_animator.SetBool("idle", !isMoving);
            m_animator.SetBool("walk", isMoving);
        }
    }
}
