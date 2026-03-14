using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Ground Check")]
    public bool isGrounded = true; // gán t? script movement

    // ===== Animator Params =====
    const string SPEED = "Speed";
    const string CROUCH = "isCrouching";
    const string SPRINT = "isSprinting";
    const string JUMP = "Jump";

    float speed;

    void Awake()
    {
        if (!animator)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        UpdateMovementSpeed();
        UpdateCrouch();
        UpdateSprint();
        UpdateJump();
    }

    // ================= MOVEMENT =================

    void UpdateMovementSpeed()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        speed = new Vector3(h, 0, v).magnitude;
        speed = Mathf.Clamp01(speed);

        animator.SetFloat(SPEED, speed);
    }

    // ================= CROUCH =================

    void UpdateCrouch()
    {
        bool crouchInput = Input.GetKey(KeyCode.LeftControl);

        // ? Không cho crouch khi ?ang nh?y
        if (!isGrounded)
            crouchInput = false;

        animator.SetBool(CROUCH, crouchInput);

        // ? Không cho sprint khi crouch
        if (crouchInput)
            animator.SetBool(SPRINT, false);
    }

    // ================= SPRINT =================

    void UpdateSprint()
    {
        bool sprintInput = Input.GetKey(KeyCode.LeftShift);

        // ? Không sprint khi crouch
        if (animator.GetBool(CROUCH))
            sprintInput = false;

        animator.SetBool(SPRINT, sprintInput);
    }

    // ================= JUMP =================

    void UpdateJump()
    {
        if (!isGrounded) return;

        // ? Không jump khi crouch
        if (animator.GetBool(CROUCH)) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger(JUMP);
        }
    }

    // ================= PUBLIC API =================
    // Dùng cho script movement g?i vào

    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
    }
}
