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

    private Transform targetPoint;
    private bool isDead = false;
    private bool isWaiting = false;

    void Start()
    {
        currentHealth = maxHealth;

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
            Instantiate(hitParticle, transform.position, Quaternion.identity);
        }

        Debug.Log(gameObject.name + " took damage: " + damage +
                  " | Health Left: " + currentHealth);

        // Check Death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // =========================
    // PATROL SYSTEM
    // =========================
    void Patrol()
    {
        if (pointA == null || pointB == null || targetPoint == null)
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
        Vector3 direction = targetPoint.position - transform.position;

        // Proper Flip Without Changing Size
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
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
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

        // Disable Collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Stop Rigidbody Movement
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

            Gizmos.DrawLine(pointA.position, pointB.position);

            Gizmos.DrawSphere(pointA.position, 0.2f);
            Gizmos.DrawSphere(pointB.position, 0.2f);
        }
    }
}