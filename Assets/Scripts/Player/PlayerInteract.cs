using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 3f;
    public GameObject interactText;
    public Camera playerCamera;

    void Update()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            MonkNPC monk = hit.collider.GetComponentInParent<MonkNPC>();
            ElderNPC elder = hit.collider.GetComponentInParent<ElderNPC>();

            if (monk != null || elder != null)
            {
                interactText.SetActive(true);
                interactText.GetComponent<TextMeshProUGUI>().text = "Nói chuyện [F]";
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