using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LockpickUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private RectTransform lockpick;
    [SerializeField] private RectTransform[] pins;
    [SerializeField] private Image[] pinImages;          // 🔹 NEW: gambar pin (untuk ubah warna)
    [SerializeField] private float[] shearHeights;       // tinggi shear line per pin
    [SerializeField] private float liftLimit = 150f;
    [SerializeField] private float snapThreshold = 80f;
    [SerializeField] private float shearTolerance = 12f; // 🔹 jarak toleransi untuk efek warna
    [SerializeField] private float colorLerpSpeed = 8f;  // 🔹 kecepatan transisi warna

    private RectTransform canvasRect;
    private bool isDragging;
    private Vector2 startMousePos;
    private Vector2 startLockpickPos;
    private int currentPinIndex = -1;

    private Vector2[] pinStartPos;
    private bool[] pinUnlocked;

    void Start()
    {
        canvasRect = GetComponent<RectTransform>();
        pinStartPos = new Vector2[pins.Length];
        pinUnlocked = new bool[pins.Length];

        for (int i = 0; i < pins.Length; i++)
            pinStartPos[i] = pins[i].anchoredPosition;

        // pastikan shearHeights sama panjang
        if (shearHeights.Length != pins.Length)
        {
            shearHeights = new float[pins.Length];
            for (int i = 0; i < pins.Length; i++)
                shearHeights[i] = 100f;
        }

        // pastikan langsung sejajar ke Pin1
        if (pins.Length > 0)
        {
            Vector2 lp = lockpick.anchoredPosition;
            lp.x = pins[0].anchoredPosition.x;
            lockpick.anchoredPosition = lp;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out startMousePos);
        startLockpickPos = lockpick.anchoredPosition;
        currentPinIndex = FindNearestPin(lockpick.anchoredPosition.x);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out localPoint);
        Vector2 delta = localPoint - startMousePos;
        Vector2 newPos = startLockpickPos + delta;

        // === Horizontal Snap ===
        float closestX = SnapToPinX(newPos.x);
        newPos.x = closestX;
        lockpick.anchoredPosition = newPos;

        // === Tentukan pin aktif ===
        currentPinIndex = FindNearestPin(lockpick.anchoredPosition.x);
        if (currentPinIndex < 0) return;

        // === Vertikal: angkat pin ===
        if (!pinUnlocked[currentPinIndex])
        {
            float lift = Mathf.Clamp(newPos.y - startLockpickPos.y, 0, liftLimit);
            Vector2 pinPos = pinStartPos[currentPinIndex];
            pins[currentPinIndex].anchoredPosition = new Vector2(pinPos.x, pinPos.y + lift);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentPinIndex < 0) { isDragging = false; return; }

        Vector2 pinPos = pins[currentPinIndex].anchoredPosition;
        float lifted = pinPos.y - pinStartPos[currentPinIndex].y;

        if (Mathf.Abs(lifted - shearHeights[currentPinIndex]) <= shearTolerance)
        {
            // ✅ Pin unlocked
            pinUnlocked[currentPinIndex] = true;
            pins[currentPinIndex].anchoredPosition = new Vector2(pinStartPos[currentPinIndex].x, pinStartPos[currentPinIndex].y + shearHeights[currentPinIndex]);
            Debug.Log($"Pin {currentPinIndex + 1} unlocked!");
        }
        else
        {
            // ❌ Belum pas
            pins[currentPinIndex].anchoredPosition = pinStartPos[currentPinIndex];
        }

        currentPinIndex = -1;
        isDragging = false;
        CheckIfAllUnlocked();
    }

    void Update()
    {
        // === Color feedback ===
        for (int i = 0; i < pins.Length; i++)
        {
            if (pinImages == null || i >= pinImages.Length) continue;

            float lifted = pins[i].anchoredPosition.y - pinStartPos[i].y;
            float distance = Mathf.Abs(lifted - shearHeights[i]);

            // Jika dekat shear line → jadi hijau, kalau jauh → biru
            Color targetColor = distance <= shearTolerance ? Color.green : Color.blue;

            // Smooth transition
            pinImages[i].color = Color.Lerp(pinImages[i].color, targetColor, Time.deltaTime * colorLerpSpeed);
        }
    }

    private float SnapToPinX(float x)
    {
        float minDist = float.MaxValue;
        float closestX = x;

        foreach (var pin in pins)
        {
            float dist = Mathf.Abs(pin.anchoredPosition.x - x);
            if (dist < minDist && dist < snapThreshold)
            {
                minDist = dist;
                closestX = pin.anchoredPosition.x;
            }
        }
        return closestX;
    }

    private int FindNearestPin(float x)
    {
        float minDist = float.MaxValue;
        int index = -1;
        for (int i = 0; i < pins.Length; i++)
        {
            float dist = Mathf.Abs(pins[i].anchoredPosition.x - x);
            if (dist < minDist && dist < snapThreshold)
            {
                minDist = dist;
                index = i;
            }
        }
        return index;
    }

    private void CheckIfAllUnlocked()
    {
        foreach (bool unlocked in pinUnlocked)
        {
            if (!unlocked) return;
        }

        Debug.Log("✅ All pins unlocked! Lock opened!");
        // bisa tambahkan: shelfLockpick.ClosePuzzle();
    }
}
