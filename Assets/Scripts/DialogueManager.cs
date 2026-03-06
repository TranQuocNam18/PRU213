using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public bool isTalking = false;

    private string[] lines;
    private int index = 0;
    private float nextLineCooldown = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Instance != this) return;

        if (nextLineCooldown > 0f)
        {
            nextLineCooldown -= Time.unscaledDeltaTime;
        }

        if (dialoguePanel.activeSelf && nextLineCooldown <= 0f && Input.GetKeyDown(KeyCode.F))
        {
            nextLineCooldown = 0.2f;
            NextLine();
            Input.ResetInputAxes(); // Xóa trạng thái phím trong frame này
        }
    }

    public void StartDialogue(string[] dialogueLines)
    {
        // tránh lỗi nếu mảng rỗng
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogError("Dialogue lines is empty!");
            return;
        }

        lines = dialogueLines;
        index = 0;

        dialoguePanel.SetActive(true);
        dialogueText.text = lines[index];
        isTalking = true;
        nextLineCooldown = 0.2f;

        if (GameManager.Instance != null)
            GameManager.Instance.StartDialogue();
    }

    public void NextLine()
    {
        index++;

        if (index < lines.Length)
        {
            dialogueText.text = lines[index];
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.EndDialogue();
        isTalking = false;

        // chỉ bắt đầu nhiệm vụ 1 lần
        if (!ObjectiveManager.Instance.objectiveStarted)
        {
            ObjectiveManager.Instance.StartObjective();
        }
        SkyManager.Instance.ChangeToDark();
        // reset trạng thái NPC
        MonkNPC monk = FindObjectOfType<MonkNPC>();
        if (monk != null)
        {
            monk.EndTalk();
        }
    }

    /// <summary>
    /// Trả về true nếu dialogue panel đang hiển thị.
    /// Dùng để MonkNPC kiểm tra trước khi cho phép bắt đầu hội thoại mới.
    /// </summary>
    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }
}