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

    public void StartObjective()
    {
        objectiveStarted = true;

        if (talismanUI != null)
        {
            talismanUI.SetActive(true);
        }

        Debug.Log("Nhiệm vụ nhặt bùa bắt đầu!");
    }

    public void CollectTalisman()
    {
        if (!objectiveStarted) return;

        collectedTalismans++;

        Debug.Log("Collected: " + collectedTalismans);

        if (collectedTalismans >= totalTalismans)
        {
            objectiveCompleted = true;
            Debug.Log("Đã nhặt đủ bùa! Hãy quay lại gặp sư thầy.");
        }
    }
}