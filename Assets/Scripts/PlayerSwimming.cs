using UnityEngine;

public class PlayerSwimming : MonoBehaviour
{
    public Animator animator;
    public Rigidbody rb;
    public AudioSource swimAudio;

    [Header("Water Settings")]
    public float waterSurfaceY;
    public float buoyancyForce = 5f;
    public float swimUpSpeed = 4f;

    [Header("Swim Speed")]
    public float swimForwardSpeed = 3f;
    public float sprintMultiplier = 2f;

    bool isSwimming = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (swimAudio == null)
            swimAudio = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if (!isSwimming) return;

        float y = transform.position.y;

        // ===== BUOYANCY =====
        if (y < waterSurfaceY)
        {
            rb.AddForce(Vector3.up * buoyancyForce, ForceMode.Acceleration);
        }

        // ===== SWIM UP =====
        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * swimUpSpeed, ForceMode.Acceleration);
        }

        // ===== DIVE =====
        if (Input.GetKey(KeyCode.LeftControl))
        {
            rb.AddForce(Vector3.down * swimUpSpeed, ForceMode.Acceleration);
        }

        // ===== FORWARD SWIM =====
        float speed = swimForwardSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= sprintMultiplier;

            if (animator != null)
                animator.speed = 3f;
        }
        else
        {
            if (animator != null)
                animator.speed = 1.5f;
        }

        Vector3 move = transform.forward * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    public void EnterWater()
    {
        isSwimming = true;

        if (animator != null)
            animator.SetBool("isSwimming", true);

        rb.useGravity = false;

        if (swimAudio != null && !swimAudio.isPlaying)
        {
            swimAudio.loop = true;
            swimAudio.Play();
        }
    }

    public void ExitWater()
    {
        isSwimming = false;

        if (animator != null)
        {
            animator.SetBool("isSwimming", false);
            animator.speed = 1f;
        }

        rb.useGravity = true;

        if (swimAudio != null)
            swimAudio.Stop();
    }
}