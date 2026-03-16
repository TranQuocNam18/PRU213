using UnityEngine;

public class ElderNPC : MonoBehaviour
{
    public DialogueManager dialogueManager;

    [Header("Look At Player")]
    public float lookAtSpeed = 5f;
    public AudioSource voiceSource;

    [Header("Meet Elder Voice (sync theo index lines)")]
    public AudioClip[] meetElderVoice;

    [Header("Quest Voice (sync theo index lines)")]
    public AudioClip[] elderQuestVoice;

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
        "Trưởng Làng: Trong làng có 5 lá bùa phong ấn đã bị thất l��c.",
        "Trưởng Làng: Nếu cậu tìm đủ và mang đến cho Sư Thầy, ông ấy có thể làm nghi thức trừ tà.",
        "Trưởng Làng: Mau lên... trước khi nó tìm được cậu."
    };

    bool playerInRange = false;
    bool isTalking = false;
    float interactCooldown = 0f;
    Transform playerTransform;

    private GameManager.StoryState conversationState;

    void Update()
    {
        if (interactCooldown > 0f) interactCooldown -= Time.unscaledDeltaTime;

        if (playerInRange && playerTransform != null)
        {
            Vector3 direction = playerTransform.position - transform.position;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, lookAtSpeed * Time.unscaledDeltaTime);
            }
        }

        if (playerInRange && !isTalking && interactCooldown <= 0f)
        {
            if (DialogueManager.Instance != null &&
                !DialogueManager.Instance.IsDialogueActive() &&
                Input.GetKeyDown(KeyCode.F))
            {
                StartTalk();
            }
        }
    }

    public void StartTalk()
    {
        if (GameManager.Instance == null || dialogueManager == null) return;

        isTalking = true;

        DialogueManager.Instance.currentElder = this;
        DialogueManager.Instance.currentMonk = null;

        conversationState = GameManager.Instance.currentState;

        if (conversationState == GameManager.StoryState.MeetElder)
        {
            dialogueManager.StartDialogue(linesMeetElder);
            GameManager.Instance.AdvanceStoryState(GameManager.StoryState.MeetMonk);
        }
        else if (conversationState == GameManager.StoryState.NightStalking)
        {
            dialogueManager.StartDialogue(linesElderQuest);
            GameManager.Instance.AdvanceStoryState(GameManager.StoryState.SearchTalismans);
            if (ObjectiveManager.Instance != null) ObjectiveManager.Instance.StartObjective();
        }
        else if (conversationState == GameManager.StoryState.SearchTalismans)
        {
            dialogueManager.StartDialogue(new string[] { "Trưởng Làng: Mau tìm đủ 5 lá bùa và mang đến cho Thầy Mùi!" });
        }
        else
        {
            dialogueManager.StartDialogue(new string[] { "Trưởng Làng: Cẩn thận nhé chàng trai." });
        }
    }

    public void PlayVoiceForLine(int lineIndex)
    {
        AudioClip clip = null;

        if (conversationState == GameManager.StoryState.MeetElder)
        {
            if (meetElderVoice != null && lineIndex >= 0 && lineIndex < meetElderVoice.Length)
                clip = meetElderVoice[lineIndex];
        }
        else if (conversationState == GameManager.StoryState.NightStalking ||
                 conversationState == GameManager.StoryState.SearchTalismans)
        {
            if (elderQuestVoice != null && lineIndex >= 0 && lineIndex < elderQuestVoice.Length)
                clip = elderQuestVoice[lineIndex];
        }

        if (clip == null) return;

        // Voice người chơi (p_) -> GameManager voiceSource
        if (clip.name.StartsWith("p_"))
        {
            if (GameManager.Instance != null && GameManager.Instance.voiceSource != null)
            {
                GameManager.Instance.voiceSource.Stop();
                GameManager.Instance.voiceSource.clip = clip;
                GameManager.Instance.voiceSource.Play();
            }
        }
        else // Voice trưởng làng (tr_)
        {
            if (voiceSource != null)
            {
                voiceSource.Stop();
                voiceSource.clip = clip;
                voiceSource.Play();
            }
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
            playerTransform = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerTransform = null;
        }
    }
}