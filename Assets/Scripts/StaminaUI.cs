using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public Slider slider;
    public Image fill;

    public Color green = new Color32(0, 191, 255, 255);
    public Color yellow = new Color32(255, 215, 0, 255);
    public Color red = new Color32(255, 42, 42, 255);

    void Update()
    {
        float percent = slider.value / slider.maxValue;

        if (percent > 0.6f)
            fill.color = Color.Lerp(yellow, green, (percent - 0.6f) / 0.4f);

        else if (percent > 0.3f)
            fill.color = Color.Lerp(red, yellow, (percent - 0.3f) / 0.3f);

        else
            fill.color = red;
    }
}