using UnityEngine;
using TMPro;

public class TalismanUI : MonoBehaviour
{
    public TextMeshProUGUI talismanText;

    void Update()
    {
        if (ObjectiveManager.Instance != null && talismanText != null)
        {
            if (ObjectiveManager.Instance.collectedTalismans < ObjectiveManager.Instance.totalTalismans)
            {
                talismanText.text = "Bùa: " + ObjectiveManager.Instance.collectedTalismans + " / " + ObjectiveManager.Instance.totalTalismans;
            }
            else
            {
                talismanText.text = "Đã đủ bùa hãy quay lại chỗ sư thầy";
            }
        }
    }
}
