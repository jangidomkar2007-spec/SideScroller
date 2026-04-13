using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float followRadius = 5f;
    public float speed = 2f;

    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < followRadius)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                speed * Time.deltaTime
            );
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, followRadius);
    }
}