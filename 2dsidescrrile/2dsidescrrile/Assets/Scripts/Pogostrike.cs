using UnityEngine;
using System.Collections;

public class PogoStrike : MonoBehaviour
{
    [Header("Pogo Settings")]
    [SerializeField] private float pogoBounceForce = 12f;
    [SerializeField] private int pogoDamage = 25;
    [SerializeField] private float pogoCooldown = 0.3f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Attack Point")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.8f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject pogoEffectPrefab;
    [SerializeField] private Color pogoHitColor = Color.cyan;
    [SerializeField] private float effectLifetime = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip pogoSound;

    private PlayerController2D playerController;
    private Rigidbody2D rb;
    private AudioSource audioSource;

    private bool canPogo = true;
    private float cooldownTimer;

    void Start()
    {
        playerController = GetComponent<PlayerController2D>();
        rb = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && pogoSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Create attack point automatically
        if (attackPoint == null)
        {
            GameObject obj = new GameObject("PogoAttackPoint");
            obj.transform.parent = transform;

            // Position below player
            obj.transform.localPosition = new Vector3(0f, -0.8f, 0f);

            attackPoint = obj.transform;
        }
    }

    void Update()
    {
        // Cooldown
        if (!canPogo)
        {
            cooldownTimer -= Time.unscaledDeltaTime;

            if (cooldownTimer <= 0)
            {
                canPogo = true;
            }
        }

        // Input
        bool pogoInput =
            (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) &&
            (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));

        // Perform pogo
        if (pogoInput && canPogo && !IsGrounded() && !playerController.isDead)
        {
            PerformPogoStrike();
        }
    }

    void PerformPogoStrike()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        if (hitEnemies.Length > 0)
        {
            canPogo = false;
            cooldownTimer = pogoCooldown;

            // Bounce player upward
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, pogoBounceForce);

            // Spawn effect from attack point
            SpawnPogoEffect();

            // Play sound
            PlayPogoSound();

            foreach (Collider2D enemy in hitEnemies)
            {
                // Damage enemy
                enemy.SendMessage(
                    "TakeDamage",
                    pogoDamage,
                    SendMessageOptions.DontRequireReceiver
                );

                // Small enemy knockback
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();

                if (enemyRb != null)
                {
                    enemyRb.linearVelocity = new Vector2(0, 5f);
                }
            }
        }
    }

    void SpawnPogoEffect()
    {
        if (pogoEffectPrefab == null) return;

        // Spawn effect EXACTLY at attack point
        GameObject effect = Instantiate(
            pogoEffectPrefab,
            attackPoint.position,
            attackPoint.rotation
        );

        // Reset scale
        effect.transform.localScale = Vector3.one;

        // Color all sprites
        SpriteRenderer[] sprites = effect.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sr in sprites)
        {
            sr.color = pogoHitColor;
        }

        // Color all particles
        ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particles)
        {
            var main = ps.main;
            main.startColor = pogoHitColor;
        }

        Destroy(effect, effectLifetime);
    }

    void PlayPogoSound()
    {
        if (audioSource != null && pogoSound != null)
        {
            audioSource.PlayOneShot(pogoSound);
        }
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            0.2f
        );

        return hit.collider != null &&
               hit.collider.CompareTag("Ground");
    }

    void OnDrawGizmosSelected()
    {
        // Attack radius
        if (attackPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(
                attackPoint.position,
                attackRadius
            );
        }

        // Ground check line
        Gizmos.color = Color.blue;

        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.down * 0.2f
        );
    }
}