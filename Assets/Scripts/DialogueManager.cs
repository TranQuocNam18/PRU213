using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [HideInInspector] public MonkNPC currentMonk;
    [HideInInspector] public ElderNPC currentElder;
    [HideInInspector] public OngTamNPC currentOngTam;

    // Dialogue của Ông Tám có 2 đoạn: lần đầu và repeat
    [HideInInspector] public bool isOngTamRepeatDialogue = false;
    public GameObject dialoguePanel;

    [Header("Speaker Labels")]
    public TextMeshProUGUI textMonk;
    public TextMeshProUGUI textElder;
    public TextMeshProUGUI textPlayer;
    public TextMeshProUGUI textOngTam;

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
            Input.ResetInputAxes();
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

        // Stop voice Ông Tám
        if (currentOngTam != null)
            currentOngTam.StopVoice();
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

    private void HideAllSpeakers()
    {
        if (textMonk != null) textMonk.gameObject.SetActive(false);
        if (textElder != null) textElder.gameObject.SetActive(false);
        if (textPlayer != null) textPlayer.gameObject.SetActive(false);
        if (textOngTam != null) textOngTam.gameObject.SetActive(false);
    }

    private void DisplayLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            if (dialogueText != null)
                dialogueText.text = "";
            HideAllSpeakers();
            StopAllDialogueVoices();
            return;
        }

        // Stop trước khi play line mới để không bị chồng / câu cũ còn vang
        StopAllDialogueVoices();

        // Tắt tất cả speaker trước khi bật speaker hiện tại
        HideAllSpeakers();

        string trimmedLine = line.Trim();
        string content = trimmedLine;

        bool hasNpcSpeaker = (currentElder != null || currentMonk != null);

        if (trimmedLine.StartsWith("Nhà sư:"))
        {
            if (textMonk != null)
            {
                textMonk.gameObject.SetActive(true);
                textMonk.text = "Nhà sư";
            }

            content = trimmedLine.Substring("Nhà sư:".Length).Trim();

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

            content = trimmedLine.Substring("Trưởng Làng:".Length).Trim();

            if (currentElder != null)
                currentElder.PlayVoiceForLine(index);
        }
        else if (trimmedLine.StartsWith("Ông Tám:"))
        {
            if (textOngTam != null)
            {
                textOngTam.gameObject.SetActive(true);
                textOngTam.text = "Ông Tám";
            }

            content = trimmedLine.Substring("Ông Tám:".Length).Trim();

            // Phát voice theo chỉ số dòng hiện tại
            if (currentOngTam != null)
                currentOngTam.PlayVoiceForOngTamLine(index, isOngTamRepeatDialogue);
        }
        else if (trimmedLine.StartsWith("Tôi:"))
        {
            if (textPlayer != null)
            {
                textPlayer.gameObject.SetActive(true);
                textPlayer.text = "Tôi";
            }

            content = trimmedLine.Substring("Tôi:".Length).Trim();

            // Nếu đang nói với Ông Tám: dùng voice của Ông Tám theo index line
            if (currentOngTam != null)
            {
                currentOngTam.PlayVoiceForOngTamLine(index, isOngTamRepeatDialogue);
            }
            else if (hasNpcSpeaker)
            {
                // Elder/Monk vẫn giữ như cũ
                if (currentElder != null) currentElder.PlayVoiceForLine(index);
                else if (currentMonk != null) currentMonk.PlayVoiceForLine(index);
            }
            else
            {
                // Độc thoại: dùng voice player theo thứ tự
                PlayPlayerVoice(playerVoiceCount);
                playerVoiceCount++;
            }
        }
        else
        {
            // narration/line bình thường
            content = trimmedLine;
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

        // Ông Tám: kết thúc và advance state nếu là lần nói đầu tiên
        if (currentOngTam != null)
        {
            bool wasRepeat = isOngTamRepeatDialogue;
            currentOngTam.EndTalk();
            currentOngTam = null;
            isOngTamRepeatDialogue = false;

            if (!wasRepeat && GameManager.Instance != null &&
                GameManager.Instance.currentState == GameManager.StoryState.MeetOngTam)
            {
                GameManager.Instance.AdvanceStoryState(GameManager.StoryState.MeetElder);
            }
            return;
        }

        isOngTamRepeatDialogue = false;
    }

    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }
}
