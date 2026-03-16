using UnityEngine;

public class MonkNPC : MonoBehaviour
{
    public DialogueManager dialogueManager;

    [Header("Look At Player")]
    public float lookAtSpeed = 5f;
    public AudioSource voiceSource;

    [Header("Voice Clips (sync theo index lines, có cả p_)")]
    public AudioClip[] meetMonkVoice;
    public AudioClip[] preGameVoice;

    private static readonly string[] linesMeetMonk = new string[] {
        "Nhà sư: Ta đã chờ con đến đây.",
        "Tôi: Thầy là ai?",
        "Nhà sư: Ta là người trông coi ngôi chùa cổ này... và cũng là người trấn giữ những thứ không nên tồn tại.",
        "Tôi: Ý thầy là mấy lời đồn về ma quỷ trong làng sao?",
        "Nhà sư: Khi con bước vào đây, ta đã cảm nhận được âm khí bám theo con.",
        "Tôi: Âm khí? Cháu chỉ đi ngang qua mấy cái hồ thôi mà.",
        "Nhà sư: Chính những hồ nước đó là nơi Ma Da trú ngụ.",
        "Nhà sư: Những vong hồn chết đuối không siêu thoát, luôn tìm người thế mạng.",
        "Tôi: Thầy nói vậy là thứ đó... có thật?",
        "Nhà sư: Đêm nay đừng lại gần bất kỳ ao hồ nào. Nếu không... nó sẽ tìm đến con.",
        "Nhà sư: Tốt nhất con nên quay lại nhà trưởng làng nghỉ ngơi đi.",
        "Nhà sư: Đi đi trước khi trời tối hẳn.",
        "Tôi: Vâng cháu hiểu rồi."
    };

    private static readonly string[] linesPreGame = new string[] {
        "Nhà sư: Tốt lắm... con đã tìm đủ 5 lá bùa phong ấn.",
        "Tôi: Thầy ơi, thứ đó vẫn đang đuổi theo cháu!",
        "Nhà sư: Bình tĩnh. Đây là nghi thức phong ấn cuối cùng.",
        "Nhà sư: Nhưng con phải tự tay hoàn thành ấn chú.",
        "Tôi: Cháu phải làm gì?",
        "Nhà sư: Hãy vẽ lại phù chú phong ấn bằng tất cả sự tập trung của con.",
        "Nhà sư: Nếu nghi thức thất bại...",
        "Nhà sư: Ma Da sẽ chiếm lấy linh hồn của con."
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

        DialogueManager.Instance.currentMonk = this;
        DialogueManager.Instance.currentElder = null;

        conversationState = GameManager.Instance.currentState;

        if (conversationState == GameManager.StoryState.MeetMonk)
        {
            dialogueManager.StartDialogue(linesMeetMonk);
            GameManager.Instance.AdvanceStoryState(GameManager.StoryState.NightStalking);
        }
        else if (conversationState == GameManager.StoryState.ReturnMonk ||
                 (ObjectiveManager.Instance != null && ObjectiveManager.Instance.objectiveCompleted))
        {
            GameManager.Instance.AdvanceStoryState(GameManager.StoryState.Minigame);

            if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.talismanUI != null)
                ObjectiveManager.Instance.talismanUI.SetActive(false);

            dialogueManager.StartDialogue(linesPreGame);
        }
        else if (conversationState == GameManager.StoryState.SearchTalismans)
        {
            dialogueManager.StartDialogue(new string[] { "Nhà sư: Hãy tìm bùa đi đã, bần đạo sẽ giúp con phong ấn nó." });
        }
        else
        {
            dialogueManager.StartDialogue(new string[] { "Nhà sư: A di đà Phật..." });
        }
    }

    public void PlayVoiceForLine(int lineIndex)
    {
        AudioClip clip = null;

        if (conversationState == GameManager.StoryState.MeetMonk)
        {
            if (meetMonkVoice != null && lineIndex >= 0 && lineIndex < meetMonkVoice.Length)
                clip = meetMonkVoice[lineIndex];
        }
        else if (conversationState == GameManager.StoryState.Minigame || conversationState == GameManager.StoryState.ReturnMonk)
        {
            if (preGameVoice != null && lineIndex >= 0 && lineIndex < preGameVoice.Length)
                clip = preGameVoice[lineIndex];
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
        else // Voice nhà sư
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