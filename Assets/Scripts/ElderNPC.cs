using UnityEngine;

public class ElderNPC : MonoBehaviour
{
    public DialogueManager dialogueManager;

    // private để Unity KHÔNG serialize - đảm bảo dùng đúng giá trị trong code
    private static readonly string[] linesMeetElder = new string[] {
        "Trưởng Làng: Chào cậu thanh niên. Lâu lắm mới có người lạ đến ngôi làng này.",
        "Tôi: Chào bác. Cháu tới đây để tham quan ngôi làng sinh thái nghe đồn là bị ma ám này.",
        "Trưởng Làng: Trông cậu không giống người đến để viếng thăm đền thờ. Dạo này làng có nhiều chuyện lạ lắm...",
        "Trưởng Làng: Cậu nên đến gặp Thầy Mùi ở đền thờ trên đồi tĩnh tâm trước khi trời tối."
    };

    private static readonly string[] linesElderQuest = new string[] {
        "Trưởng Làng: Lạy trời Phật, cậu vẫn còn sống! Nó đang tìm cậu...",
        "Tôi: Bác trưởng làng... thứ đó là gì vậy?!",
        "Trưởng Làng: Đó là Ma Da, một oan hồn dưới nước không thể siêu thoát.",
        "Tôi: Cháu phải làm sao đây?",
        "Trưởng Làng: Cậu cần tìm 5 lá bùa được giấu quanh làng để Thầy Mùi làm lễ phong ấn.",
        "Trưởng Làng: Mau lên trước khi quá muộn!"
    };

    bool playerInRange = false;
    bool isTalking = false;
    float interactCooldown = 0f;

    void Update()
    {
        if (interactCooldown > 0f)
        {
            interactCooldown -= Time.unscaledDeltaTime;
        }

        if (playerInRange && !isTalking && interactCooldown <= 0f)
        {
            bool dialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();

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

        if (GameManager.Instance.currentState == GameManager.StoryState.MeetElder)
        {
            dialogueManager.StartDialogue(linesMeetElder);
            GameManager.Instance.AdvanceStoryState(GameManager.StoryState.MeetMonk);
        }
        else if (GameManager.Instance.currentState == GameManager.StoryState.NightStalking)
        {
            dialogueManager.StartDialogue(linesElderQuest);
            GameManager.Instance.AdvanceStoryState(GameManager.StoryState.SearchTalismans);

            if (ObjectiveManager.Instance != null)
            {
                ObjectiveManager.Instance.StartObjective();
            }
        }
        else if (GameManager.Instance.currentState == GameManager.StoryState.SearchTalismans)
        {
            dialogueManager.StartDialogue(new string[] {
                "Trưởng Làng: Mau tìm đủ 5 lá bùa và mang đến cho Thầy Mùi!"
            });
        }
        else
        {
            dialogueManager.StartDialogue(new string[] {
                "Trưởng Làng: Cẩn thận nhé chàng trai."
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
