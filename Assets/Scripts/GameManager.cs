using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject gameOverPanel;
    public MadaFollowerAI madaAI;
    public GameObject[] otherUIPanels;

    public bool isGameOver = false;
    public bool isDialogue = false; // thêm trạng thái hội thoại

    public enum StoryState
    {
        Intro,
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
    private string[] introLines = new string[] {
        "Trò chơi bắt đầu bằng một đoạn hội thoại và màn hình máy tính/điện thoại, cho thấy nhân vật chính (Player) đang đọc các bài viết về một \"Ngôi làng du lịch sinh thái bị ma ám\".",
        "Vốn là một người duy vật, ưa khám phá và không tin vào tâm linh, anh chàng tỏ ra nghi hoặc, buông lời chế giễu những tin đồn này.",
        "Để chứng minh mọi người đang thêu dệt, anh lập tức xách balo du lịch đến ngôi làng đó."
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
                currentIntroIndex++;
                ShowIntroLine();
                introInputCooldown = 0.2f;
            }
        }
    }

    void CreateIntroUI()
    {
        introCanvasObj = new GameObject("IntroCanvas");
        Canvas canvas = introCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Top most
        introCanvasObj.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        introCanvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject introPanel = new GameObject("IntroPanel");
        introPanel.transform.SetParent(introCanvasObj.transform, false);
        UnityEngine.UI.Image bg = introPanel.AddComponent<UnityEngine.UI.Image>();
        bg.color = Color.black;
        RectTransform rt = introPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        GameObject textObj = new GameObject("IntroText");
        textObj.transform.SetParent(introPanel.transform, false);
        introTextTMP = textObj.AddComponent<TextMeshProUGUI>();
        introTextTMP.color = Color.white;
        introTextTMP.fontSize = 28;
        introTextTMP.alignment = TextAlignmentOptions.Center;
        introTextTMP.enableWordWrapping = true;

        RectTransform rtt = textObj.GetComponent<RectTransform>();
        rtt.anchorMin = new Vector2(0.1f, 0.1f);
        rtt.anchorMax = new Vector2(0.9f, 0.9f);
        rtt.sizeDelta = Vector2.zero;
        
        Time.timeScale = 0f; 
        isDialogue = true; 
        if (madaAI != null) madaAI.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ShowIntroLine()
    {
        if (currentIntroIndex < introLines.Length)
        {
            introTextTMP.text = introLines[currentIntroIndex];
        }
        else
        {
            Destroy(introCanvasObj);
            introCanvasObj = null;
            Time.timeScale = 1f;
            isDialogue = false;
            
            AdvanceStoryState(StoryState.MeetElder);
            
            if (madaAI != null) madaAI.enabled = false; // Disable Mada until NightStalking
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void AdvanceStoryState(StoryState newState)
    {
        currentState = newState;
        Debug.Log("Story State Advanced to: " + newState.ToString());

        if (newState == StoryState.NightStalking)
        {
            if (SkyManager.Instance != null)
                SkyManager.Instance.ChangeToDark();
            
            // Enable sound effects for footstep behind player or enable MadaAI stalker mode
            if (madaAI != null)
            {
                madaAI.enabled = true;
                madaAI.gameObject.SetActive(true);
            }
        }
    }

    // ==============================
    // GAME OVER
    // ==============================

    public void GameOver()
    {
        isGameOver = true;

    // Tắt các UI trong mảng otherUIPanels để chắc chắn (nếu có)
    foreach (GameObject ui in otherUIPanels)
    {
        if (ui != null)
            ui.SetActive(false);
    }
        
    // Tắt tất cả các Canvas khác trong Scene không chứa gameOverPanel
    Canvas[] allCanvases = FindObjectsOfType<Canvas>();
    foreach (Canvas c in allCanvases)
    {
        if (c.gameObject != gameOverPanel && !gameOverPanel.transform.IsChildOf(c.transform))
        {
            c.gameObject.SetActive(false);
        }
    }

    // Tắt tất cả các UI con trong Canvas chứa gameOverPanel, ngoại trừ nhánh chứa gameOverPanel
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