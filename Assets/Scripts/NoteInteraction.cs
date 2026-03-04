using UnityEngine;

public class NoteInteraction : MonoBehaviour
{
    public GameObject noteUI;
    public GameObject pressFText;

    private bool playerInRange = false;
    private bool hasRead = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F) && !hasRead)
        {
            ReadNote();
        }
    }

    void ReadNote()
    {
        hasRead = true;

        noteUI.SetActive(true);
        pressFText.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        // BẮT ĐẦU NHIỆM VỤ
        ObjectiveManager.Instance.StartObjective();

        // ĐỔI BẦU TRỜI
        SkyManager.Instance.ChangeToDark();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            pressFText.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            pressFText.SetActive(false);
        }
    }
}