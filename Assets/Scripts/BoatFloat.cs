using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatFloat : MonoBehaviour
{
    [Header("Water Settings")]
    public float waterLevel = 30f;     // Y mặt nước

    [Header("Float Settings")]
    public float floatStrength = 3f;   // lực nổi
    public float floatOffset = -0.5f;  // chỉnh độ chìm
    public float bounceDamp = 0.3f;    // giảm rung

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 👉 Đảm bảo thuyền spawn đúng mặt nước
        Vector3 pos = transform.position;
        pos.y = waterLevel + floatOffset;
        transform.position = pos;
    }

    void FixedUpdate()
    {
        // 👉 tính độ chênh lệch với mặt nước
        float difference = (waterLevel + floatOffset) - transform.position.y;

        // 👉 clamp lực để tránh bay/chìm quá mức
        float force = Mathf.Clamp(difference * floatStrength, -10f, 10f);

        // 👉 áp dụng lực nổi
        rb.AddForce(Vector3.up * force, ForceMode.Acceleration);

        // 👉 giảm rung theo trục Y
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            rb.linearVelocity.y * (1 - bounceDamp * Time.fixedDeltaTime),
            rb.linearVelocity.z
        );
    }
}