using UnityEngine;
using System.Collections;

public class EnvironmentAudioManager : MonoBehaviour
{
    public Transform player;
    public Transform mada;

    public AudioSource natureAudio;
    public AudioSource noiseAudio;

    public float madaDistance = 25f;
    public float delayTime = 2f;

    bool playerNearMada = false;
    Coroutine audioRoutine;

    void Start()
    {
        natureAudio.Play();
    }

    void Update()
    {
        float dist = Vector3.Distance(player.position, mada.position);

        if (dist < madaDistance && !playerNearMada)
        {
            playerNearMada = true;

            if (audioRoutine != null)
                StopCoroutine(audioRoutine);

            audioRoutine = StartCoroutine(SwitchToHorror());
        }

        if (dist >= madaDistance && playerNearMada)
        {
            playerNearMada = false;

            if (audioRoutine != null)
                StopCoroutine(audioRoutine);

            audioRoutine = StartCoroutine(SwitchToNature());
        }
    }

    IEnumerator SwitchToHorror()
    {
        natureAudio.Stop();

        yield return new WaitForSeconds(delayTime);

        noiseAudio.Play();
    }

    IEnumerator SwitchToNature()
    {
        noiseAudio.Stop();

        yield return new WaitForSeconds(delayTime);

        natureAudio.Play();
    }
}