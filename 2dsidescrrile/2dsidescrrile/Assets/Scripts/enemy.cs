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

    [Header("Player Damage")]
    public int contactDamage = 10;
    public float damageCooldown = 1f;

    private bool canDamagePlayer = true;

    private Transform targetPoint;
    private bool isDead = false;
    private bool isWaiting = false;

    // ANIMATOR
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;

        // GET ANIMATOR
        animator = GetComponent<Animator>();

        // Start patrol toward Point B
        if (pointA != null && pointB != null)
        {
            targetPoint = pointB;
        }
    }

    void Update()
    {
        if (isDead)
            return;

        Patrol();

        // PLAY WALK ANIMATION
        if (animator != null)
        {
            animator.Play("Walk");
        }
    }

    // =========================
    // DAMAGE SYSTEM
    // =========================
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        // Hit Stop Effect
        if (HitStop.Instance != null)
        {
            HitStop.Instance.Trigger(hitStopIntensity);
        }

        // Hit Particle Effect
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

        // Check Death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // =========================
    // PLAYER DAMAGE
    // =========================
    private void OnCollisionStay2D(Collision2D collision)
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

        yield return new WaitForSeconds(damageCooldown);

        canDamagePlayer = true;
    }

    // =========================
    // PATROL SYSTEM
    // =========================
    void Patrol()
    {
        if (pointA == null ||
            pointB == null ||
            targetPoint == null)
            return;

        if (isWaiting)
            return;

        // Move Enemy
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPoint.position,
            moveSpeed * Time.deltaTime
        );

        // Direction Check
        Vector3 direction =
            targetPoint.position - transform.position;

        // Proper Flip
        Vector3 scale = transform.localScale;

        if (direction.x > 0)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else if (direction.x < 0)
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        transform.localScale = scale;

        // Reached Target
        if (Vector2.Distance(
            transform.position,
            targetPoint.position) < 0.1f)
        {
            StartCoroutine(WaitAndSwitchPoint());
        }
    }

    IEnumerator WaitAndSwitchPoint()
    {
        isWaiting = true;

        yield return new WaitForSeconds(waitTime);

        // Switch Patrol Point
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

    // =========================
    // DEATH SYSTEM
    // =========================
    void Die()
    {
        isDead = true;

        Debug.Log(gameObject.name + " died");

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Destroy(gameObject, destroyDelay);
    }

    // =========================
    // GIZMOS
    // =========================
    void OnDrawGizmos()
    {
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
    }
}