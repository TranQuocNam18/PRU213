using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    Light lightSource;

    void Start()
    {
        lightSource = GetComponent<Light>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            lightSource.enabled = !lightSource.enabled;
        }
    }
}
