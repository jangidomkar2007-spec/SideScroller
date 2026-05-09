using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated) return;

        if (collision.CompareTag("Player"))
        {
            activated = true;

            PlayerController2D player =
                collision.GetComponent<PlayerController2D>();

            if (player != null)
            {
                player.respawnPoint = transform;
            }

            Debug.Log("Checkpoint Activated!");
        }
    }
}