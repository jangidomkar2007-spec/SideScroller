using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 300;
    private int currentHealth;

    [Header("Attack")]
    public float attackRange = 2f;
    public int attackDamage = 25;
    public float attackCooldown = 2f;

    [Header("Death")]
    public float destroyDelay = 2f;

    [Header("Effects")]
    public GameObject hitParticle;
    public float hitStopIntensity = 0.08f;

    [Header("Health Bar")]
    public Vector3 healthBarOffset =
        new Vector3(0f, 2.5f, 0f);

    public float healthBarWidth = 2.5f;
    public float healthBarHeight = 0.25f;

    private int currentAttackCooldown;

    private bool isDead = false;
    private bool canAttack = true;

    private Animator animator;
    private Rigidbody2D rb;
    private Transform player;

    // HEALTH BAR
    private GameObject healthBarRoot;
    private Image healthBarFill;
    private Canvas healthBarCanvas;

    void Start()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // IMPORTANT
        // BOSS SHOULD NOT MOVE WHEN HIT
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        CreateHealthBar();
    }

    void Update()
    {
        if (isDead)
            return;

        FacePlayer();

        HandleAttack();

        UpdateHealthBar();
    }

    // =================================
    // FACE PLAYER
    // =================================
    void FacePlayer()
    {
        if (player == null)
            return;

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
    }

    // =================================
    // ATTACK SYSTEM
    // =================================
    void HandleAttack()
    {
        if (player == null || !canAttack)
            return;

        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );

        if (distance <= attackRange)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        canAttack = false;

        // ATTACK ANIMATION
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.4f);

        // DAMAGE PLAYER
        if (player != null)
        {
            float distance =
                Vector2.Distance(
                    transform.position,
                    player.position
                );

            if (distance <= attackRange)
            {
                PlayerController2D pc =
                    player.GetComponent<PlayerController2D>();

                if (pc != null)
                {
                    if (!pc.isInvincible)
                    {
                        pc.TakeDamage(attackDamage);
                    }
                }
            }
        }

        yield return new WaitForSeconds(
            attackCooldown
        );

        canAttack = true;
    }

    // =================================
    // DAMAGE SYSTEM
    // =================================
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        // SHOW HEALTH BAR
        if (healthBarRoot != null)
        {
            healthBarRoot.SetActive(true);
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

        // HIT STOP
        if (HitStop.Instance != null)
        {
            HitStop.Instance.Trigger(
                hitStopIntensity
            );
        }

        // OPTIONAL HIT ANIMATION
        animator.SetTrigger("Hit");

        Debug.Log(
            "Boss Took Damage: " + damage
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // =================================
    // DEATH
    // =================================
    void Die()
    {
        isDead = true;

        Debug.Log("Boss Died");

        rb.linearVelocity = Vector2.zero;

        Collider2D col =
            GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        animator.SetTrigger("Death");

        Destroy(
            gameObject,
            destroyDelay
        );
    }

    // =================================
    // HEALTH BAR
    // =================================
    void CreateHealthBar()
    {
        healthBarRoot =
            new GameObject("BossHealthBar");

        healthBarRoot.transform.SetParent(
            transform
        );

        healthBarRoot.transform.localPosition =
            healthBarOffset;

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
                0f,
                0f,
                0f,
                0.8f
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
        if (
            healthBarFill == null ||
            healthBarRoot == null
        )
            return;

        float healthPercent =
            (float)currentHealth / maxHealth;

        healthBarFill.fillAmount =
            healthPercent;

        if (Camera.main != null)
        {
            healthBarRoot.transform.rotation =
                Camera.main.transform.rotation;
        }
    }

    // =================================
    // GIZMOS
    // =================================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}