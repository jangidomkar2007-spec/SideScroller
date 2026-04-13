using UnityEngine;

public class PetMovement2D : MonoBehaviour
{
    public float speed = 5f;
    public float followDistance = 1.5f;

    public Transform player;
    public bool canMove = false;

    [Header("Auto Jump")]
    public float jumpForce = 10f;
    public float obstacleCheckDistance = 0.6f;
    public LayerMask obstacleLayer;
    public LayerMask groundLayer;

    // ================== MOUNT SETTINGS ==================
    [Header("Mount Settings")]
    public float mountRadius = 1.2f;
    public Transform mountPoint;
    public KeyCode mountKey = KeyCode.E;

    private bool isMounted = false;
    private GameObject mountedPlayer;

    private Rigidbody2D rb;
    private Vector2 movement;

    // ================== FIRE BLAST (ADDED) ==================
    [Header("Fire Blast")]
    public GameObject fireBlastPrefab;
    public Transform firePoint;

    public float maxChargeTime = 2f;
    private float currentCharge = 0f;
    private bool isCharging = false;

    public bool fireBlastUnlocked = true;
    // =======================================================

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

        if (canMove)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            float xDiff = player.position.x - transform.position.x;

            if (Mathf.Abs(xDiff) > followDistance)
                movement.x = Mathf.Sign(xDiff);
            else
                movement.x = 0f;
        }

        movement.y = 0f;

        AutoJump();
        HandleMount();

        // ================= FIRE BLAST INPUT (ADDED) =================
        if (fireBlastUnlocked)
        {
            // HOLD = charge
            if (Input.GetKey(KeyCode.F))
            {
                isCharging = true;
                currentCharge += Time.deltaTime;
                currentCharge = Mathf.Clamp(currentCharge, 0, maxChargeTime);
            }

            // RELEASE = fire
            if (Input.GetKeyUp(KeyCode.F) && isCharging)
            {
                FireBlast();
                currentCharge = 0f;
                isCharging = false;
            }
        }
        // ============================================================
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(movement.x * speed, rb.linearVelocity.y);
    }

    // ==================== AUTO JUMP ====================
    void AutoJump()
    {
        if (!IsGrounded() || movement.x == 0) return;

        Vector2 direction = new Vector2(movement.x, 0f);

        RaycastHit2D hit = Physics2D.BoxCast(
            rb.position,
            new Vector2(0.3f, 0.6f),
            0f,
            direction,
            obstacleCheckDistance,
            obstacleLayer
        );

        if (hit.collider != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // ================== GROUND CHECK ==================
    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            rb.position,
            new Vector2(0.4f, 0.1f),
            0f,
            Vector2.down,
            0.9f,
            groundLayer
        );

        return hit.collider != null;
    }

    // ==================== MOUNT SYSTEM ====================
    void HandleMount()
    {
        if (player == null) return;

        float distance = Vector2.Distance(player.position, transform.position);

        if (!isMounted && distance <= mountRadius && Input.GetKeyDown(mountKey))
        {
            MountPlayer();
        }
        else if (isMounted && Input.GetKeyDown(mountKey))
        {
            DismountPlayer();
        }
    }

    void MountPlayer()
    {
        isMounted = true;
        mountedPlayer = player.gameObject;

        PlayerController2D pc = mountedPlayer.GetComponent<PlayerController2D>();
        if (pc != null) pc.enabled = false;

        Rigidbody2D prb = mountedPlayer.GetComponent<Rigidbody2D>();
        if (prb != null)
        {
            prb.linearVelocity = Vector2.zero;
            prb.simulated = false;
        }

        Collider2D pcol = mountedPlayer.GetComponent<Collider2D>();
        if (pcol != null) pcol.enabled = false;

        mountedPlayer.transform.SetParent(transform);
        mountedPlayer.transform.position = mountPoint.position;
        mountedPlayer.transform.localRotation = Quaternion.identity;

        canMove = true;

        Debug.Log("Player Mounted on Companion!");
    }

    void DismountPlayer()
    {
        isMounted = false;

        mountedPlayer.transform.SetParent(null);

        Rigidbody2D prb = mountedPlayer.GetComponent<Rigidbody2D>();
        if (prb != null) prb.simulated = true;

        Collider2D pcol = mountedPlayer.GetComponent<Collider2D>();
        if (pcol != null) pcol.enabled = true;

        PlayerController2D pc = mountedPlayer.GetComponent<PlayerController2D>();
        if (pc != null) pc.enabled = true;

        mountedPlayer.transform.position =
            transform.position + Vector3.right * 0.8f;

        canMove = false;
        mountedPlayer = null;

        Debug.Log("Player Dismounted from Companion!");
    }

    // ================= FIRE FUNCTION (ADDED) =================
    void FireBlast()
    {
        if (fireBlastPrefab == null || firePoint == null) return;

        GameObject blast = Instantiate(fireBlastPrefab, firePoint.position, Quaternion.identity);

        FireBlast fb = blast.GetComponent<FireBlast>();

        float chargePercent = currentCharge / maxChargeTime;

        fb.damage = Mathf.RoundToInt(20 + (40 * chargePercent));
        fb.speed = 8f + (5f * chargePercent);

        Vector2 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        fb.SetDirection(dir);
    }
    // =======================================================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mountRadius);
    }
}