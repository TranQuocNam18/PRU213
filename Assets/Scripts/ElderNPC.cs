using UnityEngine;

public class ElderNPC : MonoBehaviour
{
    public DialogueManager dialogueManager;

    // private để Unity KHÔNG serialize - đảm bảo dùng đúng giá trị trong code
    private static readonly string[] linesMeetElder = new string[] {
        "Trưởng Làng: Chào cậu thanh niên... Lâu lắm rồi mới có người lạ đến làng này.",
        "Tôi: Cháu đến du lịch thôi. Nghe nói nơi này từng là làng du lịch sinh thái nổi tiếng.",
        "Trưởng Làng: ...Ngày xưa là vậy.",
        "Trưởng Làng: Nhưng dạo gần đây trong làng xảy ra nhiều chuyện lạ. Người ngoài ít ai dám tới.",
        "Tôi: Cháu cũng nghe mấy lời đồn ma quỷ trên mạng, nhưng chắc chỉ là tin đồn thôi.",
        "Trưởng Làng: ...Cậu nên đến ngôi chùa cổ ở rìa làng và gặp Sư Thầy.",
        "Trưởng Làng: Ông ấy biết rõ những chuyện đang xảy ra ở đây hơn ta."
    };

    private static readonly string[] linesElderQuest = new string[] {
        "Trưởng Làng: Trời Phật phù hộ... cậu vẫn còn sống!",
        "Tôi: Bác trưởng làng! Thứ gì đó... nó đang bám theo cháu!",
        "Trưởng Làng: Ta biết... đó là Ma Da.",
        "Tôi: Vậy mấy chuyện cháu nghe... đều là thật sao?",
        "Trưởng Làng: Những linh hồn chết đuối bị mắc kẹt dưới nước.",
        "Trưởng Làng: Chúng chỉ có thể siêu thoát khi có người khác chết thay.",
        "Tôi: Cháu phải làm gì bây giờ?!",
        "Trưởng Làng: Trong làng có 5 lá bùa phong ấn đã bị thất lạc.",
        "Trưởng Làng: Nếu cậu tìm đủ và mang đến cho Sư Thầy, ông ấy có thể làm nghi thức trừ tà.",
        "Trưởng Làng: Mau lên... trước khi nó tìm được cậu."
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
