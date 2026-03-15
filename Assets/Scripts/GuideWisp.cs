using UnityEngine;

public class GuideWisp : MonoBehaviour
{
    private Transform target;
    public float speed = 5f;
    public float lifetime = 8f;
    private float age = 0f;

    void Start()
    {
        // Setup Visuals
        CreateVisuals();
    }

    void CreateVisuals()
    {
        // Hình cầu phát sáng
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(this.transform, false);
        sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        
        Destroy(sphere.GetComponent<SphereCollider>()); // Không cần va chạm

        Renderer rend = sphere.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Sprites/Default")); // Vật liệu không ánh sáng
        rend.material.color = new Color(0.5f, 1f, 1f, 0.8f); // Màu xanh lơ phát sáng

        // Ánh sáng xung quanh
        Light pointLight = gameObject.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = new Color(0.5f, 1f, 1f);
        pointLight.range = 3f;
        pointLight.intensity = 2f;

        // Vệt sáng (Trail)
        TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.5f;
        trail.startWidth = 0.15f;
        trail.endWidth = 0f;
        trail.material = rend.material;

        // Di chuyển lượn lờ (sử dụng noise)
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Update()
    {
        age += Time.deltaTime;
        
        // Mờ dần rồi biến mất
        if (age > lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
        {
            // Trôi lờ lững lên trên rồi biến mất
            transform.position += Vector3.up * speed * 0.5f * Time.deltaTime;
            return;
        }

        // Di chuyển về phía mục tiêu kèm hiệu ứng lượn sóng
        Vector3 dir = (target.position - transform.position).normalized;
        
        // Thêm chuyển động lượn
        float waveY = Mathf.Sin(age * 5f) * 0.5f;
        float waveX = Mathf.Cos(age * 3f) * 0.5f;
        
        Vector3 waveOffset = transform.right * waveX + transform.up * waveY;
        
        Vector3 moveDir = (dir + waveOffset * 0.5f).normalized;

        transform.position += moveDir * speed * Time.deltaTime;

        // Nếu quá gần mục tiêu thì biến mất
        if (Vector3.Distance(transform.position, target.position) < 1.5f)
        {
            Destroy(gameObject);
        }
    }
}
