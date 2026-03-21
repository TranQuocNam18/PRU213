using UnityEngine;

public class SoundEvent : MonoBehaviour
{
    public AudioSource audioSource;

    public void PlayFootstep()
    {
        audioSource.Play();
    }

    public void StopFootstep()
    {
        audioSource.Stop();
    }
}