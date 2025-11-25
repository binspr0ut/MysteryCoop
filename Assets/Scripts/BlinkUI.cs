using UnityEngine;
using UnityEngine.UI;

public class BlinkUI : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 1f;

    private Image img;
    private bool isBlinking = true;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    void Update()
    {
        if (!isBlinking) return;

        float a = Mathf.Lerp(minAlpha, maxAlpha,
            (Mathf.Sin(Time.time * speed) + 1f) / 2f);

        var c = img.color;
        c.a = a;
        img.color = c;
    }

    public void StopBlink()
    {
        isBlinking = false;

        LeanTween.value(gameObject, img.color.a, 0f, 0.25f)
            .setOnUpdate((float val) =>
            {
                var c = img.color;
                c.a = val;
                img.color = c;
            })
            .setEaseOutQuad();
    }
}
