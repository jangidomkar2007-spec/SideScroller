using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Death Settings")]
    public float destroyDelay = 0.3f;

    [Header("Hit Effects")]
    public float hitStopIntensity = 0.06f;
    public GameObject hitParticle;

    [Header("Patrol Settings")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;
    public float waitTime = 1f;

    [Header("Chase Settings")]
    public float chaseRadius = 5f;
    public float stopChaseDistance = 7f;
    public float chaseSpeed = 4f;

    [Header("Player Damage")]
    public int contactDamage = 10;
    public float damageCooldown = 1f;

    private bool canDamagePlayer = true;

    private Transform player;
    private Transform targetPoint;

    private bool isDead = false;
    private bool isWaiting = false;
    private bool isChasing = false;

    private Animator animator;
    private Rigidbody2D rb;

    // ===================================
    // HEALTH BAR
    // ===================================
    [Header("Health Bar")]
    public Vector3 healthBarOffset = new Vector3(0f, 1.2f, 0f);
    public float healthBarWidth = 1f;
    public float healthBarHeight = 0.12f;

    private GameObject healthBarRoot;
    private Image healthBarFill;
    private Canvas healthBarCanvas;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;

        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (pointA != null && pointB != null)
        {
            targetPoint = pointB;
        }

        CreateHealthBar();
    }

    // ===================================
    // HEALTH BAR SETUP
    // ===================================
    void CreateHealthBar()
    {
        // Root object positioned above enemy
        healthBarRoot = new GameObject("HealthBar");
        healthBarRoot.transform.SetParent(transform);
        healthBarRoot.transform.localPosition = healthBarOffset;
        healthBarRoot.transform.localRotation = Quaternion.identity;
        healthBarRoot.transform.localScale = Vector3.one;

        // World-space canvas so it sits in the scene
        healthBarCanvas = healthBarRoot.AddComponent<Canvas>();
        healthBarCanvas.renderMode = RenderMode.WorldSpace;
        healthBarCanvas.sortingLayerName = "Default";
        healthBarCanvas.sortingOrder = 10;

        RectTransform canvasRect =
            healthBarRoot.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(healthBarWidth, healthBarHeight);

        // Background (dark)
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(healthBarRoot.transform, false);

        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Fill (green → red as health drops)
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(healthBarRoot.transform, false);

        healthBarFill = fill.AddComponent<Image>();
        healthBarFill.color = Color.green;
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillOrigin = 0; // fills left → right
        healthBarFill.fillAmount = 1f;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // Hide bar at full health — shows on first hit
        healthBarRoot.SetActive(false);
    }

    void UpdateHealthBar()
    {
        if (healthBarFill == null) return;

        float fraction = (float)currentHealth / maxHealth;
        healthBarFill.fillAmount = fraction;

        // Green at full, red at empty
        healthBarFill.color = Color.Lerp(Color.red, Color.green, fraction);

        // Keep bar facing the camera so it's always readable
        if (Camera.main != null)
        {
            healthBarRoot.transform.rotation =
                Camera.main.transform.rotation;
        }
    }

    void Update()
    {
        if (isDead)
            return;

        CheckPlayerDistance();

        // Keep health bar facing camera every frame
        if (healthBarRoot != null && healthBarRoot.activeSelf)
            UpdateHealthBar();
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    // ===================================
    // PLAYER DETECTION
    // ===================================
    void CheckPlayerDistance()
    {
        if (player == null)
            return;

        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );

        // START CHASE
        if (distance <= chaseRadius)
        {
            isChasing = true;
            isWaiting = false;
        }

        // STOP CHASE
        if (distance >= stopChaseDistance)
        {
            isChasing = false;
        }
    }

    // ===================================
    // CHASE PLAYER
    // ===================================
    void ChasePlayer()
    {
        if (player == null)
            return;

        float direction =
            Mathf.Sign(
                player.position.x - transform.position.x
            );

        rb.linearVelocity = new Vector2(
            direction * chaseSpeed,
            rb.linearVelocity.y
        );

        Flip(direction);
    }

    // ===================================
    // PATROL SYSTEM
    // ===================================
    void Patrol()
    {
        if (pointA == null ||
            pointB == null ||
            targetPoint == null)
            return;

        if (isWaiting)
        {
            rb.linearVelocity = new Vector2(
                0,
                rb.linearVelocity.y
            );

            return;
        }

        float direction =
            Mathf.Sign(
                targetPoint.position.x - transform.position.x
            );

        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );

        Flip(direction);

        float distance =
            Mathf.Abs(
                targetPoint.position.x -
                transform.position.x
            );

        if (distance <= 0.2f)
        {
            StartCoroutine(WaitAndSwitchPoint());
        }
    }

    IEnumerator WaitAndSwitchPoint()
    {
        isWaiting = true;

        rb.linearVelocity = new Vector2(
            0,
            rb.linearVelocity.y
        );

        yield return new WaitForSeconds(waitTime);

        if (targetPoint == pointA)
        {
            targetPoint = pointB;
        }
        else
        {
            targetPoint = pointA;
        }

        isWaiting = false;
    }

    // ===================================
    // FLIP
    // ===================================
    void Flip(float direction)
    {
        Vector3 scale = transform.localScale;

        if (direction > 0)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else if (direction < 0)
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }

    // ===================================
    // DAMAGE SYSTEM
    // ===================================
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        // Show and update health bar on first hit
        if (healthBarRoot != null)
        {
            healthBarRoot.SetActive(true);
            UpdateHealthBar();
        }

        if (HitStop.Instance != null)
        {
            HitStop.Instance.Trigger(hitStopIntensity);
        }

        if (hitParticle != null)
        {
            Instantiate(
                hitParticle,
                transform.position,
                Quaternion.identity
            );
        }

        Debug.Log(
            gameObject.name +
            " took damage: " +
            damage +
            " | Health Left: " +
            currentHealth
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ===================================
    // PLAYER DAMAGE
    // ===================================
    private void OnCollisionStay2D(
        Collision2D collision
    )
    {
        if (!canDamagePlayer || isDead)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController2D player =
                collision.gameObject
                .GetComponent<PlayerController2D>();

            if (player != null)
            {
                if (player.isInvincible)
                    return;

                player.TakeDamage(contactDamage);

                StartCoroutine(DamageCooldown());
            }
        }
    }

    IEnumerator DamageCooldown()
    {
        canDamagePlayer = false;

        yield return new WaitForSeconds(
            damageCooldown
        );

        canDamagePlayer = true;
    }

    // ===================================
    // DEATH SYSTEM
    // ===================================
    void Die()
    {
        isDead = true;

        Debug.Log(gameObject.name + " died");

        Collider2D col =
            GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        rb.linearVelocity = Vector2.zero;

        Destroy(gameObject, destroyDelay);
    }

    // ===================================
    // GIZMOS
    // ===================================
    void OnDrawGizmos()
    {
        // PATROL LINE
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(
                pointA.position,
                pointB.position
            );

            Gizmos.DrawSphere(
                pointA.position,
                0.2f
            );

            Gizmos.DrawSphere(
                pointB.position,
                0.2f
            );
        }

        // CHASE RADIUS
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            chaseRadius
        );

        // STOP CHASE RADIUS
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            stopChaseDistance
        );
    }
}