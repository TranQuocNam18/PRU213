using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 3f;
    public GameObject interactText;
    public Camera playerCamera;

    private TextMeshProUGUI interactTMP;

    void Start()
    {
        if (interactText != null)
        {
            // Thử lấy TMP trực tiếp, nếu không có thì tìm trong children
            interactTMP = interactText.GetComponent<TextMeshProUGUI>();
            if (interactTMP == null)
                interactTMP = interactText.GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    void Update()
    {
        if (playerCamera == null) return;

        if (SealingMinigame.Instance != null && SealingMinigame.Instance.isPlaying)
        {
            if (interactText != null) interactText.SetActive(false);
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            MonkNPC monk = hit.collider.GetComponentInParent<MonkNPC>();
            ElderNPC elder = hit.collider.GetComponentInParent<ElderNPC>();

            if (monk != null || elder != null)
            {
                interactText.SetActive(true);
                if (interactTMP != null)
                    interactTMP.text = "Nói chuyện [F]";
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