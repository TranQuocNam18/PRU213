using UnityEngine;
using System.Collections;

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

    [Header("Audio")]
    public AudioSource walkingAudio;
    public AudioSource runningAudio;
    public AudioSource breathingAudio;
    public AudioSource breathFastAudio;

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

    string currentAudioState = "";

    int runLoopCount = 0;
    float lastRunTime = 0f;
    bool breathFastPlaying = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        col = GetComponent<CapsuleCollider>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        animator.applyRootMotion = false;

        originalCamHeight = cameraHolder.localPosition.y;
        originalHeight = col.height;
        originalCenter = col.center;

        PlayStateSound("idle");
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
        isSprint = Input.GetKey(KeyCode.LeftShift) && speedValue > 0.1f && !isCrouch;
        animator.SetBool("isSprint", isSprint);

        // ===== CROUCH =====
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

        // ===== AUDIO STATE =====
        if (speedValue < 0.1f)
        {
            PlayStateSound("idle");
        }
        else if (isSprint)
        {
            PlayStateSound("run");
        }
        else
        {
            PlayStateSound("walk");
        }

        // ===== COUNT RUN LOOPS =====
        if (currentAudioState == "run" && runningAudio.isPlaying)
        {
            if (runningAudio.time < lastRunTime)
            {
                runLoopCount++;
            }

            lastRunTime = runningAudio.time;
        }

        // ===== JUMP =====
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

        rb.MovePosition(
            rb.position + move * speed * Time.fixedDeltaTime
        );

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

    void PlayStateSound(string state)
    {
        if (currentAudioState == state) return;

        currentAudioState = state;

        walkingAudio.Stop();
        runningAudio.Stop();

        if (state == "walk")
        {
            breathingAudio.Stop();
            breathFastAudio.Stop();

            walkingAudio.loop = true;
            walkingAudio.Play();
        }

        if (state == "run")
        {
            breathingAudio.Stop();
            breathFastAudio.Stop();

            runningAudio.loop = true;
            runningAudio.Play();
        }

        if (state == "idle")
        {
            walkingAudio.Stop();
            runningAudio.Stop();

            if (runLoopCount >= 3 && !breathFastPlaying)
            {
                StartCoroutine(PlayBreathFast());
            }
            else
            {
                if (!breathingAudio.isPlaying)
                {
                    breathingAudio.loop = true;
                    breathingAudio.Play();
                }
            }

            runLoopCount = 0;
        }
    }

    IEnumerator PlayBreathFast()
    {
        breathFastPlaying = true;

        breathingAudio.Stop();

        breathFastAudio.loop = false;
        breathFastAudio.Play();

        yield return new WaitForSeconds(breathFastAudio.clip.length);

        breathFastPlaying = false;

        if (currentAudioState == "idle")
        {
            breathingAudio.loop = true;
            breathingAudio.Play();
        }
    }
}