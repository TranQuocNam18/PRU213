using UnityEngine;
using TMPro;

public class NPCNameTag : MonoBehaviour
{
    public string npcName = "NPC";

    private TextMeshPro text;

    void Start()
    {
        text = GetComponent<TextMeshPro>();

        if (text != null)
        {
            text.text = npcName;
        }
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }
    }
}