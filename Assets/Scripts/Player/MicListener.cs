using UnityEngine;

public class MicListener : MonoBehaviour
{
    public float loudness;

    AudioClip micClip;
    const int sampleWindow = 128;

    void Start()
    {
        micClip = Microphone.Start(null, true, 10, 44100);
    }

    void Update()
    {
        loudness = GetLoudness();
    }

    float GetLoudness()
    {
        int micPos = Microphone.GetPosition(null) - sampleWindow;
        if (micPos < 0) return 0;

        float[] samples = new float[sampleWindow];
        micClip.GetData(samples, micPos);

        float sum = 0;
        foreach (float s in samples)
            sum += Mathf.Abs(s);

        return sum / sampleWindow;
    }
}
