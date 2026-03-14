using UnityEngine;
using System.Collections;

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

        if (ambientNightSound != null && audioSrc != null)
        {
            audioSrc.clip = ambientNightSound;
            audioSrc.Play();
        }

        StartCoroutine(FadeToDarkRoutine());

        Debug.Log("Trời đã chuyển tối, và âm thanh môi trường...");
    }

    private IEnumerator FadeToDarkRoutine()
    {
        float duration = 4f;
        float time = 0;
        float startAmbient = RenderSettings.ambientIntensity;
        Light dirLight = FindObjectOfType<Light>();
        float startLight = dirLight != null ? dirLight.intensity : 1f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            RenderSettings.ambientIntensity = Mathf.Lerp(startAmbient, 0.2f, t);
            if (dirLight != null) dirLight.intensity = Mathf.Lerp(startLight, 0.1f, t);
            yield return null;
        }
    }

    public void EnableFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.05f); // đen xám
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.035f; // khá dày
        Debug.Log("Đã bật sương mù...");
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