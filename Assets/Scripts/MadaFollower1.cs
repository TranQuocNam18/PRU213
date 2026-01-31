using UnityEngine;

public class MadaFollower1 : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public MicListener playerMic;

    [Header("Movement")]
    public float followDistance = 4f;
    public float moveSpeed = 1.5f;

    [Header("Player Look Check")]
    public float stopDot = 0.6f;
    public float disappearTime = 2.5f;

    [Header("Vision")]
    public float viewDistance = 10f;
    public LayerMask obstacleMask;
    public float eyeHeight = 1.5f;

    [Header("Hearing")]
    public float hearThreshold = 0.02f;

    Animator animator;
    Rigidbody rb;
    Renderer[] renderers;
    Collider[] colliders;

    bool isFrozen = false;
    bool isVisible = false;
    float lookTimer = 0f;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        HideMada(); // bắt đầu = KHÔNG hiện
    }

    void FixedUpdate()
    {
        if (!player) return;

        bool playerLooking = IsPlayerLookingAtMada();

        // 👁️ PLAYER ĐANG NHÌN
        if (playerLooking)
        {
            lookTimer += Time.fixedDeltaTime;
            Freeze();

            if (lookTimer >= disappearTime)
            {
                HideMada();
            }
            return;
        }

        // 👁️ PLAYER KHÔNG NHÌN
        lookTimer = 0f;

        if (!isVisible)
        {
            ShowMadaBehindPlayer();
            return;
        }

        UnFreeze();

        bool canSee = CanSeePlayer();
        bool canHear = CanHearPlayer();

        if (!canSee && !canHear)
        {
            StopMoving();
            return;
        }

        FollowPlayer();
    }

    // ================= LOOK =================

    bool IsPlayerLookingAtMada()
    {
        if (!isVisible) return false;

        Vector3 dirToMada = (transform.position - player.position).normalized;
        float dot = Vector3.Dot(player.forward, dirToMada);
        return dot > stopDot;
    }

    // ================= SENSE =================

    bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 dir = (player.position - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, viewDistance, ~obstacleMask))
        {
            return hit.transform == player;
        }
        return false;
    }

    bool CanHearPlayer()
    {
        return playerMic && playerMic.loudness > hearThreshold;
    }

    // ================= MOVE =================

    void FollowPlayer()
    {
        Vector3 targetPos = player.position - player.forward * followDistance;
        targetPos.y = transform.position.y;

        Vector3 moveDir = targetPos - transform.position;

        if (moveDir.magnitude > 0.2f)
        {
            Vector3 move = moveDir.normalized * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);

            Quaternion lookRot = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, lookRot, 5f * Time.fixedDeltaTime));

            animator.SetBool("isMoving", true);
        }
        else
        {
            StopMoving();
        }
    }

    void StopMoving()
    {
        animator.SetBool("isMoving", false);
    }

    // ================= FREEZE =================

    void Freeze()
    {
        if (isFrozen) return;
        isFrozen = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        animator.speed = 0f;
    }

    void UnFreeze()
    {
        if (!isFrozen) return;
        isFrozen = false;

        animator.speed = 1f;
    }

    // ================= APPEAR / DISAPPEAR =================

    void HideMada()
    {
        isVisible = false;

        foreach (var r in renderers)
            r.enabled = false;

        foreach (var c in colliders)
            c.enabled = false;
    }

    void ShowMadaBehindPlayer()
    {
        isVisible = true;

        Vector3 spawnPos = player.position - player.forward * followDistance;
        spawnPos.y = transform.position.y;

        transform.position = spawnPos;
        transform.LookAt(player);

        foreach (var r in renderers)
            r.enabled = true;

        foreach (var c in colliders)
            c.enabled = true;

        lookTimer = 0f;
    }
}
