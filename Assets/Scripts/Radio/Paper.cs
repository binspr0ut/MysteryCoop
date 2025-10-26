using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Paper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RawImage drawSurface;
    [SerializeField] private Color drawColor = Color.black;
    [SerializeField] private float brushSize = 5;

    private Texture2D texture;
    private RectTransform rectTransform;
    private bool isDrawing = false;
    int scaleMultiplier = 4; // 🔥 naikkan resolusi 4x


    void Start()
    {
        rectTransform = drawSurface.GetComponent<RectTransform>();

        int scaleMultiplier = 4; // 🔥 naikkan resolusi 4x
        int width = (int)(rectTransform.rect.width * scaleMultiplier);
        int height = (int)(rectTransform.rect.height * scaleMultiplier);

        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;     // sedikit halus tapi tetap tajam
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        // isi putih
        Color[] fillColor = new Color[width * height];
        for (int i = 0; i < fillColor.Length; i++)
            fillColor[i] = Color.white;

        texture.SetPixels(fillColor);
        texture.Apply();

        drawSurface.texture = texture;
    }



    public void OnPointerDown(PointerEventData eventData)
    {
        isDrawing = true;
        DrawAt(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrawing = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDrawing)
            DrawAt(eventData);
    }

    private void DrawAt(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            float x = (localPoint.x + rectTransform.rect.width / 2) * scaleMultiplier;
            float y = (localPoint.y + rectTransform.rect.height / 2) * scaleMultiplier;


            for (int i = (int)-brushSize; i <= brushSize; i++)
            {
                for (int j = (int)-brushSize; j <= brushSize; j++)
                {
                    int px = Mathf.RoundToInt(x + i);
                    int py = Mathf.RoundToInt(y + j);

                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                        texture.SetPixel(px, py, drawColor);
                }
            }

            texture.Apply();
        }
    }

    public void ClearCanvas()
    {
        if (texture == null) return;

        // isi ulang texture jadi putih lagi
        Color[] clearColor = new Color[texture.width * texture.height];
        for (int i = 0; i < clearColor.Length; i++)
            clearColor[i] = Color.white;

        texture.SetPixels(clearColor);
        texture.Apply();

        Debug.Log("🧹 Canvas cleared!");
    }

}
