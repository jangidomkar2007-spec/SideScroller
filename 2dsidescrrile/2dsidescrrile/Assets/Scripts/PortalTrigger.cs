using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTrigger : MonoBehaviour
{
    [Header("Portal Settings")]
    public string targetScene = "Level 3";
    public KeyCode activationKey = KeyCode.E;

    [Header("UI Prompt (optional)")]
    public GameObject promptUI; // drag a "Press E to Enter" UI object here

    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(activationKey))
        {
            SceneManager.LoadScene(targetScene);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (promptUI != null)
                promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }
}