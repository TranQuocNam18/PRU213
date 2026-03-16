using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public GameObject dialoguePanel;

    [Header("Speaker Texts")]
    public TextMeshProUGUI textMonk;
    public TextMeshProUGUI textElder;
    public TextMeshProUGUI textPlayer;
    public TextMeshProUGUI textOngTam;

    [Header("Dialogue Content")]
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
        {
            Debug.LogError("Dialogue lines is empty!");
            return;
        }

        lines = dialogueLines;
        index = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        DisplayLine(lines[index]);

        isTalking = true;
        nextLineCooldown = 0.2f;

        if (GameManager.Instance != null)
            GameManager.Instance.StartDialogue();
    }

    private void DisplayLine(string line)
    {
        // Tắt tất cả speaker
        if (textMonk != null) textMonk.gameObject.SetActive(false);
        if (textElder != null) textElder.gameObject.SetActive(false);
        if (textPlayer != null) textPlayer.gameObject.SetActive(false);
        if (textOngTam != null) textOngTam.gameObject.SetActive(false);

        string contentString = line;

        if (line.StartsWith("Nhà sư:"))
        {
            if (textMonk != null)
            {
                textMonk.gameObject.SetActive(true);
                textMonk.text = "Nhà sư";
            }

            contentString = line.Substring("Nhà sư:".Length).Trim();
        }
        else if (line.StartsWith("Trưởng Làng:"))
        {
            if (textElder != null)
            {
                textElder.gameObject.SetActive(true);
                textElder.text = "Trưởng Làng";
            }

            contentString = line.Substring("Trưởng Làng:".Length).Trim();
        }
        else if (line.StartsWith("Ông Tám:"))
        {
            if (textOngTam != null)
            {
                textOngTam.gameObject.SetActive(true);
                textOngTam.text = "Ông Tám";
            }

            contentString = line.Substring("Ông Tám:".Length).Trim();
        }
        else if (line.StartsWith("Tôi:"))
        {
            if (textPlayer != null)
            {
                textPlayer.gameObject.SetActive(true);
                textPlayer.text = "Tôi";
            }

            contentString = line.Substring("Tôi:".Length).Trim();
        }

        if (dialogueText != null)
        {
            dialogueText.text = contentString;
        }
    }

    public void NextLine()
    {
        index++;

        if (index < lines.Length)
        {
            DisplayLine(lines[index]);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.EndDialogue();

        isTalking = false;

        // Reset NPC states

        MonkNPC monk = FindObjectOfType<MonkNPC>();
        if (monk != null)
            monk.EndTalk();

        ElderNPC elder = FindObjectOfType<ElderNPC>();
        if (elder != null)
            elder.EndTalk();

        OngTamNPC ongTam = FindObjectOfType<OngTamNPC>();
        if (ongTam != null)
            ongTam.EndTalk();
    }

    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }
}