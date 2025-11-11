using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class RandomBlinkLight2D : MonoBehaviour
{
    [Header("Blink Settings")]
    [Tooltip("Rentang intensitas minimum dan maksimum.")]
    public Vector2 intensityRange = new Vector2(0.5f, 2.5f);

    [Tooltip("Waktu minimum dan maksimum antar kedipan (detik).")]
    public Vector2 blinkIntervalRange = new Vector2(0.1f, 1.0f);

    [Tooltip("Apakah lampu akan memudar perlahan atau langsung berubah.")]
    public bool smoothTransition = true;

    [Tooltip("Kecepatan transisi jika smoothTransition aktif.")]
    public float smoothSpeed = 5f;

    private Light2D light2D;
    private float targetIntensity;
    private float timer;

    void Start()
    {
        light2D = GetComponent<Light2D>();
        PickNewTarget();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            PickNewTarget();
        }

        if (smoothTransition)
        {
            light2D.intensity = Mathf.Lerp(light2D.intensity, targetIntensity, Time.deltaTime * smoothSpeed);
        }
        else
        {
            light2D.intensity = targetIntensity;
        }
    }

    void PickNewTarget()
    {
        targetIntensity = Random.Range(intensityRange.x, intensityRange.y);
        timer = Random.Range(blinkIntervalRange.x, blinkIntervalRange.y);
    }
}
