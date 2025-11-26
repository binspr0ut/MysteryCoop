using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BlinkingSprite : MonoBehaviour
{
    [Header("Blink Settings")]
    [Tooltip("Rentang alpha minimum dan maksimum (0 = transparan, 1 = penuh).")]
    public Vector2 alphaRange = new Vector2(0.3f, 1.0f);

    [Tooltip("Waktu minimum dan maksimum antar kedipan.")]
    public Vector2 blinkIntervalRange = new Vector2(0.1f, 1.0f);

    [Tooltip("Apakah transisi akan halus (Lerp).")]
    public bool smoothTransition = true;

    [Tooltip("Kecepatan transisi jika smooth.")]
    public float smoothSpeed = 5f;

    public bool isStair = false;

    private SpriteRenderer sprite;
    private float targetAlpha;
    private float timer;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        PickNewTarget();
    }

    void Update()
    {
        if (isStair)
        {
            if (StairDownTrigger.instance.counter > 0)
            {
                sprite.enabled = false;
                return;
            }
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
            PickNewTarget();

        Color c = sprite.color;

        if (smoothTransition)
        {
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * smoothSpeed);
        }
        else
        {
            c.a = targetAlpha;
        }

        sprite.color = c;
    }

    void PickNewTarget()
    {
        targetAlpha = Random.Range(alphaRange.x, alphaRange.y);
        timer = Random.Range(blinkIntervalRange.x, blinkIntervalRange.y);
    }
}
