using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject gameOverPanel;
    public MadaFollowerAI madaAI;
    public GameObject[] otherUIPanels;

    public bool isGameOver = false;
    public bool isDialogue = false; // thêm trạng thái hội thoại

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
        Time.timeScale = 1f;
    }

    // ==============================
    // GAME OVER
    // ==============================

    public void GameOver()
    {
        isGameOver = true;

        // tắt UI khác
        foreach (GameObject ui in otherUIPanels)
        {
            if (ui != null)
                ui.SetActive(false);
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
        madaAI.enabled = true;
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