using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    [Header("Boss Health")]
    public int maxHealth = 300;
    private int currentHealth;

    [Header("Death Settings")]
    public float destroyDelay = 1f;

    [Header("Hit Effects")]
    public float hitStopIntensity = 0.1f;
    public GameObject hitParticle;

    [Header("Player Damage")]
    public int contactDamage = 25;
    public float damageCooldown = 1f;

    [Header("Attack")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    private float attackTimer;

    private bool canDamagePlayer = true;
    private bool isDead = false;

    private Animator animator;
    private Rigidbody2D rb;
    private Transform player;

    // ===================================
    // HEALTH BAR
    // ===================================
    [Header("Boss Health Bar")]
    public Vector3 healthBarOffset =
        new Vector3(0f, 2f, 0f);

    public float healthBarWidth = 2.5f;
    public float healthBarHeight = 0.2f;

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
    }

    // ===================================
    // CREATE HEALTH BAR
    // ===================================
    void CreateHealthBar()
    {
        healthBarRoot =
            new GameObject("BossHealthBar");

        healthBarRoot.transform.SetParent(transform);

        healthBarRoot.transform.localPosition =
            healthBarOffset;

        healthBarRoot.transform.localScale =
            Vector3.one;

        healthBarCanvas =
            healthBarRoot.AddComponent<Canvas>();

        healthBarCanvas.renderMode =
            RenderMode.WorldSpace;

        healthBarCanvas.sortingOrder = 20;

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
                0.1f,
                0.1f,
                0.1f,
                0.9f
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

        healthBarFill.color = Color.red;

        healthBarFill.type =
            Image.Type.Filled;

        healthBarFill.fillMethod =
            Image.FillMethod.Horizontal;

        healthBarFill.fillOrigin = 0;

        healthBarFill.fillAmount = 1f;

        RectTransform fillRect =
            fill.GetComponent<RectTransform>();

        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // Hide until boss takes damage
        healthBarRoot.SetActive(false);
    }

    void Update()
    {
        if (isDead)
            return;

        if (
            healthBarRoot != null &&
            healthBarRoot.activeSelf
        )
        {
            UpdateHealthBar();
        }

        HandleAttack();
    }

    void UpdateHealthBar()
    {
        if (healthBarFill == null)
            return;

        float fraction =
            (float)currentHealth / maxHealth;

        healthBarFill.fillAmount = fraction;

        if (Camera.main != null)
        {
            healthBarRoot.transform.rotation =
                Camera.main.transform.rotation;
        }
    }

    // ===================================
    // ATTACK SYSTEM
    // ===================================
    void HandleAttack()
    {
        if (player == null)
            return;

        attackTimer -= Time.deltaTime;

        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );

        // FACE PLAYER
        if (player.position.x > transform.position.x)
        {
            transform.localScale =
                new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale =
                new Vector3(-1, 1, 1);
        }

        // ATTACK
        if (
            distance <= attackRange &&
            attackTimer <= 0
        )
        {
            animator.SetTrigger("Attack");

            attackTimer = attackCooldown;
        }
    }

    // ===================================
    // DAMAGE SYSTEM
    // ===================================
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        // SHOW HEALTH BAR
        if (healthBarRoot != null)
        {
            healthBarRoot.SetActive(true);
            UpdateHealthBar();
        }

        // HIT STOP
        if (HitStop.Instance != null)
        {
            HitStop.Instance.Trigger(
                hitStopIntensity
            );
        }

        // HIT PARTICLE
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
            " Boss took damage: " +
            damage
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ===================================
    // PLAYER DAMAGE
    // ===================================
    void OnCollisionStay2D(
        Collision2D collision
    )
    {
        if (!canDamagePlayer || isDead)
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

                player.TakeDamage(
                    contactDamage
                );

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
    // DEATH
    // ===================================
    void Die()
    {
        isDead = true;

        Debug.Log(
            gameObject.name + " Boss died"
        );

        Collider2D col =
            GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Destroy(
            gameObject,
            destroyDelay
        );
    }

    // ===================================
    // GIZMOS
    // ===================================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}