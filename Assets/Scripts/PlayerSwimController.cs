using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerSwimController : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;
    public Transform cameraRig;              // không xoay ở script này
    public Transform swimPitchPivot;         // pivot con để nghiêng xuống (chống giật)

    [Header("Params")]
    public string speedParam = "Speed";
    public string inWaterBool = "InWater";
    public string treadingBool = "Treading";
    public string swimToEdgeTrigger = "SwimToEdge";
    public string climbOutTrigger = "ClimbOut";

    [Header("Swim")]
    public float swimSpeed = 3.0f;
    public float verticalSwimSpeed = 2.0f;
    public float waterDrag = 4f;

    [Header("Surface")]
    public float surfaceOffset = 0.2f;
    public float surfaceStick = 2.0f;

    [Header("Look down pulse (Shift one-shot)")]
    public KeyCode lookDownKey = KeyCode.LeftShift;
    public float lookDownAngle = 35f;
    public float lookDownHold = 0.25f;
    public float lookDownSpeed = 12f;
    float lookDownT;

    [Header("Depth buoyancy")]
    public float maxDepth = 8f;
    public float buoyNearSurface = 2.0f;
    public float buoyDeep = -0.4f;
    public float buoyancyScale = 1.0f;

    [Header("Animator smoothing")]
    public float speedDampTime = 0.15f;   // chống nháy Swim/Treading khi speed rung

    [Header("Edge")]
    public float edgeCheckDistance = 1.0f;
    public LayerMask edgeMask = ~0;

    [Header("Turn to move")]
    public float rotateToMoveSpeed = 8f;

    [Header("Walk controller (kept for mouse look)")]
    public SimpleFirstPersonController_FriendStyle walkController;

    [Header("CharacterController in water")]
    public float swimStepOffset = 0f;
    public float swimSlopeLimit = 5f;

    CharacterController cc;
    bool inWater;
    Transform waterTransform;
    float yVel;
    bool canClimb;
    bool edgeArmed;

    float defaultStepOffset;
    float defaultSlopeLimit;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        defaultStepOffset = cc.stepOffset;
        defaultSlopeLimit = cc.slopeLimit;
    }

    // IMPORTANT: guard để nếu trigger nước gọi liên tục thì không reset state liên tục (gây giật)
    public void SetInWater(bool value, Transform water)
    {
        bool sameState = (inWater == value);
        bool sameWater = (waterTransform == (value ? water : null));
        if (sameState && sameWater) return;

        inWater = value;
        waterTransform = value ? water : null;

        // reset các biến chỉ khi chuyển state thật
        yVel = 0f;
        canClimb = false;
        edgeArmed = false;
        lookDownT = 0f;

        if (walkController != null)
        {
            walkController.enableMovement = !inWater;
            walkController.enableMouseLook = true;
        }

        if (inWater)
        {
            cc.stepOffset = swimStepOffset;
            cc.slopeLimit = swimSlopeLimit;
        }
        else
        {
            cc.stepOffset = defaultStepOffset;
            cc.slopeLimit = defaultSlopeLimit;
        }

        if (swimPitchPivot != null)
            swimPitchPivot.localRotation = Quaternion.identity;

        if (animator != null)
        {
            animator.SetBool(inWaterBool, inWater);
            animator.SetBool(treadingBool, false);
            animator.SetFloat(speedParam, 0f);
        }
    }

    void Update()
    {
        if (!inWater || waterTransform == null) return;

        float surfaceY = waterTransform.position.y;

        // 1) Shift: cúi xuống 1 nhịp
        if (Input.GetKeyDown(lookDownKey))
            lookDownT = lookDownHold;

        bool lookingDown = lookDownT > 0f;

        if (swimPitchPivot != null)
        {
            float targetPitch = lookingDown ? lookDownAngle : 0f;
            Quaternion targetRot = Quaternion.Euler(targetPitch, 0f, 0f);

            swimPitchPivot.localRotation = Quaternion.Slerp(
                swimPitchPivot.localRotation,
                targetRot,
                Time.deltaTime * lookDownSpeed
            );
        }

        lookDownT = Mathf.Max(0f, lookDownT - Time.deltaTime);

        // 2) Input
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(x, 0, z);
        if (input.sqrMagnitude > 1f) input.Normalize();

        // 3) Hướng bơi
        Vector3 right = transform.right;
        Vector3 forward = transform.forward;
        if (lookingDown && swimPitchPivot != null)
            forward = swimPitchPivot.forward;

        Vector3 move = (right * input.x + forward * input.z) * swimSpeed;

        // 4) Buoyancy theo độ sâu
        float depth = Mathf.Max(0f, surfaceY - transform.position.y);
        float t = Mathf.InverseLerp(0f, maxDepth, depth);
        float buoy = Mathf.Lerp(buoyNearSurface, buoyDeep, t) * buoyancyScale;

        // 5) Vertical
        float targetY = surfaceY - surfaceOffset;
        float targetYVel;

        if (lookingDown && input.z > 0.1f)
        {
            float diveVel = Mathf.Clamp(move.y, -verticalSwimSpeed, verticalSwimSpeed);
            targetYVel = diveVel + buoy;
        }
        else
        {
            float error = (targetY - transform.position.y);
            float stickVel = Mathf.Clamp(error * surfaceStick, -0.5f, 0.8f);
            if (Input.GetKey(KeyCode.Space)) stickVel = Mathf.Max(stickVel, 1.0f);
            targetYVel = stickVel + buoy;
        }

        yVel = Mathf.Lerp(yVel, targetYVel, Time.deltaTime * waterDrag);

        // 6) Move
        Vector3 final = new Vector3(move.x, yVel, move.z);
        cc.Move(final * Time.deltaTime);

        // 7) Quay người theo hướng bơi ngang
        Vector3 planar = new Vector3(move.x, 0f, move.z);
        if (planar.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(planar.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotateToMoveSpeed);
        }

        // 8) Animator
        // FIX: speed lấy theo INPUT để không bị kẹt Treading khi move.xz bị nhỏ/0 dưới nước
        float speed01 = Mathf.Clamp01(new Vector2(x, z).magnitude);
        bool treading = (speed01 < 0.05f) && !lookingDown;

        if (animator != null)
        {
            animator.SetFloat(speedParam, speed01, speedDampTime, Time.deltaTime); // overload chính thức [web:1880]
            animator.SetBool(treadingBool, treading);
        }

        // 9) Edge check (chỉ cho leo bờ khi gần mặt nước)
        bool nearSurfaceForEdge = transform.position.y > surfaceY - surfaceOffset - 0.15f;

        canClimb = !lookingDown && nearSurfaceForEdge && Physics.Raycast(
            transform.position + Vector3.up * 0.5f,
            transform.forward,
            edgeCheckDistance,
            edgeMask,
            QueryTriggerInteraction.Ignore
        );

        if (Input.GetKeyDown(KeyCode.E) && canClimb && animator != null)
        {
            if (!edgeArmed)
            {
                animator.SetTrigger(swimToEdgeTrigger);
                edgeArmed = true;
            }
            else
            {
                animator.SetTrigger(climbOutTrigger);
            }
        }
    }

    public void FinishClimbOut()
    {
        SetInWater(false, null);
    }
}
