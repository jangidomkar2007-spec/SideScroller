using UnityEngine;
using System.Collections;

public class HitStop : MonoBehaviour
{
    public static HitStop Instance;

    [Header("Hit Stop")]
    public float defaultFreezeTime = 0.08f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Trigger(float freezeTime)
    {
        StopAllCoroutines();
        StartCoroutine(HitStopCoroutine(freezeTime));
    }

    IEnumerator HitStopCoroutine(float freezeTime)
    {
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(freezeTime);

        Time.timeScale = 1f;
    }
}