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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (HitStop.Instance != null)
        {
            HitStop.Instance.Trigger(hitStopIntensity);
        }

        Vector3 pos = transform.position;

        if (hitParticle != null)
        {
            Instantiate(hitParticle, pos, Quaternion.identity);
        }

        Debug.Log(gameObject.name + " took damage: " + damage +
                  " | Health left: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " died");

        Destroy(gameObject, destroyDelay);
    }
}