using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    // ===================================
    // HEALTH
    // ===================================

    [Header("Health")]
    public int maxHealth = 50;
    private int currentHealth;

    // ===================================
    // MOVEMENT
    // ===================================

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Patrol")]
    public Transform pointA;
    public Transform pointB;

    private Vector3 targetPoint;

    // ===================================
    // HOLLOW KNIGHT CHASE
    // ===================================

    [Header("Hollow Knight Chase")]

    public float chaseRadius = 7f;
    public float losePlayerRadius = 12f;

    public float chaseSpeed = 5f;

    public float acceleration = 12f;
    public float deceleration = 18f;

    public float attackStopDistance = 1.2f;

    private float currentSpeed;

    private float rememberPlayerTime = 2f;
    private float rememberTimer;

    private Vector2 lastKnownDirection;

    // ===================================
    // DAMAGE
    // ===================================

    [Header("Damage")]
    public int contactDamage = 10;
    public float damageCooldown = 1f;

    // ===================================
    // EFFECTS
    // ===================================

    [Header("Effects")]
    public float hitStopIntensity = 0.06f;
    public GameObject hitParticle;

    // ===================================
    // DROPS
    // ===================================

    [Header("Drops")]
    public GameObject healingDropPrefab;
    public GameObject doubleJumpDropPrefab;

    [Range(0, 100)]
    public int healingDropChance = 50;

    [Range(0, 100)]
    public int doubleJumpDropChance = 20;

    // ===================================
    // HEALTH BAR
    // ===================================

    [Header("Health Bar")]
    public Vector3 healthBarOffset =
        new Vector3(0f, 1.2f, 0f);

    public float healthBarWidth = 1f;
    public float healthBarHeight = 0.12f;

    // ===================================
    // PRIVATE
    // ===================================

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;

    private bool isDead = false;
    private bool isChasing = false;
    private bool canDamagePlayer = true;

    private GameObject healthBarRoot;
    private Image healthBarFill;
    private Canvas healthBarCanvas;

    // ===================================
    // START
    // ===================================

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

        targetPoint = pointB.position;

        CreateHealthBar();
    }

    // ===================================
    // UPDATE
    // ===================================

    void Update()
    {
        if (isDead)
            return;

        CheckPlayerDistance();

        UpdateAnimator();

        if (healthBarRoot != null &&
            healthBarRoot.activeSelf)
        {
            UpdateHealthBar();
        }
    }

    // ===================================
    // FIXED UPDATE
    // ===================================

    void FixedUpdate()
    {
        if (isDead)
            return;

        if (isChasing)
        {
            HollowKnightChase();
        }
        else
        {
            Patrol();
        }
    }

    // ===================================
    // PATROL SYSTEM
    // ===================================

    void Patrol()
    {
        Vector2 direction =
            (targetPoint - transform.position).normalized;

        currentSpeed = Mathf.Lerp(
            currentSpeed,
            moveSpeed,
            acceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity =
            new Vector2(
                direction.x * currentSpeed,
                rb.linearVelocity.y
            );

        if (Mathf.Abs(direction.x) > 0.05f)
        {
            Flip(direction.x);
        }

        float distance =
            Vector2.Distance(
                transform.position,
                targetPoint
            );

        if (distance < 0.3f)
        {
            if (targetPoint == pointA.position)
            {
                targetPoint = pointB.position;
            }
            else
            {
                targetPoint = pointA.position;
            }
        }
    }

    // ===================================
    // HOLLOW KNIGHT CHASE
    // ===================================

    void HollowKnightChase()
    {
        if (player == null)
            return;

        Vector2 direction =
            (player.position - transform.position);

        float distance = direction.magnitude;

        direction.Normalize();

        lastKnownDirection = direction;

        // STOP CLOSE TO PLAYER

        if (distance <= attackStopDistance)
        {
            currentSpeed = Mathf.Lerp(
                currentSpeed,
                0,
                deceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            currentSpeed = Mathf.Lerp(
                currentSpeed,
                chaseSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }

        rb.linearVelocity =
            new Vector2(
                direction.x * currentSpeed,
                rb.linearVelocity.y
            );

        // SMOOTH FLIP

        if (Mathf.Abs(direction.x) > 0.05f)
        {
            Flip(direction.x);
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

            rememberTimer = rememberPlayerTime;
        }

        // REMEMBER PLAYER

        if (isChasing)
        {
            rememberTimer -= Time.deltaTime;

            if (distance >= losePlayerRadius &&
                rememberTimer <= 0)
            {
                isChasing = false;
            }
        }
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
    // ANIMATION
    // ===================================

    void UpdateAnimator()
    {
        if (animator == null)
            return;

        animator.SetFloat(
            "Speed",
            Mathf.Abs(rb.linearVelocity.x)
        );

        animator.SetBool(
            "IsChasing",
            isChasing
        );
    }

    // ===================================
    // DAMAGE
    // ===================================

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (healthBarRoot != null)
        {
            healthBarRoot.SetActive(true);

            UpdateHealthBar();
        }

        // FORCE CHASE AFTER HIT

        isChasing = true;

        rememberTimer = rememberPlayerTime;

        // HITSTOP

        if (HitStop.Instance != null)
        {
            HitStop.Instance.Trigger(hitStopIntensity);
        }

        // PARTICLE

        if (hitParticle != null)
        {
            Instantiate(
                hitParticle,
                transform.position,
                Quaternion.identity
            );
        }

        // DEATH

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ===================================
    // PLAYER DAMAGE
    // ===================================

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!canDamagePlayer || isDead)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController2D playerController =
                collision.gameObject.GetComponent<PlayerController2D>();

            if (playerController != null)
            {
                if (playerController.isInvincible)
                    return;

                playerController.TakeDamage(contactDamage);

                StartCoroutine(DamageCooldown());
            }
        }
    }

    IEnumerator DamageCooldown()
    {
        canDamagePlayer = false;

        yield return new WaitForSeconds(damageCooldown);

        canDamagePlayer = true;
    }

    // ===================================
    // DEATH
    // ===================================

    void Die()
    {
        isDead = true;

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        rb.linearVelocity = Vector2.zero;

        int randomValue = Random.Range(0, 100);

        // DOUBLE JUMP DROP

        if (randomValue < doubleJumpDropChance)
        {
            if (doubleJumpDropPrefab != null)
            {
                Instantiate(
                    doubleJumpDropPrefab,
                    transform.position,
                    Quaternion.identity
                );
            }
        }

        // HEAL DROP

        else if (
            randomValue <
            healingDropChance + doubleJumpDropChance
        )
        {
            if (healingDropPrefab != null)
            {
                Instantiate(
                    healingDropPrefab,
                    transform.position,
                    Quaternion.identity
                );
            }
        }

        Destroy(gameObject, 0.3f);
    }

    // ===================================
    // HEALTH BAR
    // ===================================

    void CreateHealthBar()
    {
        healthBarRoot = new GameObject("HealthBar");

        healthBarRoot.transform.SetParent(transform);

        healthBarRoot.transform.localPosition =
            healthBarOffset;

        healthBarCanvas =
            healthBarRoot.AddComponent<Canvas>();

        healthBarCanvas.renderMode =
            RenderMode.WorldSpace;

        RectTransform canvasRect =
            healthBarRoot.GetComponent<RectTransform>();

        canvasRect.sizeDelta =
            new Vector2(
                healthBarWidth,
                healthBarHeight
            );

        GameObject fill =
            new GameObject("Fill");

        fill.transform.SetParent(
            healthBarRoot.transform,
            false
        );

        healthBarFill =
            fill.AddComponent<Image>();

        healthBarFill.color = Color.green;

        healthBarFill.type =
            Image.Type.Filled;

        healthBarFill.fillMethod =
            Image.FillMethod.Horizontal;

        healthBarFill.fillAmount = 1f;

        RectTransform fillRect =
            fill.GetComponent<RectTransform>();

        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        healthBarRoot.SetActive(false);
    }

    void UpdateHealthBar()
    {
        if (healthBarFill == null)
            return;

        float fraction =
            (float)currentHealth / maxHealth;

        healthBarFill.fillAmount = fraction;

        healthBarFill.color =
            Color.Lerp(
                Color.red,
                Color.green,
                fraction
            );

        if (Camera.main != null)
        {
            healthBarRoot.transform.rotation =
                Camera.main.transform.rotation;
        }
    }

    // ===================================
    // GIZMOS
    // ===================================

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            chaseRadius
        );

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            losePlayerRadius
        );

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            attackStopDistance
        );
    }
}