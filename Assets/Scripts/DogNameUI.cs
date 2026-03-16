using UnityEngine;

public class DogNameUI : MonoBehaviour
{
    public GameObject nameUI;
    public float showDistance = 3f;

    Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        nameUI.SetActive(false);
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < showDistance)
            nameUI.SetActive(true);
        else
            nameUI.SetActive(false);
    }
}