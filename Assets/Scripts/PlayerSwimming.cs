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

        // ===== LỰC ĐẨY (BUOYANCY) =====
        // Nếu người chơi ở dưới mặt nước, áp dụng lực đẩy hướng lên để nổi
        if (y < waterSurfaceY)
        {
            rb.AddForce(Vector3.up * buoyancyForce, ForceMode.Acceleration);
        }

        // ===== BƠI LÊN (SWIM UP) =====
        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * swimUpSpeed, ForceMode.Acceleration);
        }

        // ===== LẶN XUỐNG (DIVE) =====
        if (Input.GetKey(KeyCode.LeftControl))
        {
            rb.AddForce(Vector3.down * swimUpSpeed, ForceMode.Acceleration);
        }

        // ===== TỐC ĐỘ BƠI VỀ PHÍA TRƯỚC =====
        float speed = swimForwardSpeed;

        // Xử lý bơi nhanh (Sprinting)
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= sprintMultiplier;

            if (animator != null)
                animator.speed = 3f; // Tăng tốc độ animation bơi
        }
        else
        {
            if (animator != null)
                animator.speed = 1.5f; // Tốc độ animation bơi bình thường
        }

        // Di chuyển người chơi theo hướng nhìn
        Vector3 move = transform.forward * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    // Kích hoạt trạng thái bơi khi vào vùng nước
    public void EnterWater()
    {
        isSwimming = true;

        if (animator != null)
            animator.SetBool("isSwimming", true);

        // Tắt trọng lực để sử dụng lực đẩy của nước
        rb.useGravity = false;

        // Phát âm thanh bơi
        if (swimAudio != null && !swimAudio.isPlaying)
        {
            swimAudio.loop = true;
            swimAudio.Play();
        }
    }

    // Tắt trạng thái bơi khi ra khỏi vùng nước
    public void ExitWater()
    {
        isSwimming = false;

        if (animator != null)
        {
            animator.SetBool("isSwimming", false);
            animator.speed = 1f;
        }

        // Bật lại trọng lực khi lên cạn
        rb.useGravity = true;

        if (swimAudio != null)
            swimAudio.Stop();
    }
}