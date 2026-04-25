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
    [SerializeField] private Color pogoFlashColor = Color.cyan;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Audio")]
    [SerializeField] private AudioClip pogoSound;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    private bool canPogo = true;
    private float cooldownTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pogoSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (attackPoint == null)
        {
            GameObject obj = new GameObject("PogoAttackPoint");
            obj.transform.parent = transform;
            obj.transform.localPosition = new Vector3(0, -1f, 0);
            attackPoint = obj.transform;
        }
    }

    void Update()
    {
        if (!canPogo)
        {
            cooldownTimer -= Time.unscaledDeltaTime;
            if (cooldownTimer <= 0)
                canPogo = true;
        }

        bool pogoInput =
            (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) &&
            (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));

        if (pogoInput && canPogo && !IsGrounded())
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

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, pogoBounceForce);

            StartCoroutine(PogoFlash());

            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.SendMessage("TakeDamage", pogoDamage, SendMessageOptions.DontRequireReceiver);

                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    enemyRb.linearVelocity = new Vector2(0, 5f);
                }
            }

            SpawnPogoEffect();
            PlayPogoSound();
        }
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.2f);
        return hit.collider != null && hit.collider.CompareTag("Ground");
    }

    IEnumerator PogoFlash()
    {
        if (pogoEffectPrefab == null) yield break;

        GameObject flash = Instantiate(pogoEffectPrefab, attackPoint.position, Quaternion.identity);

        SpriteRenderer flashRenderer = flash.GetComponent<SpriteRenderer>();
        if (flashRenderer != null)
            flashRenderer.color = pogoFlashColor;

        yield return new WaitForSecondsRealtime(flashDuration);

        Destroy(flash);
    }

    void SpawnPogoEffect()
    {
        if (pogoEffectPrefab != null)
        {
            GameObject effect = Instantiate(pogoEffectPrefab, attackPoint.position, Quaternion.identity);
            Destroy(effect, 0.5f);
        }
    }

    void PlayPogoSound()
    {
        if (audioSource != null && pogoSound != null)
        {
            audioSource.PlayOneShot(pogoSound);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.2f);
    }
}