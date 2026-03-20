using UnityEngine;
using System.Collections;

public class SkyManager : MonoBehaviour
{
    public static SkyManager Instance;

    public Material daySkybox;
    public Material nightSkybox;

    [Header("Audio Settings")]
    public AudioClip ambientNightSound;
    private AudioSource audioSrc;

    [Tooltip("Kéo thả các Audio Source phát âm thanh ban ngày vào đây")]
    public AudioSource[] daytimeSounds; // Mảng chứa các âm thanh cần tắt khi trời tối
    private float[] daytimeStartVolumes; // Lưu lại âm lượng gốc để bật lại khi trời sáng

    void Awake()
    {
        Instance = this;
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.loop = true;
        audioSrc.volume = 0.3f; // subtle horror sound
    }

    void Start()
    {
        // Ghi nhớ mức âm lượng ban đầu của các âm thanh ban ngày
        daytimeStartVolumes = new float[daytimeSounds.Length];
        for (int i = 0; i < daytimeSounds.Length; i++)
        {
            if (daytimeSounds[i] != null)
            {
                daytimeStartVolumes[i] = daytimeSounds[i].volume;
            }
        }
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

        Debug.Log("Trời đã chuyển tối, và âm thanh môi trường lịm dần...");
    }

    private IEnumerator FadeToDarkRoutine()
    {
        float duration = 4f;
        float time = 0;

        float startAmbient = RenderSettings.ambientIntensity;
        Light dirLight = FindObjectOfType<Light>();
        float startLight = dirLight != null ? dirLight.intensity : 1f;

        // Lấy mức âm lượng hiện tại của âm thanh ban ngày ngay lúc bắt đầu chuyển tối
        float[] currentDayVolumes = new float[daytimeSounds.Length];
        for (int i = 0; i < daytimeSounds.Length; i++)
        {
            if (daytimeSounds[i] != null)
                currentDayVolumes[i] = daytimeSounds[i].volume;
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // 1. Giảm ánh sáng môi trường và ánh sáng mặt trời
            RenderSettings.ambientIntensity = Mathf.Lerp(startAmbient, 0.2f, t);
            if (dirLight != null) dirLight.intensity = Mathf.Lerp(startLight, 0.1f, t);

            // 2. Ép âm lượng các âm thanh ban ngày nhỏ dần về 0
            for (int i = 0; i < daytimeSounds.Length; i++)
            {
                if (daytimeSounds[i] != null)
                {
                    daytimeSounds[i].volume = Mathf.Lerp(currentDayVolumes[i], 0f, t);
                }
            }

            yield return null;
        }
        // Đảm bảo tắt hẳn (Stop) các âm thanh ban ngày sau khi đã fade xong
        for (int i = 0; i < daytimeSounds.Length; i++)
        {
            if (daytimeSounds[i] != null)
            {
                daytimeSounds[i].Stop();
            }
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

        // Bật lại các âm thanh ban ngày và trả về âm lượng nguyên bản
        for (int i = 0; i < daytimeSounds.Length; i++)
        {
            if (daytimeSounds[i] != null)
            {
                daytimeSounds[i].volume = daytimeStartVolumes[i];
                daytimeSounds[i].Play();
            }
        }

        Debug.Log("Trời đã sáng trở lại...");
    }
}