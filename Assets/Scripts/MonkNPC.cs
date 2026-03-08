using UnityEngine;

public class MonkNPC : MonoBehaviour
{
    public DialogueManager dialogueManager;

    bool playerInRange = false;
    bool isTalking = false;
    float interactCooldown = 0f;
    bool hasPreGameTalked = false;

    // private static readonly - Unity KHÔNG serialize, luôn dùng đúng giá trị trong code
    private static readonly string[] linesMeetMonk = new string[] {
        "Sư Thầy: A Di Đà Phật. Làng này vốn yên bình nhưng gần đây âm khí rất nặng.",
        "Tôi: Cháu không tin vào mấy chuyện tâm linh đâu. Nhưng đúng là không khí ở đây thật quái lạ.",
        "Sư Thầy: Trời sắp tối rồi, cậu nên quay lại nhà trưởng làng nghỉ ngơi đi.",
        "Tôi: Vâng cháu hiểu rồi.",
        "Sư Thầy: Đi cẩn thận, tuyệt đối đừng lại gần ao hồ lúc nhập nhoạng tối..."
    };

    private static readonly string[] linesPreGame = new string[] {
        "Sư Thầy: Con đã thu thập đủ 5 lá bùa rồi sao! Tốt lắm.",
        "Tôi: Thầy ơi, mau tìm cách đuổi nó đi!",
        "Sư Thầy: Bây giờ, hãy dùng sức mạnh của ngũ hành Kim, Mộc, Thủy, Hỏa, Thổ để phong ấn nó!!",
        "Sư Thầy: Cẩn thận, tâm trí con phải thật tập trung...",
        "Sư Thầy: Nếu thất bại, con sẽ bị Ma Da chiếm lấy linh hồn!!"
    };

    void Update()
    {
        if (interactCooldown > 0f)
        {
            interactCooldown -= Time.unscaledDeltaTime;
        }

        if (playerInRange && !isTalking && interactCooldown <= 0f)
        {
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
        if (GameManager.Instance == null) return;

        isTalking = true;

        if (GameManager.Instance.currentState == GameManager.StoryState.MeetMonk)
        {
            dialogueManager.StartDialogue(linesMeetMonk);
            GameManager.Instance.AdvanceStoryState(GameManager.StoryState.NightStalking);
        }
        else if (GameManager.Instance.currentState == GameManager.StoryState.ReturnMonk ||
                 (ObjectiveManager.Instance != null && ObjectiveManager.Instance.objectiveCompleted))
        {
            GameManager.Instance.AdvanceStoryState(GameManager.StoryState.Minigame);

            if (!hasPreGameTalked)
            {
                if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.talismanUI != null)
                {
                    ObjectiveManager.Instance.talismanUI.SetActive(false);
                }

                dialogueManager.StartDialogue(linesPreGame);
                hasPreGameTalked = true;
            }
            else
            {
                SealingMinigame minigame = FindObjectOfType<SealingMinigame>();
                if (minigame == null)
                {
                    GameObject go = new GameObject("SealingMinigame");
                    minigame = go.AddComponent<SealingMinigame>();
                }
                minigame.StartMinigame();
            }
        }
        else if (GameManager.Instance.currentState == GameManager.StoryState.SearchTalismans)
        {
            dialogueManager.StartDialogue(new string[] {
                "Sư Thầy: Hãy tìm bùa đi đã, bần tăng sẽ giúp con phong ấn nó."
            });
        }
        else
        {
            dialogueManager.StartDialogue(new string[] {
                "Sư Thầy: A Di Đà Phật."
            });
        }
    }

    public void EndTalk()
    {
        isTalking = false;
        interactCooldown = 1f;
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