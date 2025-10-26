using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ClockBackUI : MonoBehaviour, IDragHandler, IPointerUpHandler
{
    [Header("Puzzle Elements")]
    public RectTransform battery;
    public RectTransform clockTargetZone; // area jam tempat battery harus diletakkan
    public float snapDistance = 80f;

    public event Action onPuzzleDone;

    private Canvas canvas;
    private bool puzzleDone = false;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (puzzleDone) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPos
        );

        battery.anchoredPosition = localPos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (puzzleDone) return;

        float distance = Vector2.Distance(battery.anchoredPosition, clockTargetZone.anchoredPosition);
        if (distance <= snapDistance)
        {
            battery.anchoredPosition = clockTargetZone.anchoredPosition;
            Debug.Log("🔋 Battery inserted into Clock!");
            puzzleDone = true;
            onPuzzleDone?.Invoke();
        }
    }
}
