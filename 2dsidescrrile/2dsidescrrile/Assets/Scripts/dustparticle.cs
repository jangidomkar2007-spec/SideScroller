using UnityEngine;

public class PlayerDust : MonoBehaviour
{
    public ParticleSystem dust;
    public Rigidbody2D rb;
    public bool isGrounded;

    void Update()
    {
        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f && isGrounded)
        {
            if (!dust.isPlaying)
                dust.Play();
        }
        else
        {
            if (dust.isPlaying)
                dust.Stop();
        }
    }
}