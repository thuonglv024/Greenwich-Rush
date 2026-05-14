using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalPosition;

    private float shakeDuration = 0f;
    private float shakeStrength = 0f;

    private void Awake()
    {
        Instance = this;

        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (shakeDuration > 0)
        {
            Vector3 randomOffset = Random.insideUnitSphere * shakeStrength;

            randomOffset.z = 0f;

            transform.localPosition = originalPosition + randomOffset;

            shakeDuration -= Time.deltaTime;
        }
        else
        {
            shakeDuration = 0f;

            transform.localPosition = originalPosition;
        }
    }

    public void Shake(float duration, float strength)
    {
        shakeDuration = duration;
        shakeStrength = strength;
    }
}