using UnityEngine;

public class SkyManager : MonoBehaviour
{
    public static SkyManager Instance;

    public Material daySkybox;
    public Material nightSkybox;

    [Header("Audio")]
    public AudioClip ambientNightSound;
    private AudioSource audioSrc;

    void Awake()
    {
        Instance = this;
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.loop = true;
        audioSrc.volume = 0.3f; // subtle horror sound
    }

    public void ChangeToDark()
    {
        RenderSettings.skybox = nightSkybox;

        // Giảm ánh sáng môi trường
        RenderSettings.ambientIntensity = 0.2f;

        // Tối hơn
        Light dirLight = FindObjectOfType<Light>();
        if (dirLight != null)
        {
            dirLight.intensity = 0.1f;
        }

        // Bật Fog
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.05f); // đen xám
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.035f; // khá dày

        if (ambientNightSound != null && audioSrc != null)
        {
            audioSrc.clip = ambientNightSound;
            audioSrc.Play();
        }

        Debug.Log("Trời đã chuyển tối, có sương mù và âm thanh môi trường...");
    }

    public void ChangeToBright()
    {
        RenderSettings.skybox = daySkybox;

        // Phục hồi ánh sáng môi trường
        RenderSettings.ambientIntensity = 1f;

        // Phục hồi độ sáng mặt trời
        Light dirLight = FindObjectOfType<Light>();
        if (dirLight != null)
        {
            dirLight.intensity = 1f;
        }

        // Tắt Fog hoặc giảm nhẹ
        RenderSettings.fog = false;

        if (audioSrc != null)
        {
            audioSrc.Stop();
        }

        Debug.Log("Trời đã sáng trở lại...");
    }
}