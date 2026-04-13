using UnityEngine;

public class DaggerDamage : MonoBehaviour
{
    public int damage = 15;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyHealth>()?.TakeDamage(damage);
        }
    }
}