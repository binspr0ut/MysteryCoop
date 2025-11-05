using UnityEngine;
using UnityEngine.EventSystems;

public class RadioPuzzle : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI References")]
    [SerializeField] private RectTransform knob;
    [SerializeField] private RectTransform needleIndicator;

    [Header("Audio")]
    [SerializeField] private AudioSource correctSound;
    [SerializeField] private AudioSource noiseSound;

    [Header("Settings")]
    [Range(-360, 360)] public float targetAngle = -150f; // boleh negatif
    public float tolerance = 5f;
    public float minAngle = -300f;
    public float maxAngle = 0f;

    private bool isDragging = false;
    private float currentAngle;
    private float startAngle;
    private Vector2 startPointerPos;
    private float initialNeedleY;

    void Start()
    {
        if (needleIndicator != null)
            initialNeedleY = needleIndicator.anchoredPosition.y;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        startPointerPos = eventData.position;
        startAngle = currentAngle;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // Ambil delta horizontal
        float deltaX = eventData.position.x - startPointerPos.x;

        // Semakin besar rotationSpeed, semakin sensitif
        float rotationSpeed = 0.5f;

        // arah dibalik (-deltaX) agar drag kanan = rotasi searah jarum jam
        currentAngle = Mathf.Clamp(startAngle - deltaX * rotationSpeed, minAngle, maxAngle);

        // Set rotasi knob (RectTransform positif = CCW, jadi tetap pakai currentAngle)
        knob.localEulerAngles = new Vector3(0, 0, currentAngle);

        UpdateRadioFeedback();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        CheckSolved();
    }

    private void UpdateRadioFeedback()
    {
        // --- 1️⃣ Hitung posisi relatif knob terhadap rentang min–max
        float normalized = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);

        // Karena knob berputar clockwise (arah sebaliknya dari nilai euler Unity),
        // kita balik nilai normalized agar kiri = minAngle, kanan = maxAngle
        normalized = 1f - Mathf.Clamp01(normalized);

        // --- 2️⃣ Update posisi jarum kiri → kanan
        if (needleIndicator != null)
        {
            float moveRange = 70f;
            float xPos = Mathf.Lerp(-moveRange / 2f, moveRange / 2f, normalized);
            needleIndicator.anchoredPosition = new Vector2(xPos, initialNeedleY);
        }

        // --- 3️⃣ Hitung jarak terhadap frekuensi target (untuk suara)
        float distance = Mathf.Abs(currentAngle - targetAngle);
        float proximity = Mathf.InverseLerp(90f, 0f, distance);

        // --- 4️⃣ Update volume
        correctSound.volume = Mathf.Lerp(0.0f, 1.0f, proximity);
        noiseSound.volume = Mathf.Lerp(1.0f, 0.2f, proximity);
    }


    private void CheckSolved()
    {
        if (Mathf.Abs(currentAngle - targetAngle) <= tolerance)
        {
            Debug.Log("✅ Correct frequency tuned!");
            correctSound.volume = 1f;
            noiseSound.volume = 0f;
        }
    }
}
