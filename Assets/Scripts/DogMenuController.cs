using UnityEngine;
using UnityEngine.UI;

public class DogMenuController : MonoBehaviour
{
    public Button[] buttons;
    public Animator dogAnimator;

    public GameObject interactUI;      // "Tương tác [F]"
    public DogInteraction dogInteraction;

    public GameObject dogNameUI;       // Name tag của dog

    int currentIndex = 0;

    void OnEnable()
    {
        currentIndex = 0;
        Highlight();
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0)
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = buttons.Length - 1;

            Highlight();
        }

        if (scroll < 0)
        {
            currentIndex++;
            if (currentIndex >= buttons.Length)
                currentIndex = 0;

            Highlight();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            SelectOption();
        }
    }

    void Highlight()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            ColorBlock colors = buttons[i].colors;

            if (i == currentIndex)
            {
                colors.normalColor = new Color(1f, 0.9f, 0.5f); // màu vàng nhạt
            }
            else
            {
                colors.normalColor = Color.white;
            }

            buttons[i].colors = colors;
        }
    }

    void SelectOption()
    {
        // Ẩn tên dog khi animation bắt đầu
        if (dogNameUI != null)
        {
            dogNameUI.SetActive(false);
        }

        switch (currentIndex)
        {
            case 0:
                dogAnimator.SetTrigger("Sit");
                break;

            case 1:
                dogAnimator.SetTrigger("Shake");
                break;

            case 2:
                dogAnimator.SetTrigger("Roll");
                break;

            case 3:
                dogAnimator.SetTrigger("Dead");
                break;
        }

        // đóng menu
        gameObject.SetActive(false);

        // sau 9s hiện lại interaction + name
        Invoke(nameof(ResetInteraction), 9f);
    }

    void ResetInteraction()
    {
        if (dogInteraction != null)
        {
            dogInteraction.ResetInteraction();
        }

        // hiện lại name tag
        if (dogNameUI != null)
        {
            dogNameUI.SetActive(true);
        }
    }
}