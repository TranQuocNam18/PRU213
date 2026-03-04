using UnityEngine;
using TMPro;

public class TalismanUI : MonoBehaviour
{
    public TextMeshProUGUI talismanText;

    void Update()
    {
        talismanText.text = "Bùa: " +
            ObjectiveManager.Instance.collectedTalismans
            + " / " +
            ObjectiveManager.Instance.totalTalismans;
    }
}