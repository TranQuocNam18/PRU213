using UnityEngine;

public class QuestMarker : MonoBehaviour
{
    Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // xoay nhanh hơn
        transform.Rotate(0, 180 * Time.deltaTime, 0);

        // bay lên xuống nhanh hơn
        float y = Mathf.Sin(Time.time * 4) * 0.4f;

        transform.localPosition = startPos + new Vector3(0, y, 0);
    }
}