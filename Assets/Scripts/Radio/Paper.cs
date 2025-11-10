using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Paper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RawImage drawSurface;
    [SerializeField] private Color drawColor = Color.black;
    [SerializeField] private float brushSize = 8f;

    private Texture2D texture;
    private RectTransform rectTransform;
    private bool isDrawing = false;
    private int scaleMultiplier = 4;

    private Vector2 lastPos;     // to smooth strokes

    void Start()
    {
        rectTransform = drawSurface.GetComponent<RectTransform>();

        int width = (int)(rectTransform.rect.width * scaleMultiplier);
        int height = (int)(rectTransform.rect.height * scaleMultiplier);

        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        ClearCanvas();
        drawSurface.texture = texture;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDrawing = true;
        if (TryGetLocalPosition(eventData, out Vector2 pos))
        {
            lastPos = pos;
            DrawCircle(pos);
            texture.Apply();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrawing = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDrawing) return;

        if (TryGetLocalPosition(eventData, out Vector2 pos))
        {
            DrawLine(lastPos, pos); // smooth connect
            lastPos = pos;
            texture.Apply();
        }
    }

    private bool TryGetLocalPosition(PointerEventData eventData, out Vector2 pos)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,
            eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            pos = new Vector2(
                (localPoint.x + rectTransform.rect.width / 2) * scaleMultiplier,
                (localPoint.y + rectTransform.rect.height / 2) * scaleMultiplier
            );
            return true;
        }

        pos = Vector2.zero;
        return false;
    }

    private void DrawCircle(Vector2 center)
    {
        int r = Mathf.RoundToInt(brushSize * scaleMultiplier);

        for (int y = -r; y <= r; y++)
        {
            for (int x = -r; x <= r; x++)
            {
                if (x * x + y * y <= r * r) // circle check
                {
                    int px = Mathf.RoundToInt(center.x + x);
                    int py = Mathf.RoundToInt(center.y + y);

                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                        texture.SetPixel(px, py, drawColor);
                }
            }
        }
    }

    private void DrawLine(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance);

        for (int i = 0; i < steps; i++)
        {
            Vector2 t = Vector2.Lerp(start, end, i / (float)steps);
            DrawCircle(t);
        }
    }

    public void ClearCanvas()
    {
        Color[] clearColor = new Color[texture.width * texture.height];
        for (int i = 0; i < clearColor.Length; i++)
            clearColor[i] = Color.white;

        texture.SetPixels(clearColor);
        texture.Apply();
    }
}
