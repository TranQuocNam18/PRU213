using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;
    public bool objectiveStarted = false;
    public GameObject talismanUI;

    public int totalTalismans = 5;
    public int collectedTalismans = 0;

    public RitualDoor ritual_Door;

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
        collectedTalismans++;

        Debug.Log("Collected: " + collectedTalismans);

        if (collectedTalismans >= totalTalismans)
        {
            UnlockRitual();
        }
    }

    void UnlockRitual()
    {
        ritual_Door.OpenDoor();
    }
}