using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healAmount = 25;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController2D player =
                collision.GetComponent<PlayerController2D>();

            if (player != null)
            {
                player.Heal(healAmount);

                Destroy(gameObject);
            }
        }
    }
}