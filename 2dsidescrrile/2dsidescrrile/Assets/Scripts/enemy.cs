using UnityEngine;
using System.Collections;

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
    }

    void Update()
    {
        if (isDead)
            return;

        CheckPlayerDistance();

        
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