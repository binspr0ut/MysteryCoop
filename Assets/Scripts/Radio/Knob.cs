using UnityEngine;
using UnityEngine.EventSystems;

public class RadioKnob : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private RectTransform knob;            // knob yang diputar
    [SerializeField] private RectTransform pointer;         // jarum merah (bergerak di sumbu X)
    [SerializeField] private RectTransform pointerTrackLeft;  // batas kiri jarum
    [SerializeField] private RectTransform pointerTrackRight; // batas kanan jarum
    [SerializeField] private AudioSource correctAudio;
    [SerializeField] private AudioSource wrongAudio;

    [Header("Knob Limits")]
    [SerializeField] private float minAngle = -90f;
    [SerializeField] private float maxAngle = 90f;

    [Header("Tuning Settings")]
    [Range(0f, 1f)] public float targetNormalized = 0.5f;  // posisi target (0..1) di track jarum
    public float tolerancePixels = 20f;                    // seberapa dekat dianggap benar
    public float audioFadeSpeed = 0.2f;

    private float currentAngle;
    private float pointerMinX, pointerMaxX, pointerBaseY;
    private bool isDragging;

    void Start()
    {
        // Hitung batas X pointer
        pointerBaseY = pointer.anchoredPosition.y;
        pointerMinX = WorldToLocalX(pointer.parent as RectTransform, pointerTrackLeft.position);
        pointerMaxX = WorldToLocalX(pointer.parent as RectTransform, pointerTrackRight.position);

        // Siapkan audio
        correctAudio.loop = true;
        wrongAudio.loop = true;
        correctAudio.Play();
        wrongAudio.Play();
        correctAudio.volume = 0f;
        wrongAudio.volume = 1f;
    }

    public void OnPointerDown(PointerEventData e) => isDragging = true;
    public void OnPointerUp(PointerEventData e) => isDragging = false;

    public void OnDrag(PointerEventData e)
    {
        if (!isDragging) return;

        // Hitung arah dari tengah knob ke posisi pointer
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(e.pressEventCamera, knob.position);
        Vector2 dir = e.position - screenPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        // Clamp rotasi agar tidak lewat batas
        currentAngle = Mathf.Clamp(angle, minAngle, maxAngle);
        knob.localRotation = Quaternion.Euler(0, 0, currentAngle);

        UpdatePointerAndAudio();
    }

    private void UpdatePointerAndAudio()
    {
        // --- geser pointer horizontal sesuai rotasi knob ---
        float t = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);
        float pointerX = Mathf.Lerp(pointerMinX, pointerMaxX, t);
        pointer.anchoredPosition = new Vector2(pointerX, pointerBaseY);

        // --- hitung jarak ke target ---
        float targetX = Mathf.Lerp(pointerMinX, pointerMaxX, targetNormalized);
        float dist = Mathf.Abs(pointerX - targetX);

        // --- volume audio ---
        float correctVol = Mathf.Clamp01(1f - (dist / tolerancePixels));
        float wrongVol = 1f - correctVol;

        correctAudio.volume = Mathf.Lerp(correctAudio.volume, correctVol, audioFadeSpeed);
        wrongAudio.volume = Mathf.Lerp(wrongAudio.volume, wrongVol, audioFadeSpeed);
    }

    private float WorldToLocalX(RectTransform parent, Vector3 worldPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out Vector2 local);
        return local.x;
    }
}
