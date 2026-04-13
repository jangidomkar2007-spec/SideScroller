using UnityEngine;
using System.Collections;

public class DoubleJumpAbility : MonoBehaviour
{
    public float abilityTime = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponent<PlayerController2D>();

        if (player != null)
        {
            Debug.Log("Ability Picked");

            player.UnlockDoubleJump();

            StartCoroutine(DisableAbility(player));

            Destroy(gameObject);
        }
    }

    IEnumerator DisableAbility(PlayerController2D player)
    {
        yield return new WaitForSeconds(abilityTime);

        player.canDoubleJump = false;

        Debug.Log("Double Jump Disabled");
    }
}