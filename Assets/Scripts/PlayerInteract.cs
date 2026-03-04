using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 3f;
    public GameObject interactText;

    void Update()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // ======= DOOR =======
            RitualDoor door = hit.collider.GetComponent<RitualDoor>();
            if (door != null)
            {
                interactText.SetActive(true);
                interactText.GetComponent<TMPro.TextMeshProUGUI>().text = "Mở cửa [F]";

                if (Input.GetKeyDown(KeyCode.F))
                {
                    door.OpenDoor();
                }

                return;
            }

            // ======= NOTE =======
            NoteInteraction note = hit.collider.GetComponent<NoteInteraction>();
            if (note != null)
            {
                interactText.SetActive(true);
                interactText.GetComponent<TMPro.TextMeshProUGUI>().text = "Đọc [F]";

                return;
            }

            interactText.SetActive(false);
        }
        else
        {
            interactText.SetActive(false);
        }
    }
}