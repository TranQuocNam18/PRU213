using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Hướng Dẫn")]
    public GameObject tutorialCanvas; // Kéo Canvas hoặc Panel chứa hướng dẫn vào đây

    private bool isTutorialActive = false;

    void Start()
    {
        // Nếu có UI hướng dẫn, bật nó lên lúc mới vào game và dừng game
        if (tutorialCanvas != null)
        {
            tutorialCanvas.SetActive(true);
            isTutorialActive = true;
            Time.timeScale = 0f; // Dừng thời gian
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Update()
    {
        if (isTutorialActive)
        {
            // Nhấn Space hoặc Click để thoát hướng dẫn
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                CloseTutorial();
            }
        }
    }

    public void CloseTutorial()
    {
        if (tutorialCanvas != null)
        {
            tutorialCanvas.SetActive(false);
        }

        isTutorialActive = false;
        Time.timeScale = 1f; // Phục hồi thời gian

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
