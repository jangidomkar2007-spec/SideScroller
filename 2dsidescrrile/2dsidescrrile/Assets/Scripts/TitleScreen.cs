using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleScreen : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI pressAnyKeyText;

    [Header("Settings")]
    public float blinkSpeed = 1f;
    public string MainMenuSceneName = "Main Menu";

    private float timer;
    private bool isVisible = true;

    void Update()
    {
        // Blinking effect
        timer += Time.deltaTime;
        if (timer >= blinkSpeed)
        {
            isVisible = !isVisible;
            pressAnyKeyText.enabled = isVisible;
            timer = 0f;
        }

        // Detect any key press
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(1);
        }
    }
}