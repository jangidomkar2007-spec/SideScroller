using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TutorialSystem : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI tutorialText;
    public Image tutorialPanel;
    public PlayerController2D player;
    public Transform enemyDetectPoint;

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public float hintDisplayTime = 3f;
    public float enemyDetectRange = 5f;

    // Track tutorial steps
    private bool shownMoveHint = false;
    private bool shownSprintHint = false;
    private bool shownJumpHint = false;
    private bool shownEnemyHint = false;
    private bool isTyping = false;

    void Start()
    {
        tutorialPanel.gameObject.SetActive(false);
        // Show move hint at start
        StartCoroutine(ShowHint("Press D to Move Forward  |  Press A to Move Backward"));
        shownMoveHint = true;
    }

    void Update()
    {
        // Sprint hint - when D is pressed
        if (shownMoveHint && !shownSprintHint && !isTyping && Input.GetKey(KeyCode.D))
        {
            shownSprintHint = true;
            StartCoroutine(ShowHint("Press Shift + D to Sprint"));
        }

        // Jump hint - when Shift+D is pressed
        if (shownSprintHint && !shownJumpHint &&
            Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.D))
        {
            shownJumpHint = true;
            StartCoroutine(ShowHint("Press Space to Jump over Obstacles"));
        }

        // Enemy hints - when enemy is nearby
        if (!shownEnemyHint && IsEnemyNearby())
        {
            shownEnemyHint = true;
            StartCoroutine(ShowHint("Left Click to Attack  |  Right Click to Heavy Attack  |  Alt to Dash"));
        }
    }

    bool IsEnemyNearby()
    {
        // Detect enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            player.transform.position,
            enemyDetectRange
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
                return true;
        }
        return false;
    }

    IEnumerator ShowHint(string message)
    {
        // Wait if already typing
        while (isTyping)
            yield return null;

        isTyping = true;
        tutorialPanel.gameObject.SetActive(true);
        tutorialText.text = "";

        // Typing effect
        foreach (char letter in message)
        {
            tutorialText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Wait then fade out
        yield return new WaitForSeconds(hintDisplayTime);
        StartCoroutine(FadeOut());
        isTyping = false;
    }

    IEnumerator FadeOut()
    {
        float timer = 0f;
        float fadeDuration = 1f;
        Color panelColor = tutorialPanel.color;
        Color textColor = tutorialText.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - (timer / fadeDuration);
            tutorialPanel.color = new Color(panelColor.r, panelColor.g, panelColor.b, alpha);
            tutorialText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            yield return null;
        }

        tutorialPanel.gameObject.SetActive(false);

        // Reset colors for next hint
        tutorialPanel.color = new Color(panelColor.r, panelColor.g, panelColor.b, 0.6f);
        tutorialText.color = new Color(textColor.r, textColor.g, textColor.b, 1f);
    }
}