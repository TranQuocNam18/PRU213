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
                if (pickupSound.clip != null)
                    AudioSource.PlayClipAtPoint(pickupSound.clip, transform.position);
                else
                    pickupSound.Play();
            }

            // Hide object immediately but delay destruction to let sound play
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

            Destroy(gameObject, 2f);
        }
    }
}