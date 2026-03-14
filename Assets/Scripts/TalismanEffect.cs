using UnityEngine;

public class TalismanEffect : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0, 60 * Time.deltaTime, 0);
    }
}