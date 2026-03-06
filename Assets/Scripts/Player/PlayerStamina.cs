using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    public float maxStamina = 100f;
    public float stamina;
    public Slider staminaSlider; // Thêm Slider UI

    [Header("Recovery")]
    public float recoveryRate = 10f;     // hoi phuc / giây
    public float recoveryDelay = 3f;      // doi may giay khi het stamina

    bool exhausted;
    float delayTimer;

    void Start()
    {
        stamina = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = stamina;
        }
    }

    void Update()
    {
        //  met → dem delay
        if (exhausted)
        {
            delayTimer -= Time.deltaTime;

            if (delayTimer <= 0f)
            {
                exhausted = false;
            }
            return;
        }

        // hoi stamina
        if (stamina < maxStamina)
        {
            stamina += recoveryRate * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
        }

        // Cập nhật Slider UI
        if (staminaSlider != null)
        {
            staminaSlider.value = stamina;
        }
    }

    public void UseStamina(float amount)
    {
        if (exhausted) return;

        stamina -= amount;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        // het stamina → chay delay
        if (stamina <= 0f)
        {
            exhausted = true;
            delayTimer = recoveryDelay;
        }
    }

    public bool CanRun()
    {
        return stamina > 0.2f && !exhausted;
    }
}
