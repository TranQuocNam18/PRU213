using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MouseLookRB : MonoBehaviour
{
    public Transform pitchPivot;
    public float sensX = 200f;
    public float sensY = 200f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    Rigidbody rb;
    float yaw;
    float pitch;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate; // giảm jitter [web:866]

        if (pitchPivot == null && Camera.main != null)
            pitchPivot = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = rb.rotation.eulerAngles.y;
        pitch = 0f;
        if (pitchPivot != null) pitchPivot.localRotation = Quaternion.identity;
    }

    void Update()
    {
        float mx = Input.GetAxisRaw("Mouse X") * sensX * Time.deltaTime;
        float my = Input.GetAxisRaw("Mouse Y") * sensY * Time.deltaTime;
        yaw += mx;
        pitch = Mathf.Clamp(pitch - my, minPitch, maxPitch);
    }

    void FixedUpdate()
    {
        rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));
    }

    //void FixedUpdate()
    //{
    //    // chỉ quay thân theo physics
    //    rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));
    //}

    void LateUpdate()
    {
        // quay camera mỗi frame để mượt
        if (pitchPivot != null)
            pitchPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
