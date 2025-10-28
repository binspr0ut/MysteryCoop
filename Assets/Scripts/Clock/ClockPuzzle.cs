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
    [SerializeField] private AudioSource alarmSound;

    private RectTransform currentHand;
    private bool isDragging = false;
    private Vector2 pivotScreenPos;

    private void Awake()
    {
        // fallback: otomatis ambil AudioSource di anak
        if (alarmSound == null)
            alarmSound = GetComponentInChildren<AudioSource>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
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

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || currentHand == null) return;

        Vector2 dir = eventData.position - pivotScreenPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        currentHand.localEulerAngles = new Vector3(0, 0, angle - 90);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        CheckPuzzleSolved();
    }

    private void CheckPuzzleSolved()
    {
        float hourZ = Mathf.Abs(NormalizeAngle(hourArrow.localEulerAngles.z));
        float minuteZ = Mathf.Abs(NormalizeAngle(minuteArrow.localEulerAngles.z));

        if (Mathf.Abs(hourZ - targetHourAngle) <= tolerance &&
            Mathf.Abs(minuteZ - targetMinuteAngle) <= tolerance)
        {
            Debug.Log("✅ Puzzle solved! Time = 3:20");

            // 🔊 Mainkan suara alarm
            if (alarmSound != null)
                alarmSound.Play();
            else
                Debug.LogWarning("⚠️ Alarm AudioSource belum diset atau tidak ditemukan.");

            // parentClock?.Unpossess();
        }
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180) angle -= 360;
        return angle;
    }
}
