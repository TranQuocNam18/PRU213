using UnityEngine;

public class OngTamNPC : MonoBehaviour
{
    public GameObject interactUI;
    public DialogueManager dialogueManager;

    [Header("Voice")]
    public AudioSource voiceSource;
    public AudioClip[] ongTamVoices;        // voice cho đoạn nói lần đầu (linesOngTam)
    public AudioClip[] ongTamRepeatVoices;  // voice cho đoạn nói lại (repeatLines) - nếu muốn

    [Header("Name Label")]
    public GameObject nameLabel;

    [Header("Look At Player")]
    public float lookAtSpeed = 5f;

    [Header("Interact Distance")]
    public float interactDistance = 7f;

    [Header("Name Display Distance")]
    public float nameDistance = 6f;

    private bool talking = false;
    private bool hasTalked = false;

    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;
        }

        // đảm bảo UI tắt lúc bắt đầu
        if (interactUI != null)
            interactUI.SetActive(false);

        if (nameLabel != null)
            nameLabel.SetActive(false);
    }

    void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(playerTransform.position, transform.position);

        // NPC nhìn player
        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                lookAtSpeed * Time.deltaTime
            );
        }

        // Name distance
        if (nameLabel != null)
        {
            if (dist <= nameDistance && !talking)
                nameLabel.SetActive(true);
            else
                nameLabel.SetActive(false);
        }

        // Interact distance
        if (dist <= interactDistance && !talking)
        {
            interactUI.SetActive(true);

            if (Input.GetKeyDown(KeyCode.F))
            {
                interactUI.SetActive(false);
                nameLabel.SetActive(false);

                talking = true;

                if (dialogueManager == null)
                {
                    Debug.LogError("OngTamNPC: dialogueManager reference is NULL (chưa kéo trong Inspector)");
                    return;
                }

                // DÙNG reference trực tiếp, KHÔNG dùng DialogueManager.Instance
                dialogueManager.currentOngTam = this;
                dialogueManager.isOngTamRepeatDialogue = hasTalked;

                if (!hasTalked)
                    dialogueManager.StartDialogue(linesOngTam);
                else
                    dialogueManager.StartDialogue(repeatLines);
            }
        }
        else
        {
            interactUI.SetActive(false);
        }
    }

    public void EndTalk()
    {
        talking = false;
        hasTalked = true;
        StopVoice();
    }

    private static readonly string[] linesOngTam =
    {
        "Ông Tám: Ủa? Cậu trai lạ mặt quá ha. Từ đâu tới đây vậy?",
        "Tôi: Dạ cháu từ nước ngoài qua. Nghe trên mạng nói ở đây có mấy chuyện lạ nên cháu tới xem thử.",
        "Ông Tám: Trời đất… mấy chuyện trên mạng đó hả? Người ta đồn tùm lum thôi.",
        "Ông Tám: Mà thôi, khách tới làng là quý rồi. Lại đây ngồi nghỉ chút đi.",
        "Ông Tám: Ta mới mua ổ bánh mì ngoài chợ về nè, ăn miếng cho đỡ đói.",
        "Tôi: Dạ cháu cảm ơn bác nhiều… nhưng cháu vừa ăn trên đường tới rồi ạ.",
        "Ông Tám: Ờ… vậy cũng được. Khách sáo quá trời.",
        "Ông Tám: À mà nè, thấy con chó kia không?",
        "Ông Tám: Nó tên Cậu Vàng đó. Con chó của làng này, khôn lắm.",
        "Ông Tám: Cậu thử lại gần chơi với nó coi.",
        "Ông Tám: Nó thích người lạ lắm, hay làm mấy trò vui lắm đó.",
        "Tôi: Nghe hay quá.",
        "Ông Tám: Nếu muốn tìm hiểu thêm về làng này...",
        "Ông Tám: Cậu đi lên con đường dốc bên phải kia.",
        "Ông Tám: Trưởng Làng ở trên đó.",
        "Tôi: Dạ cháu hiểu rồi.",
        "Ông Tám: Đi đường cẩn thận nghen cậu trai."
    };

    private static readonly string[] repeatLines =
    {
        "Ông Tám: Cậu cứ đi lên con dốc bên phải. Trưởng Làng ở trên đó."
    };

    public void StopVoice()
    {
        if (voiceSource != null)
            voiceSource.Stop();
    }

    /// <summary>
    /// Play voice theo thứ tự câu của Ông Tám (0..n-1) cho đoạn hội thoại hiện tại.
    /// </summary>
    public void PlayVoiceForOngTamLine(int ongTamLineIndex, bool isRepeatDialogue)
    {
        if (voiceSource == null) return;

        AudioClip[] bank = isRepeatDialogue ? ongTamRepeatVoices : ongTamVoices;
        if (bank == null || bank.Length == 0) return;
        if (ongTamLineIndex < 0 || ongTamLineIndex >= bank.Length) return;

        AudioClip clip = bank[ongTamLineIndex];
        if (clip == null) return;

        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.Play();
    }
}