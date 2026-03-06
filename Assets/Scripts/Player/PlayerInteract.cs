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

            if (monk != null)
            {
                interactText.SetActive(true);
                interactText.GetComponent<TextMeshProUGUI>().text = "Nói chuyện [F]";

                // Xử lý phím F đã được chuyển sang MonkNPC.cs và DialogueManager.cs
                // để tránh bị nhận đúp (double input)
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