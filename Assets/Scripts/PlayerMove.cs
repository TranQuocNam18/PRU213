using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 6f;

    public Transform groundCheck;
    public LayerMask groundLayer;

    Rigidbody rb;
    Animator animator;
    PlayerStamina stamina;

    bool isGrounded;
    Vector3 moveInput;
    bool isMoving;
    bool isRunning;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        stamina = GetComponent<PlayerStamina>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // ===== INPUT =====
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        moveInput = transform.forward * v + transform.right * h;
        isMoving = moveInput.magnitude > 0.1f;

        bool wantsRun = Input.GetKey(KeyCode.LeftShift);
        isRunning = wantsRun && isMoving && stamina.stamina > 0;

        // ===== GROUND CHECK =====
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            0.2f,
            groundLayer
        );

        // ===== JUMP =====
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // ===== ANIMATION (PHẢI Ở UPDATE) =====
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", isRunning);

    }

    void FixedUpdate()
    {
        float speed = walkSpeed;

        if (isRunning)
        {
            speed = runSpeed;
            stamina.UseStamina(20f * Time.fixedDeltaTime);
        }

        rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
    }
}
