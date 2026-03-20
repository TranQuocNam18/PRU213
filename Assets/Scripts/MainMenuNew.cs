using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu1 : MonoBehaviour
{
    public GameObject instructionPanel;
    public GameObject mainButtonsGroup;

    [Header("Audio")]
    public MenuClickSound click;         // kéo MenuAudio (component MenuClickSound) vào đây
    public float clickDelay = 0.08f;      // chỉnh 0.05 - 0.12 tuỳ bạn

    Coroutine running;

    void RunAfterClick(System.Action action)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(CoRunAfterClick(action));
    }

    IEnumerator CoRunAfterClick(System.Action action)
    {
        if (click != null) click.PlayClick();

        // dùng realtime để không bị ảnh hưởng bởi Time.timeScale
        yield return new WaitForSecondsRealtime(clickDelay);

        action?.Invoke();
        running = null;
    }

    public void PlayGame()
    {
        RunAfterClick(() =>
        {
            SceneManager.LoadScene("Phuc_Scene");
        });
    }

    public void OpenInstructions()
    {
        RunAfterClick(() =>
        {
            instructionPanel.SetActive(true);
            mainButtonsGroup.SetActive(false);
        });
    }

    public void CloseInstructions()
    {
        RunAfterClick(() =>
        {
            instructionPanel.SetActive(false);
            mainButtonsGroup.SetActive(true);
        });
    }

    public void QuitGame()
    {
        RunAfterClick(() =>
        {
            Debug.Log("Đã thoát game!");
            Application.Quit();
        });
    }
}