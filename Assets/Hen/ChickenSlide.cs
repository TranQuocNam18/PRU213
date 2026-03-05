using UnityEngine;

public class ChickenSlide : MonoBehaviour
{
    public float speed = 2f;
    public float moveRadius = 5f;

    private Vector3 startPos;
    private Vector3 target;

    void Start()
    {
        startPos = transform.position;
        SetNewTarget();
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        Vector3 direction = target - transform.position;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                5f * Time.deltaTime
            );
        }

        if (Vector3.Distance(transform.position, target) < 0.3f)
        {
            SetNewTarget();
        }
    }

    void SetNewTarget()
    {
        float randomX = Random.Range(-moveRadius, moveRadius);
        float randomZ = Random.Range(-moveRadius, moveRadius);

        target = new Vector3(
            startPos.x + randomX,
            startPos.y,
            startPos.z + randomZ
        );
    }
}