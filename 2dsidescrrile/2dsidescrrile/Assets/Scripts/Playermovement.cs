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

    [Header("Pet Sitting")]
    public Transform sitPoint;
    public Transform pet;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead = false;

    [Header("Health Restoration")]
    public int healthPickupAmount = 25;

    [Header("Fall Detection")]
    public float fallLimitY = -10f;

    [Header("Dual Daggers")]
    public GameObject rightDagger;
    public GameObject leftDagger;
    public float attackDuration = 0.2f;
    public float attackCooldown = 0.1f;
    [Header("Slide")]
    public bool slideUnlocked = false;
    public float slideDuration = 0.6f;
    private float slideTimer;

    private bool isSliding = false;


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

    private bool isSitting;
    private PetMovement2D petScript;
    private bool wasGrounded;
    private bool isAttacking;
    private bool facingRight = true;
    Animator animator;  
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponents<Collider2D>();

        petScript = pet.GetComponent<PetMovement2D>();
        currentHealth = maxHealth;

        rightDagger.SetActive(false);
        leftDagger.SetActive(false);
        //for slide
        box = GetComponent<BoxCollider2D>();
        originalSize = box.size;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (slideUnlocked && Input.GetKeyDown(KeyCode.LeftControl) && !isSliding)
        {
            StartSlide();
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0)
            {
                StopSlide();
            }
        }

        if (isDead) return;

        if (doubleJumpActive)
        {
            doubleJumpTimer -= Time.deltaTime;

            if (doubleJumpTimer <= 0)
            {
                canDoubleJump = false;
                doubleJumpActive = false;
                Debug.Log("Double Jump Ability Ended");
            }
        }

        if (transform.position.y < fallLimitY)
            Die();

        if (isSitting) return;

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
        if (moveInput != 0)
        {
            animator.SetBool("Walking", true);
        }

        else
        {
            animator.SetBool("Walking", false);
        }
        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }
        
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

            if (IsGrounded())

            {
                animator.SetBool("jumping", true);
            }
            else
            {
                animator.SetBool("jumping", false);
            }

            bool isGrounded = IsGrounded();

            // 🔹 TAKEOFF (jump start)
            if (wasGrounded && !isGrounded)
            {
                dustEffect.Play();
            }

            // 🔹 LANDING
            if (!wasGrounded && isGrounded)
            {
                dustEffect.Play();
            }

            wasGrounded = isGrounded;
        }

        if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            if (jumpTimer > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpTimer -= Time.deltaTime;
            }
            else
                isJumping = false;
        }

        if (Input.GetKeyUp(KeyCode.Space))
            isJumping = false;



        if (Input.GetKeyDown(KeyCode.LeftAlt) && !isDashing)
        {
            Debug.Log("Dash Triggered");

            isDashing = true;
            dashTimer = dashTime;

            StartCoroutine(DashInvincibility());

            float dir = moveInput != 0 ? Mathf.Sign(moveInput) : transform.localScale.x;
            rb.linearVelocity = new Vector2(dir * dashForce, rb.linearVelocity.y);
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
                isDashing = false;
        }

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(DaggerAttack());
        }

       
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (!isDashing && !isSitting)
        {
           float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
             if (moveInput != 0)
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
    // ── HEALTH RESTORATION ────────────────────────────────────

    public void Heal(int amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log("Healed " + amount + " HP. Current Health: " + currentHealth + "/" + maxHealth);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("HealthPickup"))
        {
            Heal(healthPickupAmount);
            Destroy(other.gameObject);
        }
    }

    // ── EXISTING SYSTEMS ──────────────────────────────────────

    IEnumerator DaggerAttack()
    {
        isAttacking = true;

        rightDagger.SetActive(true);
        leftDagger.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        rightDagger.SetActive(false);
        leftDagger.SetActive(false);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    IEnumerator DashInvincibility()
    {
        isInvincible = true;

        Debug.Log("Player is INVINCIBLE during dash");

        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);

        yield return new WaitForSeconds(invincibilityTime);

        isInvincible = false;

        Debug.Log("Dash invincibility ended");

        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);


    }

    // Slide Ability Pickup Function
    public void ActivateSlideAbility(float duration)
    {
        StartCoroutine(SlideAbilityTimer(duration));
    }

    IEnumerator SlideAbilityTimer(float duration)
    {
        slideUnlocked = true;

        Debug.Log("Slide ability activated");

        yield return new WaitForSeconds(duration);

        slideUnlocked = false;

        Debug.Log("Slide ability ended");
    }
    //Slide Functions
    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;

        box.size = new Vector2(originalSize.x, originalSize.y / 2);

        transform.rotation = Quaternion.Euler(0, 0, 35); // tilt player
    }

    void StopSlide()
    {
        isSliding = false;

        box.size = originalSize;

        transform.rotation = Quaternion.Euler(0, 0, 0); // return to normal
    }



    //Jump Functions
    void StartJump()
    {
        isJumping = true;
        jumpTimer = maxJumpTime;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
    

    void Sit()
    {
        transform.position = sitPoint.position;
        transform.SetParent(pet);

        rb.simulated = false;
        isSitting = true;

        petScript.canMove = true;
    }

    void Stand()
    {
        transform.SetParent(null);

        rb.simulated = true;
        isSitting = false;

        petScript.canMove = false;
    }

    bool IsGrounded()
    {
        return Physics2D.BoxCast(
            transform.position,
            boxSize,
            0f,
            Vector2.down,
            castDistance,
            groundLayer
        );
    }

    public void TakeDamage(int damage, float hitStopDuration = 0.06f)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        HitStop.Instance.Trigger(hitStopDuration);

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        spriteRenderer.enabled = false;

        foreach (Collider2D col in colliders)
            col.enabled = false;

        Invoke(nameof(ReloadScene), 1.2f);
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.CompareTag("Obstacle"))
        {
            TakeDamage(20);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            transform.position + Vector3.down * castDistance,
            boxSize
        );
    }

    public void UnlockDoubleJump()
    {
        canDoubleJump = true;
        doubleJumpActive = true;
        doubleJumpTimer = doubleJumpDuration;

        Debug.Log("Double Jump Ability Activated");
    }
    void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        if(IsGrounded())
            {
            dustEffect.Play();
        }
    }

}