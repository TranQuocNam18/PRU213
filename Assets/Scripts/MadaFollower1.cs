using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class MadaFollowerAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public Rigidbody playerRb;

    [Header("Water State")]
    private bool playerInWaterZone = false;

    [Header("Movement")]
    public float moveSpeed = 1.8f;
    public float rotateSpeed = 4f;
    public float followDistance = 2.5f;

    [Header("Attack")]
    public float attackDistance = 1.2f;
    public float appearDelay = 6f;
    public float disappearTime = 2f;
    public float lookConfirmTime = 0.15f;
    public GameObject gameOverUI;

    [Header("Animator")]
    public string movingBool = "isMoving";
    public string chasingBool = "isChasing";
    [Header("Animator States")]
    public string idleStateName = "Idle";

    [Header("Jumpscare")]
    public Camera playerCamera;
    public Camera jumpscareCamera;
    public AudioSource jumpscareSound;
    public float shakeAmount = 0.3f;
    public float attackDuration = 0.6f;


    Rigidbody rb;
    Animator animator;
    Renderer[] renderers;
    Collider[] colliders;

    bool isVisible = false;
    bool isAttacking = false;

    float appearTimer = 0f;
    float lookTimer = 0f;
    float lookConfirmTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        HideInstant();
    }

    void Update()
    {
        if (transform.position.y < -20f)
        {
            HideInstant();
            return;
        }
        if (!player || isAttacking) return;

        // Nếu player không ở vùng nước → ẩn luôn
        if (!playerInWaterZone)
        {
            HideInstant();
            return;
        }

        appearTimer += Time.deltaTime;

        // ===== Chưa xuất hiện =====
        if (!isVisible)
        {
            if (appearTimer >= appearDelay)
            {
                TeleportBehindPlayer();
                Show();
            }
            return;
        }

        // ===== Nếu player nhìn vào Ma Da =====
        if (IsPlayerLooking())
        {
            lookConfirmTimer += Time.deltaTime;

            if (lookConfirmTimer >= lookConfirmTime)
            {
                lookTimer += Time.deltaTime;
                SetAnim(false, false);

                if (lookTimer >= disappearTime)
                {
                    Hide();
                }
            }
            return;
        }

        // reset khi không nhìn
        lookTimer = 0f;
        lookConfirmTimer = 0f;

        MoveTowardsPlayer();
    }

    // ================= WATER SIGNAL =================
    public void SetPlayerInWater(bool value)
    {
        playerInWaterZone = value;

        if (!value)
            HideInstant();
    }

    // ================= MOVEMENT =================
    void MoveTowardsPlayer()
    {
        Vector3 target = player.position;
        target.y = transform.position.y;

        Vector3 dir = target - transform.position;
        float dist = dir.magnitude;

        if (dist <= attackDistance)
        {
            StartCoroutine(Jumpscare());
            return;
        }

        Vector3 move = dir.normalized * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + move);

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, rot, rotateSpeed * Time.deltaTime));
        }

        SetAnim(true, true);
    }

    // ================= LOOK CHECK =================
    bool IsPlayerLooking()
    {
        if (!isVisible) return false;

        Camera cam = Camera.main;
        if (!cam) return false;

        Vector3 dir = (transform.position - cam.transform.position).normalized;
        return Vector3.Dot(cam.transform.forward, dir) > 0.7f;
    }

    // ================= TELEPORT =================
    void TeleportBehindPlayer()
    {
        Vector3 backDir = -player.forward;
        backDir.y = 0;
        backDir.Normalize();

        Vector3 targetPos = player.position + backDir * followDistance;

        RaycastHit hit;

        // Raycast từ trên xuống để tìm mặt đất
        if (Physics.Raycast(targetPos + Vector3.up * 10f, Vector3.down, out hit, 50f))
        {
            rb.position = hit.point + Vector3.up * 0.1f;
        }
        else
        {
            // Nếu không tìm được đất → đặt ngang player
            rb.position = new Vector3(targetPos.x, player.position.y, targetPos.z);
        }

        Vector3 lookDir = player.position - rb.position;
        lookDir.y = 0;
        rb.rotation = Quaternion.LookRotation(lookDir);
    }

    // ================= JUMPSCARE =================
    IEnumerator Jumpscare()
    {
        isAttacking = true;
        if (jumpscareSound != null)
        {
            jumpscareSound.Play();
        }
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.isKinematic = true;

        // Reset rotation để đứng thẳng
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        if (animator)
        {
            animator.applyRootMotion = false;   // Quan trọng
            animator.Rebind();                  // Reset toàn bộ pose
            animator.Update(0f);

            animator.SetBool(movingBool, false);
            animator.SetBool(chasingBool, false);

            animator.Play(idleStateName, 0, 0f);
        }

        // Switch camera
        if (playerCamera && jumpscareCamera)
        {
            playerCamera.enabled = false;
            jumpscareCamera.enabled = true;
        }

        // Đặt Mada trước camera
        Vector3 camForward = jumpscareCamera.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        // Đặt camera ngay trước mặt Mada
        Vector3 madaForward = transform.forward;
        madaForward.y = 0;
        madaForward.Normalize();

        Vector3 camPos = transform.position + madaForward * 1.2f;
        camPos.y = transform.position.y + 1.6f; // ngang đầu

        jumpscareCamera.transform.position = camPos;
        jumpscareCamera.transform.LookAt(transform.position + Vector3.up * 1.4f);


        transform.LookAt(jumpscareCamera.transform.position);
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        float timer = 0f;
        Vector3 originalCamPos = jumpscareCamera.transform.position;

        while (timer < attackDuration)
        {
            jumpscareCamera.transform.position =
                originalCamPos + Random.insideUnitSphere * shakeAmount;

            timer += Time.deltaTime;
            yield return null;
        }
        
        if (gameOverUI)
            gameOverUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

    }



    // ================= ANIMATION =================
    void SetAnim(bool moving, bool chasing)
    {
        if (!animator) return;

        animator.SetBool(movingBool, moving);
        animator.SetBool(chasingBool, chasing);
    }

    // ================= VISIBILITY =================
    void Hide()
    {
        isVisible = false;
        appearTimer = 0f;

        SetAnim(false, false);

        foreach (var r in renderers) r.enabled = false;
        foreach (var c in colliders) c.enabled = false;

        rb.isKinematic = true;
    }

    void HideInstant()
    {
        isVisible = false;
        appearTimer = 0f;

        foreach (var r in renderers) r.enabled = false;
        foreach (var c in colliders) c.enabled = false;

        rb.isKinematic = true;
    }

    void Show()
    {
        isVisible = true;

        foreach (var r in renderers) r.enabled = true;
        foreach (var c in colliders) c.enabled = true;

        rb.isKinematic = false;

        SetAnim(true, false);
    }
}
