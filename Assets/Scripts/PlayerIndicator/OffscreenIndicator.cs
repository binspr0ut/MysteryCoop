using UnityEngine;
using UnityEngine.UI;

public class OffscreenIndicator : MonoBehaviour
{
    [Header("Refs")]
    public Camera targetCamera;             // Jika kosong, akan pakai Camera.main
    public Canvas canvas;                   // Canvas tempat UI ditempel
    public RectTransform indicatorPrefab;   // Prefab UI Image panah
    public Transform target;                // Transform yang diikuti

    [Header("Behavior")]
    public float screenEdgePadding = 24f;   // Jarak dari tepi layar (px)
    public float worldHeightOffset = 1.2f;  // Geser titik target di atas kepala
    public bool hideWhenOnScreen = true;    // Sembunyikan saat target terlihat

    [Header("Style")]
    public Color indicatorColor = Color.white;

    // runtime
    private RectTransform indicator;
    private Image indicatorImage;
    private bool inited;     // sudah spawn prefab?

    // --- LAZY INIT: tidak ada Spawn di Awake/Start ---

    void OnEnable()
    {
        // Pastikan mulai dalam keadaan hidden (sampai siap)
        if (indicator != null) indicator.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (indicator != null) Destroy(indicator.gameObject);
    }

    private void TryInit()
    {
        if (inited) return;
        if (indicatorPrefab == null || canvas == null) return;

        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        indicator = Instantiate(indicatorPrefab, canvas.transform);
        indicator.name = $"Indicator_{(target ? target.name : "Target")}";
        indicatorImage = indicator.GetComponent<Image>();
        if (indicatorImage != null)
        {
            // pastikan alpha penuh
            var c = indicatorColor; c.a = 1f;
            indicatorImage.color = c;
        }
        indicator.gameObject.SetActive(false);
        inited = true;
    }

    void LateUpdate()
    {
        // Tunda inisialisasi sampai semua field terisi oleh binder
        if (!inited) TryInit();
        if (!inited) return;

        if (target == null) { indicator.gameObject.SetActive(false); return; }
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) { indicator.gameObject.SetActive(false); return; }

        Vector3 worldPos = target.position + Vector3.up * worldHeightOffset;

        // Cek on-screen
        Vector3 vp = targetCamera.WorldToViewportPoint(worldPos);
        bool inFront = vp.z > 0f;
        bool onScreen = inFront && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f;

        if (hideWhenOnScreen && onScreen)
        {
            if (indicator.gameObject.activeSelf)
                indicator.gameObject.SetActive(false);
            return;
        }

        if (!indicator.gameObject.activeSelf)
            indicator.gameObject.SetActive(true);

        // Hitung posisi tepi layar
        Vector2 sp = targetCamera.WorldToScreenPoint(worldPos);
        if (!inFront)
        {
            // jika di belakang kamera, balik agar arah tetap benar
            sp = new Vector2(Screen.width - sp.x, Screen.height - sp.y);
        }

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 dir = (sp - screenCenter).normalized;

        Vector2 half = screenCenter - Vector2.one * screenEdgePadding;
        float dx = Mathf.Approximately(dir.x, 0f) ? 1e-6f : Mathf.Abs(half.x / dir.x);
        float dy = Mathf.Approximately(dir.y, 0f) ? 1e-6f : Mathf.Abs(half.y / dir.y);
        float t = Mathf.Min(dx, dy);
        Vector2 edgePos = screenCenter + dir * t;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        switch (canvas.renderMode)
        {
            case RenderMode.ScreenSpaceOverlay:
                indicator.position = edgePos;
                indicator.rotation = Quaternion.Euler(0, 0, angle);
                break;

            case RenderMode.ScreenSpaceCamera:
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform, edgePos, canvas.worldCamera, out Vector2 lp);
                indicator.anchoredPosition = lp;
                indicator.localRotation = Quaternion.Euler(0, 0, angle);
                break;

            case RenderMode.WorldSpace:
                RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    canvas.transform as RectTransform, edgePos, targetCamera, out Vector3 wp);
                indicator.position = wp;
                indicator.rotation = Quaternion.Euler(0, 0, angle);
                break;
        }
    }
}