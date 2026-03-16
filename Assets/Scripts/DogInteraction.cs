using UnityEngine;

public class DogInteraction : MonoBehaviour
{
    public GameObject interactUI;
    public GameObject dogMenuUI;

    private bool playerNear = false;
    private bool menuOpen = false;

    void Update()
    {
        if (playerNear && !menuOpen && Input.GetKeyDown(KeyCode.F))
        {
            interactUI.SetActive(false);
            dogMenuUI.SetActive(true);
            menuOpen = true;
        }
    }

    public void ResetInteraction()
    {
        menuOpen = false;

        // Không bật lại "Tương tác [F]"
        interactUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            if (!menuOpen)
                interactUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            interactUI.SetActive(false);
            dogMenuUI.SetActive(false);
            menuOpen = false;
        }
    }
}