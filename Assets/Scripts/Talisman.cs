using UnityEngine;

public class Talisman : MonoBehaviour
{
    public ObjectiveManager objectiveManager;
    public AudioSource pickupSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (objectiveManager != null)
            {
                objectiveManager.CollectTalisman();
            }

            if (pickupSound != null)
            {
                pickupSound.Play();
            }

            Destroy(gameObject, 0.1f);
        }
    }
}