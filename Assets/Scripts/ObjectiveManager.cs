using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    public bool objectiveStarted = false;
    public bool objectiveCompleted = false;

    public GameObject talismanUI;

    public int totalTalismans = 5;
    public int collectedTalismans = 0;

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

    // Bắt đầu nhiệm vụ thu thập bùa
    public void StartObjective()
    {
        objectiveStarted = true;

        // Hiện UI hướng dẫn thu thập bùa
        if (talismanUI != null)
        {
            talismanUI.SetActive(true);
        }

        Debug.Log("Nhiệm vụ nhặt bùa bắt đầu!");
    }

    // Xử lý khi người chơi nhặt được một lá bùa
    public void CollectTalisman()
    {
        if (!objectiveStarted) return; // Nếu chưa bắt đầu nhiệm vụ thì không đếm

        collectedTalismans++;

        Debug.Log("Collected: " + collectedTalismans);

        // Cập nhật nội dung Quest trên giao diện chính
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateQuestUI();
        }

        // Kiểm tra xem đã nhặt đủ số lượng bùa chưa
        if (collectedTalismans >= totalTalismans)
        {
            objectiveCompleted = true; // Đánh dấu hoàn thành nhiệm vụ
            
            // Chuyển sang giai đoạn quay lại gặp sư thầy
            if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.StoryState.SearchTalismans)
            {
                GameManager.Instance.AdvanceStoryState(GameManager.StoryState.ReturnMonk);
            }
            Debug.Log("Đã nhặt đủ bùa! Hãy quay lại gặp sư thầy.");
        }
        else
        {
            // Có thể thêm hiệu ứng tâm linh tại đây nếu cần
        }
    }
}