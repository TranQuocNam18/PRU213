using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    public AudioSource walking;
    public AudioSource running;
    public AudioSource breathing;

    void StopAllMovementSound()
    {
        walking.Stop();
        running.Stop();
        breathing.Stop();
    }

    public void PlayWalking()
    {
        if (!walking.isPlaying)
        {
            StopAllMovementSound();
            walking.Play();
        }
    }

    public void PlayRunning()
    {
        if (!running.isPlaying)
        {
            StopAllMovementSound();
            running.Play();
        }
    }

    public void PlayBreathing()
    {
        if (!breathing.isPlaying)
        {
            StopAllMovementSound();
            breathing.Play();
        }
    }
}