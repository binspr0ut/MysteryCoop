using UnityEngine;
using UnityEngine.EventSystems;

public class ClockPuzzle : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform hourArrow;
    [SerializeField] private RectTransform minuteArrow;
    [SerializeField] private Clock parentClock;

    [Header("Puzzle Target (deg)")]
    public float targetHourAngle = 100f;    // target 3:20
    public float targetMinuteAngle = 120f;
    public float tolerance = 5f;            // toleransi sukses (5 derajat)

    private RectTransform currentHand;
    private bool isDragging = false;
    private Vector2 pivotScreenPos;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Tentukan apakah yang diklik adalah hour atau minute arrow
        var clickedObj = eventData.pointerPressRaycast.gameObject;
        if (clickedObj == null) return;

        if (clickedObj.name.Contains("Hour"))
            currentHand = hourArrow;
        else if (clickedObj.name.Contains("Minute"))
            currentHand = minuteArrow;
        else
            return;

        // Ambil posisi pivot untuk perhitungan sudut
        pivotScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, currentHand.position);
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || currentHand == null) return;

        Vector2 dir = eventData.position - pivotScreenPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Rotasi jarum (pivot di pangkal)
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
            parentClock?.ClosePuzzle();
        }
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180) angle -= 360;
        return angle;
    }
}
