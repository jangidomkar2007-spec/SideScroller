using UnityEngine;

public class SlideAbilityObject : MonoBehaviour
{
    public float abilityDuration = 1f;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponent<PlayerController2D>();

        if (player != null)
        {
            player.ActivateSlideAbility(abilityDuration);
            Destroy(gameObject);
        }
    }
}