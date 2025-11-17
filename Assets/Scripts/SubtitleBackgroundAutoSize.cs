using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image))]
public class SubtitleBackgroundAutoSize : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    public Vector2 padding = new Vector2(30, 10);

    void LateUpdate()
    {
        if (targetText == null) return;

        RectTransform textRect = targetText.GetComponent<RectTransform>();
        RectTransform bgRect = GetComponent<RectTransform>();

        Vector2 size = textRect.sizeDelta + padding;
        bgRect.sizeDelta = size;
    }
}
