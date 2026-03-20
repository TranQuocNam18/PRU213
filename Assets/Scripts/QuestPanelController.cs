using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuestPanelController : MonoBehaviour
{
    public static QuestPanelController Instance;

    private GameObject panelObj;
    private RectTransform panelRect;
    private TextMeshProUGUI questTextTMP;

    private bool isVisible = false;
    private Coroutine slideCoroutine;

    // Kích thước của Panel báo nhiệm vụ
    private float panelWidth = 350f;
    private float panelHeight = 120f;

    private float slideDuration = 0.5f;

    private string currentQuestText = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            CreateUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void CreateUI()
    {
        TMP_FontAsset defaultFont = null;
        if (TalismanUI.Instance != null && TalismanUI.Instance.talismanText != null)
        {
            defaultFont = TalismanUI.Instance.talismanText.font;
        }

        // Tạo Canvas Overlay nếu script được gắn vào 1 object độc lập
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            gameObject.AddComponent<RectTransform>();
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Đặt lên cao để hiện trên UI khác

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            gameObject.AddComponent<GraphicRaycaster>();
        }

        // Tạo Panel bên phải
        panelObj = new GameObject("QuestSlidePanel");
        panelObj.transform.SetParent(this.transform, false);

        Image bgImage = panelObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Đen mờ

        panelRect = panelObj.GetComponent<RectTransform>();

        // Anchor bên phải giữa
        panelRect.anchorMin = new Vector2(1, 0.5f);
        panelRect.anchorMax = new Vector2(1, 0.5f);
        panelRect.pivot = new Vector2(1, 0.5f); // Pivot bên phải

        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

        // Ẩn panel lúc đầu (bằng cách trượt ra khỏi màn hình)
        panelRect.anchoredPosition = new Vector2(panelWidth + 10, 0); // Vị trí giấu đi

        // Tiêu đề: "Nhiệm vụ:"
        GameObject titleObj = new GameObject("QuestTitle");
        titleObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) titleTMP.font = defaultFont;
        titleTMP.text = "[ Nhiệm Vụ ]";
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = new Color(1f, 0.8f, 0.2f); // Vàng
        titleTMP.fontSize = 24;
        titleTMP.alignment = TextAlignmentOptions.TopLeft;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0, 1);
        titleRect.offsetMin = new Vector2(20, -40);
        titleRect.offsetMax = new Vector2(-20, -10);

        // Nội dung Quest
        GameObject textObj = new GameObject("QuestText");
        textObj.transform.SetParent(panelObj.transform, false);
        questTextTMP = textObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) questTextTMP.font = defaultFont;
        questTextTMP.text = "Chưa có nhiệm vụ";
        questTextTMP.color = Color.white;
        questTextTMP.fontSize = 20;
        questTextTMP.alignment = TextAlignmentOptions.TopLeft;
        questTextTMP.enableWordWrapping = true;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0, 1);
        textRect.offsetMin = new Vector2(20, 10);
        textRect.offsetMax = new Vector2(-20, -50);
    }

    public void ShowQuest(string questText)
    {
        if (questText == currentQuestText && isVisible) return; // Trùng thì không cần animate lại (trừ khi cập nhật số bùa)

        if (questText == "")
        {
            HideQuestPanel();
            return;
        }

        currentQuestText = questText;
        questTextTMP.text = questText;

        if (slideCoroutine != null) StopCoroutine(slideCoroutine);

        // Luôn trượt ra một chút rồi hiện lại để gây chú ý nếu chỉ update text
        if (isVisible)
        {
            slideCoroutine = StartCoroutine(BounceInAnimation());
        }
        else
        {
            slideCoroutine = StartCoroutine(SlideAnimation(new Vector2(0, 0))); // Slide in to X=0
        }
    }

    public void HideQuestPanel()
    {
        if (!isVisible) return;

        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideAnimation(new Vector2(panelWidth, 0))); // Slide out
    }

    private IEnumerator SlideAnimation(Vector2 targetPos)
    {
        Vector2 startPos = panelRect.anchoredPosition;
        float elapsed = 0;

        while (elapsed < slideDuration)
        {
            panelRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsed / slideDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        panelRect.anchoredPosition = targetPos;
        isVisible = (targetPos.x == 0);
    }

    private IEnumerator BounceInAnimation()
    {
        // Kéo vô 1 chút
        Vector2 startPos = panelRect.anchoredPosition;
        Vector2 pulledPos = new Vector2(50f, 0f);

        float elapsed = 0;
        while (elapsed < 0.15f)
        {
            panelRect.anchoredPosition = Vector2.Lerp(startPos, pulledPos, elapsed / 0.15f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Đẩy lại
        elapsed = 0;
        while (elapsed < 0.2f)
        {
            panelRect.anchoredPosition = Vector2.Lerp(pulledPos, Vector2.zero, elapsed / 0.2f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        panelRect.anchoredPosition = Vector2.zero;
        isVisible = true;
    }
}