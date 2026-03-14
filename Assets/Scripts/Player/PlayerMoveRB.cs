using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMoveRB : MonoBehaviour
{
    [Header("Move")]
    public float walkSpeed = 2.5f;
    public float sprintSpeed = 4.5f;

    [Header("Air control")]
    [Range(0f, 1f)] public float airControl = 0.3f;

    [Header("Jump")]
    public float jumpImpulse = 5f;

    [Header("Ground check")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask = ~0;

    [Header("Slopes")]
    public float maxSlopeAngle = 55f;
    public float stickToGroundForce = 20f;

    [Header("Animator")]
    public Animator anim;                 // kéo Animator của Remy vào đây
    public string walkBool = "isWalking"; // đúng tên parameter

    Rigidbody rb;
    CapsuleCollider col;
    bool jumpQueued;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.applyRootMotion = false;

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            jumpQueued = true;
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float speed = sprint ? sprintSpeed : walkSpeed;

        Vector3 forward = transform.forward; forward.y = 0f;
        Vector3 right = transform.right; right.y = 0f;

        Vector3 dir = (right.normalized * h + forward.normalized * v);
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        bool grounded = TryGetGround(out RaycastHit groundHit);

        if (grounded)
        {
            float slopeAngle = Vector3.Angle(groundHit.normal, Vector3.up);
            if (slopeAngle > maxSlopeAngle)
            {
                float uphill = Vector3.Dot(dir, Vector3.ProjectOnPlane(groundHit.normal, Vector3.up).normalized);
                if (uphill > 0f) dir = Vector3.zero;
            }
        }

        Vector3 vel = rb.linearVelocity;
        Vector3 wish = dir * speed;

        if (grounded)
        {
            rb.linearVelocity = new Vector3(wish.x, vel.y, wish.z);
            if (rb.linearVelocity.y <= 0f)
                rb.AddForce(Vector3.down * stickToGroundForce, ForceMode.Acceleration);
        }
        else
        {
            float nx = Mathf.Lerp(vel.x, wish.x, airControl);
            float nz = Mathf.Lerp(vel.z, wish.z, airControl);
            rb.linearVelocity = new Vector3(nx, vel.y, nz);
        }

        // Update Animator (Idle/Walk)
        if (anim != null)
        {
            bool isWalking = dir.sqrMagnitude > 0.0001f; // hoặc dùng vel.magnitude
            anim.SetBool(walkBool, isWalking);
        }

        if (jumpQueued && grounded)
        {
            Vector3 v0 = rb.linearVelocity;
            v0.y = 0f;
            rb.linearVelocity = v0;
            rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
        }

        jumpQueued = false;
    }

    bool TryGetGround(out RaycastHit hit)
    {
        float radius = col.radius * 0.9f;

        Vector3 origin = col.bounds.center;
        origin.y = col.bounds.min.y + radius + 0.02f;

        return Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out hit,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }
}
