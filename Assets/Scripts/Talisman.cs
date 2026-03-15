using UnityEngine;

public class Talisman : MonoBehaviour
{
    public ObjectiveManager objectiveManager;
    public AudioSource pickupSound;

    [Header("Glow Effect")]
    public Color glowColor = new Color(1f, 0.8f, 0.2f);
    public float glowIntensity = 1.5f;
    public float glowRange = 2f;
    private Light pointLight;

    private void Start()
    {
        // Add a point light to the talisman to make it glow
        pointLight = gameObject.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = glowColor;
        pointLight.intensity = glowIntensity;
        pointLight.range = glowRange;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (objectiveManager != null)
            {
                if (!objectiveManager.objectiveStarted) return; // Prevent early pickup
                objectiveManager.CollectTalisman();
            }

            if (pickupSound != null)
            {
                if (pickupSound.clip != null)
                    AudioSource.PlayClipAtPoint(pickupSound.clip, transform.position);
                else
                    pickupSound.Play();
            }

            // Hide object immediately but delay destruction to let sound play
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
            if (pointLight != null) pointLight.enabled = false;

            Destroy(gameObject, 2f);
        }
    }
}