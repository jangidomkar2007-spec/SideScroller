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

    [Header("Drops")]
    public GameObject healingDropPrefab;
    public GameObject doubleJumpDropPrefab;

    [Range(0, 100)]
    public int healingDropChance = 50;

    [Range(0, 100)]
    public int doubleJumpDropChance = 20;

    private bool canDamagePlayer = true;

    private Transform player;

    private bool isDead = false;
    private bool isWaiting = false;
    private bool isChasing = false;

    private int moveDirection = 1;

    private Animator animator;
    private Rigidbody2D rb;

    // ===================================
    // HEALTH BAR
    // ===================================

    [Header("Health Bar")]
    public Vector3 healthBarOffset =
        new Vector3(0f, 1.2f, 0f);

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

        CreateHealthBar();

        // START DIRECTION
        if (pointB != null)
        {
            if (pointB.position.x < transform.position.x)
            {
                moveDirection = -1;
            }
            else
            {
                moveDirection = 1;
            }
        }
    }

    // ===================================
    // HEALTH BAR SETUP
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

        // BACKGROUND

        GameObject bg =
            new GameObject("Background");

        bg.transform.SetParent(
            healthBarRoot.transform,
            false
        );

        Image bgImage =
            bg.AddComponent<Image>();

        bgImage.color =
            new Color(
                0.15f,
                0.15f,
                0.15f,
                0.85f
            );

        RectTransform bgRect =
            bg.GetComponent<RectTransform>();

        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // FILL

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

    void Update()
    {
        if (isDead)
            return;

        CheckPlayerDistance();

        if (
            healthBarRoot != null &&
            healthBarRoot.activeSelf
        )
        {
            UpdateHealthBar();
        }
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

        if (
            !isChasing &&
            distance <= chaseRadius
        )
        {
            isChasing = true;
        }
        else if (
            isChasing &&
            distance >= stopChaseDistance
        )
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
                player.position.x -
                transform.position.x
            );

        rb.linearVelocity =
            new Vector2(
                direction * chaseSpeed,
                rb.linearVelocity.y
            );

        Flip(direction);
    }

    // ===================================
    // PRO PATROL SYSTEM
    // ===================================

    void Patrol()
    {
        if (
            pointA == null ||
            pointB == null
        )
            return;

        if (isWaiting)
        {
            rb.linearVelocity =
                new Vector2(
                    0,
                    rb.linearVelocity.y
                );

            return;
        }

        rb.linearVelocity =
            new Vector2(
                moveDirection * moveSpeed,
                rb.linearVelocity.y
            );

        Flip(moveDirection);

        // RIGHT LIMIT

        if (
            moveDirection > 0 &&
            transform.position.x >=
            pointB.position.x
        )
        {
            StartCoroutine(TurnAround());
        }

        // LEFT LIMIT

        else if (
            moveDirection < 0 &&
            transform.position.x <=
            pointA.position.x
        )
        {
            StartCoroutine(TurnAround());
        }
    }

    IEnumerator TurnAround()
    {
        isWaiting = true;

        rb.linearVelocity =
            new Vector2(
                0,
                rb.linearVelocity.y
            );

        yield return new WaitForSeconds(waitTime);

        moveDirection *= -1;

        Flip(moveDirection);

        isWaiting = false;
    }

    // ===================================
    // FLIP
    // ===================================

    void Flip(float direction)
    {
        Vector3 scale =
            transform.localScale;

        if (direction > 0)
        {
            scale.x =
                Mathf.Abs(scale.x);
        }
        else if (direction < 0)
        {
            scale.x =
                -Mathf.Abs(scale.x);
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

        if (healthBarRoot != null)
        {
            healthBarRoot.SetActive(true);

            UpdateHealthBar();
        }

        if (HitStop.Instance != null)
        {
            HitStop.Instance.Trigger(
                hitStopIntensity
            );
        }

        if (hitParticle != null)
        {
            Instantiate(
                hitParticle,
                transform.position,
                Quaternion.identity
            );
        }

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
        if (
            !canDamagePlayer ||
            isDead
        )
            return;

        if (
            collision.gameObject.CompareTag(
                "Player"
            )
        )
        {
            PlayerController2D player =
                collision.gameObject
                .GetComponent<PlayerController2D>();

            if (player != null)
            {
                if (player.isInvincible)
                    return;

                player.TakeDamage(contactDamage);

                StartCoroutine(
                    DamageCooldown()
                );
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

        Collider2D col =
            GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        rb.linearVelocity = Vector2.zero;

        int randomValue =
            Random.Range(0, 100);

        if (
            randomValue <
            doubleJumpDropChance
        )
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
        else if (
            randomValue <
            healingDropChance +
            doubleJumpDropChance
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

        Destroy(
            gameObject,
            destroyDelay
        );
    }

    // ===================================
    // GIZMOS
    // ===================================

    void OnDrawGizmos()
    {
        if (
            pointA != null &&
            pointB != null
        )
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

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            chaseRadius
        );

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            stopChaseDistance
        );
    }
}