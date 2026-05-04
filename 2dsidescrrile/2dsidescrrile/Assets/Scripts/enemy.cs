using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Death")]
    public float destroyDelay = 0.3f;

    [Header("Hit Stop")]
    public float hitStopIntensity = 0.06f;
    public GameObject hitParticle;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // INTEGRATION: called by PogoStrike via SendMessage("TakeDamage", pogoDamage)
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (HitStop.Instance != null)
            HitStop.Instance.Trigger(hitStopIntensity);

        if (hitParticle != null)
            Instantiate(hitParticle, transform.position, Quaternion.identity);

        Debug.Log(gameObject.name + " took damage: " + damage + " | Health left: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log(gameObject.name + " died");
        Destroy(gameObject, destroyDelay);
    }
}