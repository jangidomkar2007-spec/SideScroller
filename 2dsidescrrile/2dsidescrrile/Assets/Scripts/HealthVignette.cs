using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HealthVignette : MonoBehaviour
{
    [Header("References")]
    public Volume globalVolume;
    public PlayerController2D playerHealth;

    [Header("Settings")]
    public float threshold = 0.75f;
    public float pulseSpeed = 2f;
    public float maxIntensity = 0.5f;

    private Vignette vignette;

    void Start()
    {
        globalVolume.profile.TryGet(out vignette);
        vignette.color.value = Color.red;
        vignette.intensity.value = 0f;
    }

    void Update()
    {
        float healthPercent = (float)playerHealth.currentHealth / playerHealth.maxHealth;

        if (healthPercent < threshold)
        {
            float pulse = Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed));
            float dangerLevel = 1f - (healthPercent / threshold);

            vignette.intensity.value = pulse * maxIntensity * dangerLevel;
        }
        else
        {
            vignette.intensity.value = 0f;
        }
    }
}