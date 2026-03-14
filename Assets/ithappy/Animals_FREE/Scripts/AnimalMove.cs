using UnityEngine;

public class AnimalMove : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float moveDuration = 3f;
    public float idleDuration = 2f;

    private float timer;
    private bool isMoving;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        ChooseAction();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (isMoving)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }

        if (timer <= 0)
        {
            ChooseAction();
        }
    }

    void ChooseAction()
    {
        isMoving = Random.value > 0.5f;

        if (isMoving)
        {
            timer = moveDuration;
            transform.Rotate(0, Random.Range(0, 360), 0);

            if (anim) anim.SetFloat("Vert", 1); // đi
        }
        else
        {
            timer = idleDuration;

            if (anim) anim.SetFloat("Vert", 0); // đứng
        }
    }
}