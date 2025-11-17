using UnityEngine;
using UnityEngine.EventSystems;

public class UIZoomController : MonoBehaviour, IDragHandler, IScrollHandler
{
    [Header("Zoom Settings")]
    public float minZoom = 0.8f;
    public float maxZoom = 2.5f;
    public float zoomSpeed = 0.1f;

    private RectTransform rt;
    private float currentZoom = 1f;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Mobile Pinch Zoom
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float prevMag = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
            float currentMag = (t0.position - t1.position).magnitude;

            float diff = (currentMag - prevMag) * 0.005f;

            Zoom(diff);
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        // Scroll wheel zoom (desktop)
        Zoom(eventData.scrollDelta.y * 0.1f);
    }

    void Zoom(float increment)
    {
        currentZoom = Mathf.Clamp(currentZoom + increment, minZoom, maxZoom);
        rt.localScale = Vector3.one * currentZoom;
        KeepInsideParentBounds();   // <- penting!

    }

    public void OnDrag(PointerEventData eventData)
    {
        rt.anchoredPosition += eventData.delta;
        KeepInsideParentBounds();

    }

    void KeepInsideParentBounds()
    {
        if (rt == null) rt = GetComponent<RectTransform>();
        RectTransform parent = rt.parent as RectTransform;
        if (parent == null) return;

        Vector2 parentSize = parent.rect.size;
        Vector2 scaledSize = rt.rect.size * rt.localScale.x; // ukuran child setelah di-zoom

        Vector2 min = new Vector2(-parentSize.x * parent.pivot.x + scaledSize.x * rt.pivot.x,
                                  -parentSize.y * parent.pivot.y + scaledSize.y * rt.pivot.y);

        Vector2 max = new Vector2(parentSize.x * (1 - parent.pivot.x) - scaledSize.x * (1 - rt.pivot.x),
                                  parentSize.y * (1 - parent.pivot.y) - scaledSize.y * (1 - rt.pivot.y));

        Vector2 pos = rt.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.y = Mathf.Clamp(pos.y, min.y, max.y);

        rt.anchoredPosition = pos;
    }

}
