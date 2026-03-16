using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [HideInInspector] public MonkNPC currentMonk;
    [HideInInspector] public ElderNPC currentElder;

    public GameObject dialoguePanel;

    [Header("Speaker Labels")]
    public TextMeshProUGUI textMonk;
    public TextMeshProUGUI textElder;
    public TextMeshProUGUI textPlayer;

    [Header("Dialogue Content")]
    public TextMeshProUGUI dialogueText;

    [HideInInspector] public bool isTalking = false;

    private string[] lines;
    private int index = 0;

    // dùng cho độc thoại (không có NPC)
    private int playerVoiceCount = 0;

    private float nextLineCooldown = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (nextLineCooldown > 0f)
            nextLineCooldown -= Time.unscaledDeltaTime;

        if (dialoguePanel != null && dialoguePanel.activeSelf && nextLineCooldown <= 0f && Input.GetKeyDown(KeyCode.F))
        {
            nextLineCooldown = 0.2f;
            NextLine();
        }
    }

    public void StartDialogue(string[] dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
            return;

        lines = dialogueLines;
        index = 0;
        playerVoiceCount = 0;

        isTalking = true;

        // tránh bấm F nhảy dòng ngay lúc vừa mở
        nextLineCooldown = 0.3f;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (GameManager.Instance != null)
            GameManager.Instance.StartDialogue();

        DisplayLine(lines[index]);
    }

    private void StopAllDialogueVoices()
    {
        // Stop voice player (GameManager source)
        if (GameManager.Instance != null && GameManager.Instance.voiceSource != null)
            GameManager.Instance.voiceSource.Stop();

        // Stop voice elder
        if (currentElder != null && currentElder.voiceSource != null)
            currentElder.voiceSource.Stop();

        // Stop voice monk
        if (currentMonk != null && currentMonk.voiceSource != null)
            currentMonk.voiceSource.Stop();
    }

    private void PlayPlayerVoice(int voiceIdx)
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.voiceSource == null) return;
        if (GameManager.Instance.playerVoices == null) return;

        if (voiceIdx < 0 || voiceIdx >= GameManager.Instance.playerVoices.Length) return;

        AudioClip clip = GameManager.Instance.playerVoices[voiceIdx];
        if (clip == null) return;

        GameManager.Instance.voiceSource.Stop();
        GameManager.Instance.voiceSource.clip = clip;
        GameManager.Instance.voiceSource.Play();
    }

    private void DisplayLine(string line)
    {
        // Stop trước khi play line mới để không bị chồng / câu cũ còn vang
        StopAllDialogueVoices();

        if (textMonk != null) textMonk.gameObject.SetActive(false);
        if (textElder != null) textElder.gameObject.SetActive(false);
        if (textPlayer != null) textPlayer.gameObject.SetActive(false);

        string content = line;
        string trimmedLine = line.Trim();

        bool hasNpcSpeaker = (currentElder != null || currentMonk != null);

        if (trimmedLine.StartsWith("Nhà sư:"))
        {
            if (textMonk != null)
            {
                textMonk.gameObject.SetActive(true);
                textMonk.text = "Nhà sư";
            }
            content = trimmedLine.Replace("Nhà sư:", "").Trim();

            if (currentMonk != null)
                currentMonk.PlayVoiceForLine(index);
        }
        else if (trimmedLine.StartsWith("Trưởng Làng:"))
        {
            if (textElder != null)
            {
                textElder.gameObject.SetActive(true);
                textElder.text = "Trưởng Làng";
            }
            content = trimmedLine.Replace("Trưởng Làng:", "").Trim();

            if (currentElder != null)
                currentElder.PlayVoiceForLine(index);
        }
        else if (trimmedLine.StartsWith("Tôi:"))
        {
            if (textPlayer != null)
            {
                textPlayer.gameObject.SetActive(true);
                textPlayer.text = "Tôi";
            }
            content = trimmedLine.Replace("Tôi:", "").Trim();

            if (hasNpcSpeaker)
            {
                // Trong hội thoại NPC: player voice lấy từ mảng voice của NPC (clip p_) theo index
                if (currentElder != null) currentElder.PlayVoiceForLine(index);
                else if (currentMonk != null) currentMonk.PlayVoiceForLine(index);
            }
            else
            {
                // Độc thoại: lấy từ GameManager.playerVoices theo thứ tự
                PlayPlayerVoice(playerVoiceCount);
                playerVoiceCount++;
            }
        }

        if (dialogueText != null)
            dialogueText.text = content;
    }

    public void NextLine()
    {
        index++;
        if (lines != null && index < lines.Length) DisplayLine(lines[index]);
        else EndDialogue();
    }

    public void EndDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        isTalking = false;

        StopAllDialogueVoices();

        if (GameManager.Instance != null)
            GameManager.Instance.EndDialogue();

        if (currentElder != null) { currentElder.EndTalk(); currentElder = null; }
        if (currentMonk != null) { currentMonk.EndTalk(); currentMonk = null; }
    }

    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }
}