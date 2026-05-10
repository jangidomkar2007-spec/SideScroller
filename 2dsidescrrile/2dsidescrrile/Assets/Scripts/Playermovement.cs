using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public float maxJumpTime = 0.25f;

    [Header("Double Jump Ability")]
    public bool canDoubleJump = false;
    public float doubleJumpDuration = 10f;
    private float doubleJumpTimer;
    private bool doubleJumpActive = false;
    private bool hasDoubleJumped = false;

    [Header("Ground Check")]
    public Vector2 boxSize = new Vector2(0.8f, 0.2f);
    public float castDistance = 0.1f;
    public LayerMask groundLayer;
    public ParticleSystem dustEffect;

    [Header("Dash")]
    public float dashForce = 15f;
    public float dashTime = 0.2f;

    [Header("Dash Invincibility")]
    public bool isInvincible = false;
    public float invincibilityTime = 0.25f;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead = false;

    [Header("Health Restoration")]
    public int healthPickupAmount = 25;

    [Header("Fall Detection")]
    public float fallLimitY = -10f;

    [Header("Respawn")]
    public Transform respawnPoint;
    public float respawnDelay = 1.2f;

    [Header("Attack")]
    public float attackCooldown = 0.2f;
 


    [Header("Pogo Strike")]
    [SerializeField] private float pogoBounceForce = 12f;
    [SerializeField] private int pogoDamage = 25;
    [SerializeField] private float pogoCooldown = 0.3f;
    [SerializeField] private LayerMask enemyLayer;

    [SerializeField] private Transform pogoAttackPoint;
    [SerializeField] private float pogoAttackRadius = 0.8f;

    [SerializeField] private GameObject pogoEffectPrefab;
    [SerializeField] private Color pogoHitColor = Color.cyan;
    [SerializeField] private float pogoEffectLifetime = 0.5f;

    [SerializeField] private AudioClip pogoSound;

    private AudioSource audioSource;

    private bool canPogo = true;
    private float pogoCooldownTimer;
    public bool isPogoStriking = false;

    [Header("Slide")]
    public bool slideUnlocked = false;
    public float slideForce = 14f;
    public float slideDuration = 0.35f;
    public float slideCooldown = 1f;

    private bool isSliding = false;
    private bool canSlide = true;

    private BoxCollider2D box;
    private Vector2 originalSize;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D[] colliders;
    private float moveInput;

    private bool isJumping;
    private float jumpTimer;

    private bool isDashing;
    private float dashTimer;

    private bool wasGrounded;
    private bool isAttacking;

   

    private bool facingRight = true;

    Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponents<Collider2D>();

        currentHealth = maxHealth;

        box = GetComponent<BoxCollider2D>();
        originalSize = box.size;

        animator = GetComponent<Animator>();

        if (respawnPoint == null)
        {
            GameObject spawn = new GameObject("SpawnPoint");
            spawn.transform.position = transform.position;
            respawnPoint = spawn.transform;
        }

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && pogoSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (pogoAttackPoint == null)
        {
            GameObject obj = new GameObject("PogoAttackPoint");

            obj.transform.parent = transform;

            obj.transform.localPosition =
                new Vector3(0f, -0.8f, 0f);

            pogoAttackPoint = obj.transform;
        }
    }

    void Update()
    {
        if (slideUnlocked &&
            Input.GetKeyDown(KeyCode.LeftControl) &&
            canSlide &&
            IsGrounded())
        {
            StartCoroutine(Slide());
        }

        if (isDead) return;

        if (doubleJumpActive)
        {
            doubleJumpTimer -= Time.deltaTime;

            if (doubleJumpTimer <= 0)
            {
                canDoubleJump = false;
                doubleJumpActive = false;
            }
        }

        if (transform.position.y < fallLimitY)
            Die();

        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput != 0 && IsGrounded())
        {
            if (!dustEffect.isPlaying)
                dustEffect.Play();
        }
        else
        {
            if (dustEffect.isPlaying)
                dustEffect.Stop();
        }

        if (moveInput > 0 && !facingRight)
            Flip();
        else if (moveInput < 0 && facingRight)
            Flip();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsGrounded())
            {
                StartJump();
                hasDoubleJumped = false;
            }
            else if (canDoubleJump && !hasDoubleJumped)
            {
                StartJump();
                hasDoubleJumped = true;
            }
        }

        if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            if (jumpTimer > 0)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );

                jumpTimer -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
            isJumping = false;

        if (Input.GetKeyDown(KeyCode.LeftAlt) && !isDashing)
        {
            isDashing = true;
            dashTimer = dashTime;

            StartCoroutine(DashInvincibility());

            float dir = moveInput != 0
                ? Mathf.Sign(moveInput)
                : transform.localScale.x;

            rb.linearVelocity = new Vector2(
                dir * dashForce,
                rb.linearVelocity.y
            );
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
                isDashing = false;
        }


        bool grounded = IsGrounded();

        // JUMP START
        if (wasGrounded && !grounded)
        {
            animator.SetTrigger("JumpStart");
            dustEffect.Play();
        }

        // IN AIR
        if (!grounded && rb.linearVelocity.y > 0.1f)
        {
            animator.SetBool("IsJumping", true);
            animator.SetBool("IsFalling", false);
        }

        // FALLING
        if (!grounded && rb.linearVelocity.y < -0.1f)
        {
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", true);
        }

        // LAND
        if (!wasGrounded && grounded)
        {
            animator.SetTrigger("Land");

            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);

            dustEffect.Play();
        }

        wasGrounded = grounded;

        // POGO COOLDOWN
        if (!canPogo)
        {
            pogoCooldownTimer -= Time.deltaTime;

            if (pogoCooldownTimer <= 0)
            {
                canPogo = true;
            }
        }

        // POGO INPUT
        bool pogoInput =
            (Input.GetKey(KeyCode.S) ||
             Input.GetKey(KeyCode.DownArrow)) &&
            (Input.GetKeyDown(KeyCode.Space) ||
             Input.GetMouseButtonDown(0));

        // POGO STRIKE
        if (pogoInput &&
            canPogo &&
            !IsGrounded() &&
            !isDead)
        {
            PerformPogoStrike();
        }

        if (Input.GetMouseButtonDown(0) &&
      !isAttacking &&
      !isPogoStriking)
        {
            StartCoroutine(Attack1());
        }

        if (Input.GetMouseButtonDown(1) &&
            !isAttacking &&
            !isPogoStriking)
        {
            StartCoroutine(Attack2());
        }
    }

        void FixedUpdate()
    {
        if (isDead) return;

        if (!isDashing && !isSliding)
        {
            float speed = Input.GetKey(KeyCode.LeftShift)
                ? sprintSpeed
                : moveSpeed;

            rb.linearVelocity = new Vector2(
                moveInput * speed,
                rb.linearVelocity.y
            );

            if (moveInput != 0 && IsGrounded())
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    animator.SetBool("Running", true);
                    animator.SetBool("Walking", false);
                }
                else
                {
                    animator.SetBool("Running", false);
                    animator.SetBool("Walking", true);
                }
            }
            else
            {
                animator.SetBool("Running", false);
                animator.SetBool("Walking", false);
            }
        }

    }


    void PerformPogoStrike()
    {
        isPogoStriking = true;

        Collider2D[] hitEnemies =
            Physics2D.OverlapCircleAll(
                pogoAttackPoint.position,
                pogoAttackRadius,
                enemyLayer
            );

        if (hitEnemies.Length > 0)
        {
            canPogo = false;

            pogoCooldownTimer = pogoCooldown;

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                pogoBounceForce
            );

            SpawnPogoEffect();

            PlayPogoSound();

            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.SendMessage(
                    "TakeDamage",
                    pogoDamage,
                    SendMessageOptions.DontRequireReceiver
                );

                Rigidbody2D enemyRb =
                    enemy.GetComponent<Rigidbody2D>();

                if (enemyRb != null)
                {
                    enemyRb.linearVelocity =
                        new Vector2(0, 5f);
                }
            }
        }

        StartCoroutine(ResetPogoState());
    }

    IEnumerator ResetPogoState()
    {
        yield return new WaitForSeconds(0.2f);

        isPogoStriking = false;
    }

    void SpawnPogoEffect()
    {
        if (pogoEffectPrefab == null) return;

        GameObject effect = Instantiate(
            pogoEffectPrefab,
            pogoAttackPoint.position,
            pogoAttackPoint.rotation
        );

        effect.transform.localScale = Vector3.one;

        SpriteRenderer[] sprites =
            effect.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sr in sprites)
        {
            sr.color = pogoHitColor;
        }

        ParticleSystem[] particles =
            effect.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particles)
        {
            var main = ps.main;

            main.startColor = pogoHitColor;
        }

        Destroy(effect, pogoEffectLifetime);
    }

    void PlayPogoSound()
    {
        if (audioSource != null &&
            pogoSound != null)
        {
            audioSource.PlayOneShot(pogoSound);
        }
    }


    IEnumerator Attack1()
    {
        isAttacking = true;

        animator.ResetTrigger("Attack2");
        animator.SetTrigger("Attack1");

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    IEnumerator Attack2()
    {
        isAttacking = true;

        animator.ResetTrigger("Attack1");
        animator.SetTrigger("Attack2");

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    IEnumerator Slide()
    {
        isSliding = true;
        canSlide = false;

        animator.SetBool("Sliding", true);

        float direction = facingRight ? 1f : -1f;

        rb.linearVelocity = new Vector2(
            direction * slideForce,
            rb.linearVelocity.y
        );

        yield return new WaitForSeconds(slideDuration);

        isSliding = false;

        animator.SetBool("Sliding", false);

        yield return new WaitForSeconds(slideCooldown);

        canSlide = true;
    }

    void StartJump()
    {
        isJumping = true;
        jumpTimer = maxJumpTime;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );
    }

    void Flip()
    {
        facingRight = !facingRight;

        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    public bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            transform.position,
            boxSize,
            0,
            Vector2.down,
            castDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    IEnumerator DashInvincibility()
    {
        isInvincible = true;

        yield return new WaitForSeconds(invincibilityTime);

        isInvincible = false;
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth = Mathf.Min(
            currentHealth + amount,
            maxHealth
        );
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        animator.SetTrigger("Death");

        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        rb.linearVelocity = Vector2.zero;

        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        yield return new WaitForSeconds(respawnDelay);

        transform.position = respawnPoint.position;

        currentHealth = maxHealth;
        isDead = false;

        foreach (Collider2D col in colliders)
        {
            col.enabled = true;
        }
    }
    // DOUBLE JUMP UNLOCK
    public void UnlockDoubleJump()
    {
        canDoubleJump = true;
        doubleJumpActive = true;
        doubleJumpTimer = doubleJumpDuration;
    }

    // SLIDE ABILITY UNLOCK
    public void ActivateSlideAbility(float value)
    {
        slideUnlocked = true;
    }
    public void SetCheckpoint(Vector2 checkpointPosition)
    {
        respawnPoint.position = checkpointPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(
            transform.position + Vector3.down * castDistance,
            boxSize
        );

        if (pogoAttackPoint != null)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireSphere(
                pogoAttackPoint.position,
                pogoAttackRadius
            );
        }
    }
}