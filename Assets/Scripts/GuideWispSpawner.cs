using UnityEngine;
using System.Collections.Generic;

public class GuideWispSpawner : MonoBehaviour
{
    public static GuideWispSpawner Instance;

    public float spawnInterval = 5f;
    private float spawnTimer = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // Chỉ spawn wisp nếu có mục tiêu để di chuyển tới
        bool hasObjective = (GameManager.Instance.currentState >= GameManager.StoryState.Intro &&
                             GameManager.Instance.currentState < GameManager.StoryState.Minigame);

        if (!hasObjective) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnWisp();
        }
    }

    void SpawnWisp()
    {
        Transform target = GetNearestObjective();
        if (target == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 spawnPos = transform.position;
        if (player != null)
        {
            spawnPos = player.transform.position;
            spawnPos += player.transform.forward * 0.5f + Vector3.up * 1f;
        }

        // Code setup the wisp
        GameObject wispObj = new GameObject("GuideWisp");
        wispObj.transform.position = spawnPos;

        GuideWisp wispScript = wispObj.AddComponent<GuideWisp>();
        wispScript.SetTarget(target);
    }

    Transform GetNearestObjective()
    {
        // Lấy mục tiêu từ state hiện tại
        var state = GameManager.Instance.currentState;
        if (state == GameManager.StoryState.MeetOngTam)
        {
            OngTamNPC ongTam = FindObjectOfType<OngTamNPC>();
            if (ongTam != null) return ongTam.transform;
        }
        else if (state == GameManager.StoryState.MeetElder || state == GameManager.StoryState.NightStalking)
        {
            ElderNPC elder = FindObjectOfType<ElderNPC>();
            if (elder != null) return elder.transform;
        }
        else if (state == GameManager.StoryState.MeetMonk || state == GameManager.StoryState.ReturnMonk)
        {
            MonkNPC monk = FindObjectOfType<MonkNPC>();
            if (monk != null) return monk.transform;
        }
        else if (state == GameManager.StoryState.SearchTalismans)
        {
            Talisman[] allTalismans = FindObjectsByType<Talisman>(FindObjectsSortMode.None);
            Transform nearest = null;
            float minDist = float.MaxValue;
            foreach (Talisman t in allTalismans)
            {
                float dist = Vector3.Distance(transform.position, t.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = t.transform;
                }
            }
            return nearest;
        }

        return null;
    }
}