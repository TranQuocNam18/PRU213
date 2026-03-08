using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Move")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 4.5f;
    public float jumpForce = 6f;
    public bool canMove = true;

    [Header("Ground")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundRadius = 0.25f;

    [Header("Crouch")]
    public float crouchHeight = 1.0f;
    [Header("Crouch Camera")]
    float originalCamHeight;
    public Transform cameraHolder;
    public float crouchCamOffset = 0.5f;
    public float camLerpSpeed = 10f;
    PlayerStamina stamina;

    Rigidbody rb;
    Animator animator;
    CapsuleCollider col;

    float h, v;
    bool isGrounded;
    bool isSprint;
    bool isCrouch;
    bool jumpRequest;

    float originalHeight;
    Vector3 originalCenter;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        col = GetComponent<CapsuleCollider>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        stamina = GetComponent<PlayerStamina>();
        animator.applyRootMotion = false;
        originalCamHeight = cameraHolder.localPosition.y;
        originalHeight = col.height;
        originalCenter = col.center;
    }

    void Update()
    {
        if (!canMove) return;

        // ===== INPUT =====
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        float speedValue = new Vector2(h, v).magnitude;
        animator.SetFloat("Speed", speedValue);

        // ===== SPRINT =====
        isSprint = Input.GetKey(KeyCode.LeftShift)
           && speedValue > 0.1f
           && !isCrouch
           && stamina.CanRun();
        if (isSprint)
        {
            stamina.UseStamina(20f * Time.deltaTime);
        }
        // ===== CROUCH TOGGLE =====
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ToggleCrouch();
        }

        // ===== GROUND CHECK =====
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundRadius,
            groundLayer
        );

        animator.SetBool("isGrounded", isGrounded);
        float targetY = isCrouch
        ? originalCamHeight - crouchCamOffset
        : originalCamHeight;

        Vector3 camPos = cameraHolder.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetY, Time.deltaTime * camLerpSpeed);
        cameraHolder.localPosition = camPos;


        // ===== JUMP REQUEST =====
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isCrouch)
        {
            jumpRequest = true;
            animator.SetTrigger("Jump");
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        float speed = isSprint ? runSpeed : walkSpeed;

        Vector3 move =
            transform.forward * v +
            transform.right * h;

        // Move only XZ – không đụng Y
        rb.MovePosition(
            rb.position + move * speed * Time.fixedDeltaTime
        );

        // ===== APPLY JUMP =====
        if (jumpRequest)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequest = false;
        }

    }

    void ToggleCrouch()
    {
        isCrouch = !isCrouch;

        if (animator)
            animator.SetBool("isCrouch", isCrouch);

        if (isCrouch)
        {
            col.height = crouchHeight;
            col.center = new Vector3(
                originalCenter.x,
                originalCenter.y - (originalHeight - crouchHeight) / 2f,
                originalCenter.z
            );
        }
        else
        {
            col.height = originalHeight;
            col.center = originalCenter;
        }
    }

}
