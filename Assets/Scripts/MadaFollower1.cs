using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class MadaFollowerAI : MonoBehaviour
{
    // ================= TARGET =================
    [Header("Target")]
    public Transform player;
    public Rigidbody playerRb;
    public MicListener playerMic;

    // ================= MOVEMENT =================
    [Header("Movement")]
    public float moveSpeed = 2.2f;
    public float rotateSpeed = 6f;
    public float followDistance = 3f;

    // ================= ATTACK =================
    [Header("Attack")]
    public float attackDistance = 1.2f;
    public float jumpscareDuration = 0.8f;
    public GameObject gameOverUI;

    // ================= VISION =================
    [Header("Vision")]
    public float viewDistance = 12f;
    public float eyeHeight = 1.5f;

    // ================= HEARING =================
    [Header("Hearing")]
    public float hearThreshold = 0.02f;

    // ================= PLAYER LOOK =================
    [Header("Player Look")]
    [Range(0f, 1f)]
    public float stopDot = 0.6f;
    public float disappearTime = 2.5f;
    public float lookConfirmTime = 0.15f;

    // ================= SEARCH / ROAM =================
    [Header("Search / Roam")]
    public float searchRadius = 8f;
    public float roamRadius = 12f;
    public float waitAtPointTime = 1.5f;

    // ================= TELEPORT =================
    [Header("Teleport / Ground")]
    public float groundRayHeight = 25f;
    public LayerMask groundMask;

    // ================= ANIM =================
    [Header("Animator")]
    public string movingBool = "isMoving";
    public string chasingBool = "isChasing";

    // ================= PRIVATE =================
    Rigidbody rb;
    Animator animator;
    Renderer[] renderers;
    Collider[] colliders;

    bool isVisible;
    bool isFrozen;
    bool isChasing;
    bool isAttacking;

    float lookTimer;
    float lookConfirmTimer;

    Vector3 lastKnownPlayerPos;
    bool hasLastKnownPos;
    Vector3 roamTarget;
    float waitTimer;

    // ================= START =================
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        HideMada();
    }

    // ================= FIXED UPDATE =================
    void FixedUpdate()
    {
        if (!player || isAttacking) return;

        // ===== PLAYER NHÌN THẤY → FREEZE =====
        if (IsPlayerLookingAtMada())
        {
            lookConfirmTimer += Time.fixedDeltaTime;

            if (lookConfirmTimer >= lookConfirmTime)
            {
                lookTimer += Time.fixedDeltaTime;
                Freeze();
                SetAnim(false, false);

                if (lookTimer >= disappearTime)
                    HideMada();
            }
            return;
        }

        // reset
        lookConfirmTimer = 0f;
        lookTimer = 0f;
        UnFreeze();

        // ===== CHƯA HIỆN → TELEPORT =====
        if (!isVisible)
        {
            TeleportBehindPlayerSafe();
            return;
        }

        // ===== SENSE =====
        bool canSee = CanSeePlayer();
        bool canHear = CanHearPlayer();
        isChasing = canSee || canHear;

        if (isChasing)
        {
            lastKnownPlayerPos = player.position;
            hasLastKnownPos = true;
            ChasePlayer();
            return;
        }

        // ===== KHÔNG THẤY → SEARCH / ROAM =====
        SearchOrRoam();
    }

    // ================= CHASE =================
    void ChasePlayer()
    {
        Vector3 target = player.position;
        target.y = transform.position.y;

        Vector3 dir = target - transform.position;
        float dist = dir.magnitude;

        if (dist <= attackDistance)
        {
            StartCoroutine(JumpscareSequence());
            return;
        }

        MoveTo(target, true);
    }

    // ================= SEARCH / ROAM =================
    void SearchOrRoam()
    {
        // SEARCH: đi tới vị trí cuối cùng của player
        if (hasLastKnownPos)
        {
            MoveTo(lastKnownPlayerPos, false);

            if (Vector3.Distance(transform.position, lastKnownPlayerPos) < 0.8f)
            {
                waitTimer += Time.fixedDeltaTime;
                SetAnim(false, false);

                if (waitTimer >= waitAtPointTime)
                {
                    hasLastKnownPos = false;
                    waitTimer = 0f;
                }
            }
            return;
        }

        // ROAM: lang thang xung quanh player
        if (roamTarget == Vector3.zero || Vector3.Distance(transform.position, roamTarget) < 0.5f)
        {
            roamTarget = GetRandomRoamPoint();
        }

        MoveTo(roamTarget, false);
    }

    // ================= MOVE =================
    void MoveTo(Vector3 target, bool chasing)
    {
        if (isFrozen) return;

        target.y = transform.position.y;
        Vector3 dir = target - transform.position;

        Vector3 move = dir.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, rot, rotateSpeed * Time.fixedDeltaTime));
        }

        SetAnim(true, chasing);
    }

    // ================= SENSE =================
    bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 dir = (player.position - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, viewDistance))
        {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }
        return false;
    }

    bool CanHearPlayer()
    {
        return playerMic && playerMic.loudness > hearThreshold;
    }

    // ================= PLAYER LOOK CHECK =================
    bool IsPlayerLookingAtMada()
    {
        if (!isVisible) return false;

        Camera cam = Camera.main;
        if (!cam) return false;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        if (vp.z <= 0f || vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f)
            return false;

        Vector3 dir = (transform.position - cam.transform.position).normalized;
        if (Vector3.Dot(cam.transform.forward, dir) < stopDot)
            return false;

        if (Physics.Raycast(cam.transform.position, dir, out RaycastHit hit, viewDistance))
        {
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        }

        return false;
    }

    // ================= TELEPORT =================
    void TeleportBehindPlayerSafe()
    {
        Camera cam = Camera.main;
        if (!cam) return;

        Vector3 backDir = -player.forward;
        backDir.y = 0f;
        backDir.Normalize();

        Vector3 desiredPos = player.position + backDir * followDistance;

        Vector3 vp = cam.WorldToViewportPoint(desiredPos);
        if (vp.z > 0 && vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1)
            return;

        if (Physics.Raycast(desiredPos + Vector3.up * groundRayHeight, Vector3.down,
            out RaycastHit hit, groundRayHeight * 2f, groundMask))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.position = hit.point + Vector3.up * 0.05f;
            rb.rotation = Quaternion.LookRotation(player.position - rb.position);

            ShowMada();
        }
    }

    // ================= JUMPSCARE =================
    IEnumerator JumpscareSequence()
    {
        isAttacking = true;

        rb.linearVelocity = Vector3.zero;
        SetAnim(false, false);

        if (playerRb)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.isKinematic = true;
        }

        Camera cam = Camera.main;
        if (cam)
        {
            transform.position = cam.transform.position + cam.transform.forward * 0.5f;
            transform.LookAt(cam.transform);
        }

        yield return new WaitForSeconds(jumpscareDuration);

        if (gameOverUI)
            gameOverUI.SetActive(true);

        Time.timeScale = 0f;
    }

    // ================= ANIM =================
    void SetAnim(bool moving, bool chasing)
    {
        if (!animator) return;
        animator.SetBool(movingBool, moving);
        animator.SetBool(chasingBool, chasing);
    }

    // ================= FREEZE =================
    void Freeze()
    {
        if (isFrozen) return;
        isFrozen = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        if (animator) animator.speed = 0f;
    }

    void UnFreeze()
    {
        if (!isFrozen) return;
        isFrozen = false;

        rb.isKinematic = false;
        if (animator) animator.speed = 1f;
    }

    // ================= VISIBILITY =================
    void HideMada()
    {
        isVisible = false;
        hasLastKnownPos = false;
        roamTarget = Vector3.zero;

        foreach (var r in renderers) r.enabled = false;
        foreach (var c in colliders) c.enabled = false;

        rb.isKinematic = true;
    }

    void ShowMada()
    {
        isVisible = true;

        foreach (var r in renderers) r.enabled = true;
        foreach (var c in colliders) c.enabled = true;

        rb.isKinematic = false;
    }
    Vector3 GetRandomRoamPoint()
    {
        Camera cam = Camera.main;

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * roamRadius;
            randomDir.y = 0f;

            Vector3 point = player.position + randomDir;

            // Tránh xuất hiện trong tầm nhìn camera
            if (cam)
            {
                Vector3 vp = cam.WorldToViewportPoint(point);
                if (vp.z > 0 && vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1)
                    continue;
            }

            // Raycast xuống đất để tìm vị trí an toàn
            if (Physics.Raycast(point + Vector3.up * groundRayHeight,
                Vector3.down,
                out RaycastHit hit,
                groundRayHeight * 2f,
                groundMask))
            {
                return hit.point + Vector3.up * 0.05f;
            }
        }

        // Fallback nếu không tìm được điểm hợp lệ
        return transform.position;
    }
}
