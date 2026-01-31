using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFirstPersonController_FriendStyle : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;

    [Header("Jump/Gravity")]
    public float jumpHeight = 1.6f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 120f;
    private float xRotation;

    [Header("Camera")]
    public Transform cameraRig;
    public Transform cameraTarget;
    public float cameraFollowSpeed = 10f;

    [Header("Animator")]
    public Animator animator;
    public string speedParam = "Speed";
    public string groundedBool = "IsGrounded";
    public string jumpTrigger = "Jump";
    public string crouchBool = "IsCrouching";
    public string sprintBool = "IsSprinting";

    [Header("Crouch")]
    public KeyCode crouchKey = KeyCode.C;
    public float standHeight = 1.8f;
    public float crouchHeight = 1.1f;
    public float crouchSpeedMultiplier = 0.55f;
    public float crouchTransitionSpeed = 12f;
    public float cameraStandLocalY = 0.8f;
    public float cameraCrouchLocalY = 0.45f;
    public LayerMask ceilingMask = ~0;

    private bool isCrouching;
    private float targetHeight;
    private float targetCamLocalY;

    [Header("Enable/Disable")]
    public bool enableMovement = true;   // tắt khi bơi
    public bool enableMouseLook = true;  // vẫn bật khi bơi

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraRig == null && Camera.main != null)
            cameraRig = Camera.main.transform.parent != null ? Camera.main.transform.parent : Camera.main.transform;

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.applyRootMotion = false;

        targetHeight = standHeight;
        targetCamLocalY = cameraStandLocalY;

        controller.height = standHeight;
        Vector3 c = controller.center;
        c.y = controller.height * 0.5f;
        controller.center = c;

        if (cameraRig != null)
        {
            Vector3 lp = cameraRig.localPosition;
            lp.y = cameraStandLocalY;
            cameraRig.localPosition = lp;
        }
    }

    void Update()
    {
        // ----------------------
        // MOVEMENT (WASD/jump/gravity/crouch)
        // ----------------------
        if (enableMovement)
        {
            isGrounded = controller.isGrounded;
            if (isGrounded && velocity.y < 0f)
                velocity.y = -2f;

            // Toggle crouch
            if (Input.GetKeyDown(crouchKey))
            {
                if (!isCrouching)
                {
                    isCrouching = true;
                }
                else
                {
                    float radius = controller.radius * 0.95f;
                    float extra = (standHeight - controller.height) + 0.05f;

                    Vector3 origin = transform.position + Vector3.up * (controller.height * 0.5f);
                    bool blocked = Physics.SphereCast(
                        origin, radius, Vector3.up, out _,
                        extra, ceilingMask, QueryTriggerInteraction.Ignore
                    );

                    if (!blocked) isCrouching = false;
                }

                if (animator != null) animator.SetBool(crouchBool, isCrouching);
            }

            // Movement input
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = new Vector3(x, 0f, z).normalized;

            bool wantsSprint = Input.GetKey(KeyCode.LeftShift);

            if (isCrouching && wantsSprint)
            {
                isCrouching = false;
                if (animator != null) animator.SetBool(crouchBool, false);
            }

            if (animator != null) animator.SetBool(sprintBool, wantsSprint);

            float baseSpeed = wantsSprint ? sprintSpeed : moveSpeed;
            float targetSpeed = isCrouching ? baseSpeed * crouchSpeedMultiplier : baseSpeed;

            Vector3 move = transform.right * inputDir.x + transform.forward * inputDir.z;
            controller.Move(move * targetSpeed * Time.deltaTime); // CharacterController moves only when you call Move [web:1735]

            float currentSpeed = move.magnitude * targetSpeed;
            if (animator != null) animator.SetFloat(speedParam, currentSpeed);

            if (animator != null) animator.SetBool(groundedBool, isGrounded);

            if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (animator != null) animator.SetTrigger(jumpTrigger);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            // Crouch collider + camera
            targetHeight = isCrouching ? crouchHeight : standHeight;
            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

            Vector3 center = controller.center;
            center.y = controller.height * 0.5f;
            controller.center = center;

            if (cameraRig != null && cameraTarget == null)
            {
                targetCamLocalY = isCrouching ? cameraCrouchLocalY : cameraStandLocalY;
                Vector3 lp = cameraRig.localPosition;
                lp.y = Mathf.Lerp(lp.y, targetCamLocalY, Time.deltaTime * crouchTransitionSpeed);
                cameraRig.localPosition = lp;
            }
        }
        else
        {
            // Khi bơi: đảm bảo không còn "rơi" do gravity cũ trong script đi bộ
            velocity.y = 0f;

            // Tránh animator trên cạn bị set sprint/speed khi đang bơi (tuỳ bạn)
            if (animator != null) animator.SetBool(sprintBool, false);
            if (animator != null) animator.SetFloat(speedParam, 0f);
        }

        // ----------------------
        // MOUSE LOOK
        // ----------------------
        if (enableMouseLook)
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            if (cameraRig != null)
                cameraRig.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            transform.Rotate(Vector3.up * mouseX);
        }
    }

    void LateUpdate()
    {
        if (cameraRig != null && cameraTarget != null)
        {
            cameraRig.position = Vector3.Lerp(
                cameraRig.position,
                cameraTarget.position,
                Time.deltaTime * cameraFollowSpeed
            );
        }
    }
}
