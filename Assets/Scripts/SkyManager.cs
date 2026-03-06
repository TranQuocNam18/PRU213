using UnityEngine;

public class SkyManager : MonoBehaviour
{
    public static SkyManager Instance;

    public Material daySkybox;
    public Material nightSkybox;

    void Awake()
    {
        Instance = this;
    }

    public void ChangeToDark()
    {
        RenderSettings.skybox = nightSkybox;

        // Giảm ánh sáng môi trường
        RenderSettings.ambientIntensity = 0.4f;

        // Tối hơn
        Light dirLight = FindObjectOfType<Light>();
        if (dirLight != null)
        {
            dirLight.intensity = 0.2f;
        }

        Debug.Log("Trời đã chuyển tối...");
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

        Debug.Log("Trời đã sáng trở lại...");
    }
}