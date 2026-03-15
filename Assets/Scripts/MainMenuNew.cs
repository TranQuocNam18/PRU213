using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu1 : MonoBehaviour
{
    public GameObject instructionPanel;
    public GameObject mainButtonsGroup; // Khai báo thêm biến chứa nhóm nút

    public void PlayGame()
    {
        SceneManager.LoadScene("Bao-MapScene");
    }

    public void OpenInstructions()
    {
        instructionPanel.SetActive(true);
        mainButtonsGroup.SetActive(false); // Ẩn nhóm nút menu chính đi
    }

    public void CloseInstructions()
    {
        instructionPanel.SetActive(false);
        mainButtonsGroup.SetActive(true); // Hiện nhóm nút menu chính lại
    }

    public void QuitGame()
    {
        Debug.Log("Đã thoát game!");
        Application.Quit();
    }
}
