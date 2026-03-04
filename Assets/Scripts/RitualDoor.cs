using UnityEngine;

public class RitualDoor : MonoBehaviour
{
    public Animator leftDoor;
    public Animator rightDoor;
    public float openAngle = 90f;
    public float openSpeed = 2f;

    private bool isOpening = false;
    private Quaternion targetRotation;

    void Start()
    {
        targetRotation = Quaternion.Euler(0, openAngle, 0) * transform.rotation;
    }

    void Update()
    {
        if (isOpening)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * openSpeed
            );
        }
    }

    public void OpenDoor()
    {
        if (leftDoor != null)
            leftDoor.Play("DoorOpen_Left");

        if (rightDoor != null)
            rightDoor.Play("DoorOpen_Right");
    }
}