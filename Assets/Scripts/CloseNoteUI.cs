using UnityEngine;

public class CloseNoteUI : MonoBehaviour
{
    public GameObject notePanel;

    public void CloseNote()
    {
        notePanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}