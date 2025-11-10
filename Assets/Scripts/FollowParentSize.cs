using UnityEngine;

public class FollowParentSize : MonoBehaviour
{
    public RectTransform targetParent;

    void Update()
    {
        if (targetParent == null) return;

        RectTransform rect = GetComponent<RectTransform>();
        rect.sizeDelta = targetParent.sizeDelta;
    }
}
