using UnityEngine;
using System.Collections;

public class JumpscareController : MonoBehaviour
{
    public Camera playerCamera;
    public Camera jumpscareCamera;

    public GameObject mada;
    public Transform cameraTargetPoint; // ?i?m tr??c m?t Mada

    //public AudioSource jumpscareSound;

    public float zoomSpeed = 5f;
    public float duration = 2f;

    public MonoBehaviour playerController; // script di chuy?n player

    public void StartJumpscare()
    {
        StartCoroutine(JumpscareSequence());
    }

    IEnumerator JumpscareSequence()
    {
        // Disable player control
        playerController.enabled = false;

        // Switch camera
        playerCamera.enabled = false;
        jumpscareCamera.enabled = true;

        //// Play sound
        //if (jumpscareSound != null)
        //    jumpscareSound.Play();

        // Hide other UI
        if (GameManager.Instance != null && GameManager.Instance.otherUIPanels != null)
        {
            foreach (GameObject ui in GameManager.Instance.otherUIPanels)
            {
                if (ui != null)
                    ui.SetActive(false);
            }
        }

        float timer = 0f;

        while (timer < duration)
        {
            jumpscareCamera.transform.position = Vector3.Lerp(
                jumpscareCamera.transform.position,
                cameraTargetPoint.position,
                Time.deltaTime * zoomSpeed
            );

            jumpscareCamera.transform.LookAt(mada.transform);
            timer += Time.deltaTime;
            yield return null;
        }

        // Game Over
        Debug.Log("Jumpscare Finished");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}
