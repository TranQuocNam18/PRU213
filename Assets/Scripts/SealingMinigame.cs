using UnityEngine;
using System.Collections.Generic;

public class SealingMinigame : MonoBehaviour
{
    public static SealingMinigame Instance;

    public bool isPlaying = false;
    public float timeLimitPerSeal = 8f;
    private float currentTime = 0f;
    
    private bool won = false;
    private bool isCutscenePlaying = false; // Add cutscene flag

    // Audio
    public AudioClip stageClearSound;
    public AudioClip winCutsceneSound;
    private AudioSource audioSrc;
    
    // Stages
    private List<int> sequence = new List<int>();
    private int currentStageIndex = 0;

    // Drawing variables
    private List<Vector2> targetPoints = new List<Vector2>();
    private int currentPointIndex = 0;
    private float pointRadius = 30f;
    private List<Vector2> playerPath = new List<Vector2>();

    private Texture2D texWhite;
    private Texture2D texRed;
    private Texture2D texGreen;
    private Texture2D texYellow;
    private Texture2D texGray;
    private Texture2D texCyan;
    
    // Circle texture for smooth brush
    private Texture2D brushTex;

    public GameObject customWinUI; // Tham chiếu đến Custom Win UI

    private string currentElement = "";

    void Awake()
    {
        if (Instance == null) Instance = this;

        // Thêm component AudioSource nếu chưa có
        audioSrc = gameObject.AddComponent<AudioSource>();

        // Tạo sẵn các texture làm màu vẽ
        texWhite = MakeTex(2, 2, Color.white);
        texRed = MakeTex(2, 2, Color.red);
        texGreen = MakeTex(2, 2, Color.green);
        texYellow = MakeTex(2, 2, Color.yellow);
        texGray = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f));
        texCyan = MakeTex(2, 2, Color.cyan);
        
        // Tạo brush hình tròn thay vì hình vuông để nét mượt hơn
        brushTex = MakeCircleTex(8, Color.cyan); 
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
    
    private Texture2D MakeCircleTex(int radius, Color col)
    {
        int size = radius * 2;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - radius;
                float dy = y - radius;
                if (dx * dx + dy * dy <= radius * radius)
                    colors[y * size + x] = col;
                else
                    colors[y * size + x] = Color.clear;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    public void StartMinigame()
    {
        isPlaying = true;
        won = false;

        if (GameManager.Instance != null && GameManager.Instance.otherUIPanels != null)
        {
            foreach (GameObject ui in GameManager.Instance.otherUIPanels)
            {
                if (ui != null) ui.SetActive(false);
            }
        }
        
        // Tạo chuỗi 5 chữ ngẫu nhiên (0: Kim, 1: Mộc, 2: Thủy, 3: Hỏa, 4: Thổ)
        sequence.Clear();
        for (int i = 0; i < 5; i++) sequence.Add(i);
        
        // Shuffle
        for (int i = 0; i < sequence.Count; i++)
        {
            int temp = sequence[i];
            int rnd = Random.Range(i, sequence.Count);
            sequence[i] = sequence[rnd];
            sequence[rnd] = temp;
        }

        currentStageIndex = 0;
        LoadNextStage();

        Time.timeScale = 0f;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LoadNextStage()
    {
        if (currentStageIndex >= sequence.Count)
        {
            WinGame();
            return;
        }

        currentTime = timeLimitPerSeal;
        currentPointIndex = 0;
        playerPath.Clear();
        
        GenerateShape(sequence[currentStageIndex]);
    }

    private void GenerateShape(int elementID)
    {
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f + 50f;
        float s = Mathf.Min(Screen.width, Screen.height) * 0.35f;

        targetPoints.Clear();

        switch (elementID)
        {
            case 0: // KIM 金
                currentElement = "KIM (金)";
                // Phác hoạ chữ Kim (mái nhà trên, ngang giữa, sổ dọc, chấm trái, chấm phải, gạch dưới)
                targetPoints.Add(new Vector2(cx, cy - s)); // Đỉnh mái
                targetPoints.Add(new Vector2(cx - s*0.6f, cy - s*0.3f)); // Mái trái
                targetPoints.Add(new Vector2(cx + s*0.6f, cy - s*0.3f)); // Mái phải
                targetPoints.Add(new Vector2(cx, cy - s*0.3f)); // Quay vào giữa mái
                targetPoints.Add(new Vector2(cx, cy + s*0.6f)); // Sổ dọc
                targetPoints.Add(new Vector2(cx - s*0.4f, cy + s*0.2f)); // Nét ngang trên
                targetPoints.Add(new Vector2(cx + s*0.4f, cy + s*0.2f)); // Nét ngang trên
                targetPoints.Add(new Vector2(cx - s*0.7f, cy + s*0.8f)); // Nét ngang dưới cùng
                targetPoints.Add(new Vector2(cx + s*0.7f, cy + s*0.8f)); // Nét ngang dưới cùng
                break;

            case 1: // MỘC 木
                currentElement = "MỘC (木)";
                // Gạch ngang, sổ thẳng, phẩy trái, mác phải
                targetPoints.Add(new Vector2(cx - s*0.7f, cy - s*0.3f)); // Ngang trái
                targetPoints.Add(new Vector2(cx + s*0.7f, cy - s*0.3f)); // Ngang phải
                targetPoints.Add(new Vector2(cx, cy - s*0.3f)); // Quay vào giữa
                targetPoints.Add(new Vector2(cx, cy - s*0.8f)); // Đỉnh sổ
                targetPoints.Add(new Vector2(cx, cy + s*0.8f)); // Đáy sổ
                targetPoints.Add(new Vector2(cx, cy - s*0.1f)); // Tâm
                targetPoints.Add(new Vector2(cx - s*0.6f, cy + s*0.6f)); // Phẩy trái
                targetPoints.Add(new Vector2(cx, cy - s*0.1f)); // Về Tâm
                targetPoints.Add(new Vector2(cx + s*0.6f, cy + s*0.6f)); // Mác phải
                break;

            case 2: // THỦY 水
                currentElement = "THỦY (水)";
                // Sổ móc giữa, phẩy trái trên, phẩy trái dưới, mác phải trên, mác phải dưới
                targetPoints.Add(new Vector2(cx, cy - s*0.8f)); // Đỉnh dọc
                targetPoints.Add(new Vector2(cx, cy + s*0.8f)); // Đáy dọc
                targetPoints.Add(new Vector2(cx - s*0.1f, cy + s*0.7f)); // Móc nhẹ
                targetPoints.Add(new Vector2(cx, cy)); // Tâm
                targetPoints.Add(new Vector2(cx - s*0.6f, cy - s*0.3f)); // Chấp trái
                targetPoints.Add(new Vector2(cx - s*0.4f, cy + s*0.6f)); // Phẩy trái
                targetPoints.Add(new Vector2(cx, cy)); // Về Tâm
                targetPoints.Add(new Vector2(cx + s*0.6f, cy - s*0.3f)); // Phẩy phải
                targetPoints.Add(new Vector2(cx + s*0.5f, cy + s*0.6f)); // Mác phải
                break;

            case 3: // HỎA 火
                currentElement = "HỎA (火)";
                // Chấm trái, dấu phẩy trái, dấu mác phải giữa, vệt dài phải
                targetPoints.Add(new Vector2(cx, cy - s*0.6f)); // Tâm đỉnh
                targetPoints.Add(new Vector2(cx - s*0.5f, cy + s*0.7f)); // Phẩy dài trái
                targetPoints.Add(new Vector2(cx, cy - s*0.2f)); // Lùi về giữa
                targetPoints.Add(new Vector2(cx + s*0.5f, cy + s*0.7f)); // Mác dài phải
                targetPoints.Add(new Vector2(cx - s*0.5f, cy - s*0.1f)); // Chấm rời trái
                targetPoints.Add(new Vector2(cx, cy - s*0.2f)); // Về giữa
                targetPoints.Add(new Vector2(cx + s*0.5f, cy - s*0.1f)); // Chấm rời phải
                break;

            case 4: // THỔ 土
                currentElement = "THỔ (土)";
                // Ngang ngắn, sổ dọc, ngang dài
                targetPoints.Add(new Vector2(cx - s*0.4f, cy - s*0.2f)); // Ngang ngắn trái
                targetPoints.Add(new Vector2(cx + s*0.4f, cy - s*0.2f)); // Ngang ngắn phải
                targetPoints.Add(new Vector2(cx, cy - s*0.2f)); // Về giữa đoạn ngắn
                targetPoints.Add(new Vector2(cx, cy - s*0.8f)); // Lên đỉnh sổ
                targetPoints.Add(new Vector2(cx, cy + s*0.6f)); // Chạy dọc xuống
                targetPoints.Add(new Vector2(cx - s*0.7f, cy + s*0.6f)); // Kéo ra trái ngang dài
                targetPoints.Add(new Vector2(cx + s*0.7f, cy + s*0.6f)); // Kéo hết ngang dài phải
                break;
        }
    }

    void Update()
    {
        if (!isPlaying) return;

        if (currentTime > 0)
        {
            currentTime -= Time.unscaledDeltaTime;

            Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

            if (Input.GetMouseButtonDown(0))
            {
                playerPath.Clear();
                currentPointIndex = 0;
                playerPath.Add(mousePos);
            }

            if (Input.GetMouseButton(0))
            {
                // Thêm nhiều mốc (interpolation) giữa frame cũ và mới để tạo cảm giác "liền mạch", stroke tròn trịa
                if (playerPath.Count > 0)
                {
                    Vector2 lastPos = playerPath[playerPath.Count - 1];
                    float dist = Vector2.Distance(lastPos, mousePos);
                    if (dist > 2f)
                    {
                        // Thêm điểm trung gian để nét cọ lấp đầy các khoảng trống
                        int steps = Mathf.CeilToInt(dist / 2f);
                        for (int i = 1; i <= steps; i++)
                        {
                            Vector2 interpolated = Vector2.Lerp(lastPos, mousePos, (float)i / steps);
                            playerPath.Add(interpolated);
                        }
                    }
                }
                else
                {
                    playerPath.Add(mousePos);
                }

                if (currentPointIndex < targetPoints.Count)
                {
                    if (Vector2.Distance(mousePos, targetPoints[currentPointIndex]) <= pointRadius)
                    {
                        currentPointIndex++;

                        if (currentPointIndex >= targetPoints.Count)
                        {
                            if (stageClearSound != null)
                                audioSrc.PlayOneShot(stageClearSound);

                            currentStageIndex++;
                            LoadNextStage();
                        }
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (currentPointIndex < targetPoints.Count)
                {
                    currentPointIndex = 0;
                    playerPath.Clear();
                }
            }
        }
        else
        {
            LoseGame();
        }
    }

    void WinGame()
    {
        isPlaying = false;
        isCutscenePlaying = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Phát âm thanh win
        if (winCutsceneSound != null)
            audioSrc.PlayOneShot(winCutsceneSound);

        StartCoroutine(WinCutsceneCoroutine());
    }

    System.Collections.IEnumerator WinCutsceneCoroutine()
    {
        // Phục hồi thời gian để chạy cutscene
        Time.timeScale = 1f;
        
        // Chuyển trời sáng
        if (SkyManager.Instance != null)
            SkyManager.Instance.ChangeToBright();

        // Tìm Mada và tạo hiệu ứng tiêu biến
        JumpscareController jumpscare = FindObjectOfType<JumpscareController>();
        GameObject mada = null;
        if (jumpscare != null) mada = jumpscare.mada;
        
        if (mada != null)
        {
            // Tạm thời Disable AI để nó không đuổi nữa
            MadaFollowerAI madaAI = mada.GetComponent<MadaFollowerAI>();
            if (madaAI != null) madaAI.enabled = false;

            // Đổi màu thành đỏ/cam như bị thiêu đốt hoặc thêm ParticleSystem
            Renderer[] renderers = mada.GetComponentsInChildren<Renderer>();

            float burnTime = 3f;
            float t = 0;
            while (t < burnTime)
            {
                t += Time.deltaTime;
                foreach (Renderer r in renderers)
                {
                    if (r.material.HasProperty("_Color"))
                    {
                        Color c = r.material.color;
                        r.material.color = Color.Lerp(c, Color.red, Time.deltaTime * 2f);
                    }
                }
                mada.transform.localScale = Vector3.Lerp(mada.transform.localScale, Vector3.zero, Time.deltaTime);
                yield return null;
            }

            Destroy(mada);
        }
        else
        {
            yield return new WaitForSeconds(3f);
        }

        // Hiện bảng vàng chiến thắng
        if (customWinUI != null)
        {
            customWinUI.SetActive(true);
        }
        else
        {
            won = true;
        }
        
        isCutscenePlaying = false;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    void LoseGame()
    {
        isPlaying = false;
        Time.timeScale = 1f;

        JumpscareController jumpscare = FindObjectOfType<JumpscareController>();
        if (jumpscare != null)
        {
            jumpscare.StartJumpscare();
        }
        else
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }
    }

    void OnGUI()
    {
        // Đang xem cutscene thiêu đốt thì ko vẽ UI QTE hay Win Panel
        if (isCutscenePlaying) return;

        if (isPlaying)
        {
            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.fontSize = 26;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.normal.textColor = Color.red;

            GUI.Label(new Rect(0, 30, Screen.width, 40), $"GIẢI ẤN BƯỚC {currentStageIndex+1}/5: TẠO LINH PHÙ {currentElement}", textStyle);

            GUIStyle guideStyle = new GUIStyle(textStyle);
            guideStyle.fontSize = 18;
            guideStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(0, 70, Screen.width, 30), "Giữ chuột và kéo liền mạch nối các số thứ tự để vẽ!", guideStyle);
            
            GUI.Label(new Rect(0, 110, Screen.width, 30), "Thời gian vòng này: " + currentTime.ToString("F1") + "s", textStyle);

            for (int i = 0; i < targetPoints.Count; i++)
            {
                Rect r = new Rect(targetPoints[i].x - 15, targetPoints[i].y - 15, 30, 30);

                bool overlapsAnother = false;
                
                // Nếu điểm i ở tương lai mà bị trùng vị trí với bất kỳ điểm j nào trước nó, ta sẽ tạm ẩn số i
                // để chúng không hiển thị đè nhau (ví dụ 1 và 6 trùng nhau thì chỉ hiện 1 cho tới khi vẽ xong 1 tới 5)
                if (i > currentPointIndex)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (Vector2.Distance(targetPoints[i], targetPoints[j]) < 5f)
                        {
                            overlapsAnother = true;
                            break;
                        }
                    }
                }
                
                if (overlapsAnother) continue; 

                if (i < currentPointIndex)
                    GUI.DrawTexture(r, texGreen); 
                else if (i == currentPointIndex)
                    GUI.DrawTexture(r, texYellow); 
                else
                    GUI.DrawTexture(r, texGray); 

                GUIStyle numStyle = new GUIStyle(textStyle);
                numStyle.fontSize = 18;
                numStyle.normal.textColor = Color.black;
                GUI.Label(r, (i + 1).ToString(), numStyle);
            }

            // Vẽ path chuột (dùng texture tròn để nét vẽ nhìn mềm mại liền khối)
            if (playerPath.Count > 0)
            {
                GUI.color = Color.white;
                int brushSize = 16;
                foreach (Vector2 p in playerPath)
                {
                    GUI.DrawTexture(new Rect(p.x - brushSize/2f, p.y - brushSize/2f, brushSize, brushSize), brushTex);
                }
                GUI.color = Color.white; // reset lại color
            }
        }
        else if (won)
        {
            GUI.Box(new Rect(Screen.width / 2 - 250, Screen.height / 2 - 80, 500, 160), "");
            GUIStyle winStyle = new GUIStyle(GUI.skin.label);
            winStyle.alignment = TextAnchor.MiddleCenter;
            winStyle.fontSize = 28;
            winStyle.fontStyle = FontStyle.Bold;
            winStyle.normal.textColor = Color.yellow;
            
            GUI.Label(new Rect(Screen.width / 2 - 250, Screen.height / 2 - 50, 500, 60), "PHONG ẤN THÀNH CÔNG!\nBẠN ĐÃ CHIẾN THẮNG", winStyle);

            if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height / 2 + 40, 160, 40), "Chơi lại từ đầu"))
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.ResetGame();
            }
        }
    }
}
