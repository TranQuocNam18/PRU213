using UnityEngine;

public class MaDaLookDirection : MonoBehaviour
{
    Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        Vector3 moveDir = transform.position - lastPosition;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            moveDir.y = 0;

            Quaternion lookRot = Quaternion.LookRotation(moveDir.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRot,
                10f * Time.deltaTime
            );
        }

        lastPosition = transform.position;
    }
}
