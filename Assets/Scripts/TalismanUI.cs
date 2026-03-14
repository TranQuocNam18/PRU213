using UnityEngine;
using TMPro;

public class TalismanUI : MonoBehaviour
{
    // Static reference giúp GameManager luôn tìm thấy, dù GameObject đang inactive
    public static TalismanUI Instance { get; private set; }

    public TextMeshProUGUI talismanText;

    void Awake()
    {
        Instance = this;
    }
}
