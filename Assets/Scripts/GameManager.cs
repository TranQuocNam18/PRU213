using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public AudioSource voiceSource;
    public AudioClip[] introVoices;
    [Header("Player Voices")]
    public AudioClip[] playerVoices; // voice cho độc thoại của "Tôi" khi không có NPC
    Coroutine voiceCoroutine;
    public float textSpeedMultiplier = 0.5f;
    public static GameManager Instance;

    public GameObject gameOverPanel;
    public MadaFollowerAI madaAI;
    public GameObject[] otherUIPanels;

    [Header("Quest UI")]
    // Will dynamically find TalismanUI to display quests

    public bool isGameOver = false;
    public bool isDialogue = false;

    public enum StoryState
    {
        Intro,
        MeetOngTam,
        MeetElder,
        MeetMonk,
        NightStalking,
        SearchTalismans,
        ReturnMonk,
        Minigame,
        Win
    }
    public StoryState currentState = StoryState.Intro;

    private GameObject introCanvasObj;
    private TextMeshProUGUI introTextTMP;
    private int currentIntroIndex = 0;
    private float introInputCooldown = 0.5f;
    bool isTyping = false;
    Coroutine typingCoroutine;
    private string[] introLines = new string[] {
        "Một buổi tối bình thường... tôi đang lướt điện thoại và đọc được hàng loạt bài viết về một \"ngôi làng du lịch sinh thái bị ma ám\".",
        "Những tin đồn nói rằng những hồ nước trong làng bị ám bởi Ma Da – những vong hồn chết đuối luôn tìm người thế mạng.",
        "Là một người thích khám phá và không tin tâm linh, tôi quyết định tự mình đến đó xem thử.",
        "Tôi sẽ là người chứng minh tất cả chỉ là tin đồn."
    };

    [Header("In-Game Monologue")]
    private string[] inGameLines = new string[] {
        "Tôi: Cuối cùng mình cũng đến được nơi này… Đây chính là ngôi làng mà mình đã tìm kiếm."
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
        CreateIntroUI();
        ShowIntroLine();
        UpdateQuestUI();

        // tắt intro tiếng thở
        PlayerAudioManager audioManager = FindObjectOfType<PlayerAudioManager>();
        audioManager.enabled = false;

        // Tự động thêm GuideWispSpawner
        if (GuideWispSpawner.Instance == null)
        {
            GameObject spawnerObj = new GameObject("GuideWispSpawner");
            spawnerObj.AddComponent<GuideWispSpawner>();
        }
    }

    void Update()
    {
        if (introCanvasObj != null)
        {
            if (introInputCooldown > 0)
            {
                introInputCooldown -= Time.unscaledDeltaTime;
                return;
            }

            if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
            {
                // Nếu chữ đang chạy → hiện hết
                if (isTyping)
                {
                    StopCoroutine(typingCoroutine);
                    isTyping = false;
                    introTextTMP.text = introLines[currentIntroIndex];
                    return;
                }

                // Nếu chữ đã hiện xong → qua đoạn tiếp
                currentIntroIndex++;
                ShowIntroLine();
                introInputCooldown = 0.2f;
            }
        }
    }

    public void UpdateQuestUI()
    {
        string questString = "";

        switch (currentState)
        {
            case StoryState.Intro:
                questString = "";
                break;
            case StoryState.MeetOngTam:
                questString = "Nói chuyện với ông Tám";
                break;
            case StoryState.MeetElder:
                questString = "Nói chuyện với trưởng làng";
                break;
            case StoryState.MeetMonk:
                questString = "Đi đến Nhà sư";
                break;
            case StoryState.NightStalking:
                questString = "Đi về làng nói chuyện trưởng làng";
                break;
            case StoryState.SearchTalismans:
                int count = 0;
                int total = 5;
                if (ObjectiveManager.Instance != null)
                {
                    count = ObjectiveManager.Instance.collectedTalismans;
                    total = ObjectiveManager.Instance.totalTalismans;
                }
                questString = $"Tìm {total} lá bùa ({count}/{total})";
                break;
            case StoryState.ReturnMonk:
                questString = "Quay lại gặp Nhà sư";
                break;
            case StoryState.Minigame:
            case StoryState.Win:
                questString = "";
                break;
        }

        // Cập nhật TalismanUI cũ (nếu có)
        TalismanUI ui = TalismanUI.Instance;
        if (ui != null && ui.talismanText != null)
        {
            ui.talismanText.text = questString;
            if (questString != "")
            {
                ui.gameObject.SetActive(true);
                Transform parent = ui.transform.parent;
                while (parent != null)
                {
                    parent.gameObject.SetActive(true);
                    parent = parent.parent;
                }
            }
            else
            {
                ui.gameObject.SetActive(false);
            }
        }

        // Cập nhật lên UI Trượt (QuestPanelController)
        if (QuestPanelController.Instance == null)
        {
            GameObject questPanelObj = new GameObject("QuestPanelController");
            questPanelObj.AddComponent<QuestPanelController>();
        }

        if (QuestPanelController.Instance != null)
        {
            QuestPanelController.Instance.ShowQuest(questString);
        }
    }

    void CreateIntroUI()
    {
        introCanvasObj = new GameObject("IntroCanvas");
        Canvas canvas = introCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        UnityEngine.UI.CanvasScaler scaler = introCanvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        introCanvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // FULL BLACK BACKGROUND
        GameObject introPanel = new GameObject("IntroPanel");
        introPanel.transform.SetParent(introCanvasObj.transform, false);

        UnityEngine.UI.Image bg = introPanel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0, 0, 0, 1f);

        RectTransform rt = introPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // TEXT PANEL (CENTER BOX)
        GameObject textPanel = new GameObject("TextPanel");
        textPanel.transform.SetParent(introPanel.transform, false);

        UnityEngine.UI.Image panelImg = textPanel.AddComponent<UnityEngine.UI.Image>();
        panelImg.color = new Color(0, 0, 0, 0f);

        RectTransform panelRT = textPanel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.25f, 0.4f);
        panelRT.anchorMax = new Vector2(0.75f, 0.6f);
        panelRT.sizeDelta = Vector2.zero;

        // STORY TEXT
        GameObject textObj = new GameObject("IntroText");
        textObj.transform.SetParent(textPanel.transform, false);

        introTextTMP = textObj.AddComponent<TextMeshProUGUI>();
        introTextTMP.color = Color.white;
        introTextTMP.fontSize = 52;
        introTextTMP.lineSpacing = 10;
        introTextTMP.margin = new Vector4(80, 60, 80, 60);
        introTextTMP.characterSpacing = 2;
        introTextTMP.alignment = TextAlignmentOptions.Center;
        introTextTMP.enableWordWrapping = true;
        introTextTMP.outlineWidth = 0.2f;
        introTextTMP.outlineColor = Color.black;

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0f, 0.25f);
        textRT.anchorMax = new Vector2(1f, 1f);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        // CLICK TO CONTINUE TEXT
        GameObject hint = new GameObject("HintText");
        hint.transform.SetParent(introPanel.transform, false);

        TextMeshProUGUI hintText = hint.AddComponent<TextMeshProUGUI>();
        hintText.text = "Nhấn chuột để tiếp tục...";
        hintText.fontSize = 26;
        hintText.fontStyle = FontStyles.Italic;
        hintText.alignment = TextAlignmentOptions.BottomRight;
        hintText.color = new Color(1, 1, 1, 0.7f);
        StartCoroutine(BlinkHint(hintText));

        RectTransform hintRT = hint.GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0.7f, 0.05f);
        hintRT.anchorMax = new Vector2(0.98f, 0.12f);
        hintRT.offsetMin = Vector2.zero;
        hintRT.offsetMax = Vector2.zero;

        Time.timeScale = 0f;
        isDialogue = true;

        if (madaAI != null)
            madaAI.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator BlinkHint(TextMeshProUGUI hint)
    {
        while (true)
        {
            hint.alpha = 0.2f;
            yield return new WaitForSecondsRealtime(0.5f);
            hint.alpha = 0.8f;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    void ShowIntroLine()
    {
        if (currentIntroIndex < introLines.Length)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            if (voiceCoroutine != null)
                StopCoroutine(voiceCoroutine);

            if (voiceSource != null && voiceSource.isPlaying)
                voiceSource.Stop();

            typingCoroutine = StartCoroutine(TypeLine(introLines[currentIntroIndex]));
            voiceCoroutine = StartCoroutine(PlayVoiceDelayed(0.2f));
        }
        else
        {
            voiceSource.Stop();

            Destroy(introCanvasObj);
            introCanvasObj = null;

            Time.timeScale = 1f;
            isDialogue = false;

            PlayerAudioManager audioManager = FindObjectOfType<PlayerAudioManager>();
            audioManager.enabled = true;

            // Bắt đầu từ Ông Tám trước khi gặp Trưởng Làng
            AdvanceStoryState(StoryState.MeetOngTam);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            StartCoroutine(StartInGameMonologue());
        }
    }

    IEnumerator PlayVoiceDelayed(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (voiceSource != null && introVoices.Length > currentIntroIndex)
        {
            voiceSource.Stop();
            voiceSource.clip = introVoices[currentIntroIndex];
            voiceSource.Play();
        }
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        introTextTMP.text = "";

        float delay = 0.035f * textSpeedMultiplier;

        foreach (char c in line)
        {
            introTextTMP.text += c;

            if (c == '.' || c == ',' || c == '!')
                yield return new WaitForSecondsRealtime(delay * 6);
            else
                yield return new WaitForSecondsRealtime(delay);
        }

        isTyping = false;
    }

    public void AdvanceStoryState(StoryState newState)
    {
        currentState = newState;
        Debug.Log("Story State Advanced to: " + newState.ToString());

        UpdateQuestUI();

        if (TalismanCompass.Instance != null)
        {
            TalismanCompass.Instance.UpdateWaypoints();
        }

        if (newState == StoryState.NightStalking)
        {
            if (SkyManager.Instance != null)
                SkyManager.Instance.ChangeToDark();

            if (madaAI != null)
            {
                madaAI.enabled = true;
                madaAI.gameObject.SetActive(true);
            }
        }
        else if (newState == StoryState.SearchTalismans)
        {
            if (SkyManager.Instance != null)
                SkyManager.Instance.EnableFog();
        }
    }

    IEnumerator StartInGameMonologue()
    {
        yield return new WaitForSeconds(0.5f);

        if (DialogueManager.Instance != null)
        {
            // ép về độc thoại để DialogueManager dùng playerVoices[]
            DialogueManager.Instance.currentElder = null;
            DialogueManager.Instance.currentMonk = null;
            DialogueManager.Instance.currentOngTam = null;

            DialogueManager.Instance.StartDialogue(inGameLines);
        }
    }

    // ==============================
    // GAME OVER
    // ==============================

    public void GameOver()
    {
        isGameOver = true;

        foreach (GameObject ui in otherUIPanels)
        {
            if (ui != null)
                ui.SetActive(false);
        }

        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.gameObject != gameOverPanel && !gameOverPanel.transform.IsChildOf(c.transform))
            {
                c.gameObject.SetActive(false);
            }
        }

        Canvas mainCanvas = gameOverPanel.GetComponentInParent<Canvas>();
        if (mainCanvas != null)
        {
            foreach (Transform child in mainCanvas.transform)
            {
                if (child.gameObject != gameOverPanel && !gameOverPanel.transform.IsChildOf(child))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ==============================
    // DIALOGUE SYSTEM
    // ==============================

    public void StartDialogue()
    {
        isDialogue = true;
        Time.timeScale = 0f;

        if (madaAI != null)
            madaAI.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void EndDialogue()
    {
        isDialogue = false;

        if (currentState == StoryState.Minigame)
        {
            SealingMinigame minigame = FindObjectOfType<SealingMinigame>();
            if (minigame == null)
            {
                GameObject go = new GameObject("SealingMinigame");
                minigame = go.AddComponent<SealingMinigame>();
            }
            minigame.StartMinigame();
            return;
        }

        Time.timeScale = 1f;
        if (madaAI != null) madaAI.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ==============================
    // RESET GAME
    // ==============================

    public void ResetGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ==============================
    // RESUME GAME
    // ==============================

    public void ResumeGame()
    {
        gameOverPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
