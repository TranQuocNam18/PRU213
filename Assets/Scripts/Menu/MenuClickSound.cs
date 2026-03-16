using UnityEngine;

public class MenuClickSound : MonoBehaviour
{
    public AudioSource source;
    public AudioClip clickSound;

    public void PlayClick()
    {
        source.PlayOneShot(clickSound);
    }
}