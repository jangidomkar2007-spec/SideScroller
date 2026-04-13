using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUISetup : MonoBehaviour
{
    [Header("Player Reference")]
    public PlayerController2D playerController;

    private Canvas canvas;
    private Image healthBarFill;
    private TextMeshProUGUI healthText;
    private RectTransform canvasRectTransform;
    private RectTransform healthBarRectTransform;

    [Header("Colors")]
    public Color healthyColor = Color.green;
    public Color mediumColor = Color.yellow;
    public Color lowColor = Color.red;
    public Color deadColor = Color.black;

    [Header("Health Thresholds")]
    private float healthThresholdMedium = 0.66f;
    private float healthThresholdLow = 0.33f;

    void Start()
    {
        // Create Canvas if it doesn't exist
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            canvas = existingCanvas;
            Debug.Log("Found existing Canvas");
        }
        else
        {
            canvas = CreateCanvas();
            Debug.Log("Created new Canvas");
        }

        // Create Health Bar UI Elements
        CreateHealthBarUI(canvas.transform);

        Debug.Log("Health Bar UI Setup Complete!");
    }

    Canvas CreateCanvas()
    {
        // Create Canvas GameObject
        GameObject canvasObj = new GameObject("Canvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvasRectTransform = canvasObj.GetComponent<RectTransform>();

        // Set Canvas Settings
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Add Canvas Scaler
        CanvasScaler canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        // Add Graphic Raycaster for UI interaction
        canvasObj.AddComponent<GraphicRaycaster>();

        Debug.Log("Canvas created successfully");
        return canvas;
    }

    void CreateHealthBarUI(Transform canvasParent)
    {
        // ===== CREATE HEALTH BAR PANEL (Background) =====
        GameObject panelObj = new GameObject("HealthBarPanel");
        panelObj.transform.SetParent(canvasParent, false);

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark Gray Background

        healthBarRectTransform = panelObj.GetComponent<RectTransform>();
        healthBarRectTransform.anchorMin = new Vector2(1, 1); // Top Right
        healthBarRectTransform.anchorMax = new Vector2(1, 1); // Top Right
        healthBarRectTransform.pivot = new Vector2(1, 1); // Pivot at top right
        healthBarRectTransform.anchoredPosition = new Vector2(-20, -20); // Offset from corner
        healthBarRectTransform.sizeDelta = new Vector2(200, 40);

        // ===== CREATE HEALTH BAR FILL (Green Bar) =====
        GameObject fillObj = new GameObject("HealthBarFill");
        fillObj.transform.SetParent(panelObj.transform, false);

        healthBarFill = fillObj.AddComponent<Image>();
        healthBarFill.color = healthyColor; // Start with Green

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(5, 5); // Padding
        fillRect.offsetMax = new Vector2(-5, -5); // Padding

        // Set Fill Image Type
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthBarFill.fillAmount = 1f; // Start full

        // ===== CREATE HEALTH TEXT =====
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(panelObj.transform, false);

        healthText = textObj.AddComponent<TextMeshProUGUI>();
        healthText.text = "100 / 100";
        healthText.fontSize = 28;
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);

        Debug.Log("Health Bar UI elements created");
    }

    void Update()
    {
        if (playerController == null || healthBarFill == null || healthText == null)
        {
            Debug.LogWarning("Health Bar: Player Controller or UI elements not assigned!");
            return;
        }

        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        // Calculate health percentage
        float healthPercentage = (float)playerController.currentHealth / playerController.maxHealth;

        // Update fill amount
        healthBarFill.fillAmount = healthPercentage;

        // Change color based on health
        if (healthPercentage > healthThresholdMedium)
        {
            // Green - Full Health
            healthBarFill.color = healthyColor;
        }
        else if (healthPercentage > healthThresholdLow)
        {
            // Yellow - Medium Health
            healthBarFill.color = mediumColor;
        }
        else if (healthPercentage > 0)
        {
            // Red - Low Health
            healthBarFill.color = lowColor;
        }
        else if (playerController.isDead)
        {
            // Black - Dead
            healthBarFill.color = deadColor;
        }

        // Update health text
        healthText.text = playerController    .currentHealth + " / " + playerController.maxHealth;

        // Show DEAD text when dead
        if (playerController.isDead)
        {
            healthText.text = "DEAD";
        }
    }

    // Public method to manually set player controller (if not assigned in inspector)
    public void SetPlayerController(PlayerController2D player)
    {
        playerController = player;
        Debug.Log("Player Controller assigned to Health Bar UI");
    }
}
