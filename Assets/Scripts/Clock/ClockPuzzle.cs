using UnityEngine;
using UnityEngine.EventSystems;

public class ClockPuzzle : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private RectTransform hourArrow;
    [SerializeField] private RectTransform minuteArrow;
    [SerializeField] private Clock parentClock;

    [Header("Puzzle Target (deg)")]
    [SerializeField] private float targetHourAngle = 100f;
    [SerializeField] private float targetMinuteAngle = 120f;
    [SerializeField] private float tolerance = 5f;

    [Header("Audio")]
    public AudioSource alarmSound;

    [Header("SFX")]
    [SerializeField] private AudioClip arrowMoveSFX;


    private RectTransform currentHand;
    private bool isDragging = false;
    private Vector2 pivotScreenPos;

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip);
    }

    private void Awake()
    {
        // fallback: otomatis ambil AudioSource dari anak
        if (alarmSound == null)
            alarmSound = GetComponentInChildren<AudioSource>();
    }

    // =====================================================================
    // CLICK DOWN: tentukan jam mana yang di-drag (hour atau minute)
    // =====================================================================
    public void OnPointerDown(PointerEventData eventData)
    {

        // 🔊 SFX: mulai menggerakkan jarum jam
        PlaySFX(arrowMoveSFX);

        var clickedObj = eventData.pointerPressRaycast.gameObject;
        if (clickedObj == null) return;

        if (clickedObj.name.Contains("Hour"))
            currentHand = hourArrow;
        else if (clickedObj.name.Contains("Minute"))
            currentHand = minuteArrow;
        else
            return;

        pivotScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, currentHand.position);
        isDragging = true;
    }

    // =====================================================================
    // DRAG: putar jarum berdasarkan posisi mouse
    // =====================================================================
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || currentHand == null) return;

        Vector2 dir = eventData.position - pivotScreenPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        currentHand.localEulerAngles = new Vector3(0, 0, angle - 90);

        // ✅ real-time checking saat drag
        CheckPuzzleState();
    }

    // =====================================================================
    // RELEASE: cek posisi terakhir ketika mouse dilepas
    // =====================================================================
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        CheckPuzzleState();
    }

    // =====================================================================
    // CONTINUOUS CHECKER
    // Jika benar → alarm ON
    // Jika keluar dari toleransi → alarm OFF
    // =====================================================================
    private void CheckPuzzleState()
    {
        if (parentClock != null && parentClock.CurrentState == ObjectState.Locked)
            return;

        float hourZ = NormalizeAngle(hourArrow.localEulerAngles.z);
        float minuteZ = NormalizeAngle(minuteArrow.localEulerAngles.z);

        float targetHourNorm = NormalizeAngle(targetHourAngle);
        float targetMinuteNorm = NormalizeAngle(targetMinuteAngle);

        bool hourCorrect = Mathf.Abs(hourZ - targetHourNorm) <= tolerance;
        bool minuteCorrect = Mathf.Abs(minuteZ - targetMinuteNorm) <= tolerance;

        bool isCorrect = hourCorrect && minuteCorrect;

        if (isCorrect)
        {
            // ✅ Nyalakan alarm (hanya kalau belum main)
            if (!alarmSound.isPlaying)
            {
                Debug.Log("✅ Alarm ON — posisi jam benar");
                alarmSound.volume = 1f;
                alarmSound.Play();
            }
        }
        else
        {
            // ✅ Matikan alarm (kalau sedang main)
            if (alarmSound.isPlaying)
            {
                Debug.Log("🔇 Alarm OFF — keluar dari posisi benar");
                alarmSound.Stop();
            }
        }
    }

    // =====================================================================
    // Normalize sudut 0..360 menjadi -180..180 agar mudah dicek
    // =====================================================================
    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;

    }
}
