using UnityEngine;
using UnityEngine.UI;

public class UIButtonGlow : MonoBehaviour
{
    [SerializeField] private Outline outline;
    [SerializeField] private float glowSpeed = 2f;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1f;

    private Color baseColor;
    private bool fadingOut = false;

    void Start()
    {
        if (outline == null)
            outline = GetComponent<Outline>();

        baseColor = outline.effectColor;
    }

    void Update()
    {
        float alpha = outline.effectColor.a;
        float targetAlpha = fadingOut ? minAlpha : maxAlpha;
        alpha = Mathf.MoveTowards(alpha, targetAlpha, Time.deltaTime * glowSpeed);

        outline.effectColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

        if (Mathf.Approximately(alpha, targetAlpha))
            fadingOut = !fadingOut;
    }
}
