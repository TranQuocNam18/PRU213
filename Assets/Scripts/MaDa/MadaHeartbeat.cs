using UnityEngine;

public class MadaHeartbeat : MonoBehaviour
{
    public Transform mada;
    public AudioSource heartbeatSource;

    public float maxDistance = 6f;
    public float behindDot = -0.2f;

    void Update()
    {
        if (!mada) return;

        Vector3 dirToMada = (mada.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, dirToMada);
        float distance = Vector3.Distance(transform.position, mada.position);

        bool madaBehind = dot < behindDot;
        bool madaClose = distance < maxDistance;

        if (madaBehind && madaClose)
        {
            if (!heartbeatSource.isPlaying)
                heartbeatSource.Play();

            heartbeatSource.pitch = Mathf.Lerp(1.4f, 0.8f, distance / maxDistance);
            heartbeatSource.volume = Mathf.Lerp(1f, 0.2f, distance / maxDistance);
        }
        else
        {
            heartbeatSource.Stop();
        }
    }
}
