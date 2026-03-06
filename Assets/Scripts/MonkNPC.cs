using UnityEngine;

public class MonkNPC : MonoBehaviour
{
    public DialogueManager dialogueManager;

    public string[] dialogueLines;   // thêm dòng này

    bool playerInRange = false;
    bool isTalking = false;
    float interactCooldown = 0f;
    bool hasPreGameTalked = false; // Check if the "return" dialogue has played

    void Update()
    {
        if (interactCooldown > 0f)
        {
            interactCooldown -= Time.unscaledDeltaTime;
        }

        if (playerInRange && !isTalking && interactCooldown <= 0f)
        {
            // Kiểm tra thêm: dialoguePanel phải đang tắt
            // Tránh trường hợp nhấn F kết thúc hội thoại xong bị bắt đầu lại ngay
            bool dialogueActive = DialogueManager.Instance != null &&
                                  DialogueManager.Instance.IsDialogueActive();

            if (!dialogueActive && Input.GetKeyDown(KeyCode.F))
            {
                Input.ResetInputAxes();
                StartTalk();
            }
        }
    }

    public void StartTalk()
    {
        // Kiểm tra xem đã nhặt đủ bùa chưa
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.objectiveCompleted)
        {
            if (!hasPreGameTalked)
            {
                // Tắt Talisman UI
                if (ObjectiveManager.Instance.talismanUI != null)
                {
                    ObjectiveManager.Instance.talismanUI.SetActive(false);
                }

                isTalking = true;
                string[] preGameLines = new string[] {
                    "Con đã thu thập đủ 5 lá bùa rồi sao! Tốt lắm.",
                    "Bây giờ, hãy dùng sức mạnh của ngũ hành Kim, Mộc, Thủy, Hỏa, Thổ để phong ấn nó!!",
                    "Cẩn thận, tâm trí con phải thật tập trung...",
                    "Nếu thất bại, con sẽ bị Ma Da chiếm lấy linh hồn!!"
                };
                dialogueManager.StartDialogue(preGameLines);
                hasPreGameTalked = true;
                return;
            }
            else
            {
                // Bắt đầu minigame sau khi đã nói chuyện xong
                SealingMinigame minigame = FindObjectOfType<SealingMinigame>();
                if (minigame == null)
                {
                    GameObject go = new GameObject("SealingMinigame");
                    minigame = go.AddComponent<SealingMinigame>();
                }
                minigame.StartMinigame();
                return;
            }
        }

        isTalking = true;

        dialogueManager.StartDialogue(dialogueLines); // truyền lines
    }

    public void EndTalk()
    {
        isTalking = false;
        interactCooldown = 2f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}