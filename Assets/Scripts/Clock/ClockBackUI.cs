using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ClockBackUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Puzzle Elements")]
    [SerializeField] private RectTransform battery;
    [SerializeField] private RectTransform clockTargetZone;
    [SerializeField] private float snapDistance = 80f;

    public event Action onPuzzleDone;

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private bool _puzzleDone = false;

    private void Start()
    {
        _rectTransform = battery.GetComponent<RectTransform>();
        _canvasGroup = battery.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = battery.gameObject.AddComponent<CanvasGroup>();

        _canvas = GetComponentInParent<Canvas>();
        if (_canvas == null)
            Debug.LogError("❌ ClockBackUI: Canvas parent tidak ditemukan!");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_puzzleDone) return;
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_puzzleDone) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            _canvas.worldCamera,
            out Vector2 localPos))
        {
            _rectTransform.anchoredPosition = localPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_puzzleDone) return;
        _canvasGroup.blocksRaycasts = true;

        float distance = Vector2.Distance(_rectTransform.anchoredPosition, clockTargetZone.anchoredPosition);
        Debug.Log($"📏 Distance: {distance}");

        if (distance <= snapDistance)
            SnapBattery();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (_puzzleDone) return;
        var dropped = eventData.pointerDrag;
        if (dropped != null && dropped == battery.gameObject)
            SnapBattery();
    }

    private void SnapBattery()
    {
        _rectTransform.anchoredPosition = clockTargetZone.anchoredPosition;
        _puzzleDone = true;
        Debug.Log("✅ Battery inserted!");
        onPuzzleDone?.Invoke();
    }
}
