using UnityEngine;

public class WaterZoneTrigger : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource splashAudio;

    [Header("Water Settings")]
    public MadaFollowerAI mada;
    public float waterSurfaceOffset = 0f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player entered water");

        // ===== BÁO CHO MADA PLAYER Ở TRONG NƯỚC =====
        if (mada != null)
        {
            mada.SetPlayerInWater(true);
        }

        // ===== BẬT FOG =====
        RenderSettings.fog = true;

        // ===== SPLASH SOUND =====
        if (splashAudio != null && splashAudio.clip != null)
        {
            splashAudio.PlayOneShot(splashAudio.clip);
        }

        // ===== PLAYER SWIMMING =====
        PlayerSwimming swim = other.GetComponent<PlayerSwimming>();
        if (swim != null)
        {
            swim.waterSurfaceY = transform.position.y + waterSurfaceOffset;
            swim.EnterWater();
        }

        // ===== PLAYER MOVE =====
        PlayerMove move = other.GetComponent<PlayerMove>();
        if (move != null)
        {
            move.isSwimming = true;
        }

        // ===== PLAY SWIM AUDIO =====
        AudioSource playerAudio = other.GetComponent<AudioSource>();
        if (playerAudio != null && !playerAudio.isPlaying)
        {
            playerAudio.Play();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player left water");

        // ===== BÁO CHO MADA PLAYER RA KHỎI NƯỚC =====
        if (mada != null)
        {
            mada.SetPlayerInWater(false);
        }

        // ===== TẮT FOG =====
        RenderSettings.fog = false;

        // ===== TẮT SWIMMING =====
        PlayerSwimming swim = other.GetComponent<PlayerSwimming>();
        if (swim != null)
        {
            swim.ExitWater();
        }

        // ===== TẮT SWIM TRONG MOVE =====
        PlayerMove move = other.GetComponent<PlayerMove>();
        if (move != null)
        {
            move.isSwimming = false;
        }

        // ===== STOP SWIM AUDIO =====
        AudioSource playerAudio = other.GetComponent<AudioSource>();
        if (playerAudio != null)
        {
            playerAudio.Stop();
        }
    }
}