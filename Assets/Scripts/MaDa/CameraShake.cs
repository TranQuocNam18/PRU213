using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    public void Shake(float duration, float strength)
    {
        StartCoroutine(ShakeCoroutine(duration, strength));
    }

    IEnumerator ShakeCoroutine(float duration, float strength)
    {
        float time = 0f;

        while (time < duration)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * strength;
            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
