using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ClockBackUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Battery Items")]
    [SerializeField] private RectTransform battery1;
    [SerializeField] private RectTransform battery2;

    [Header("Drop Targets")]
    [SerializeField] private RectTransform target1;
    [SerializeField] private RectTransform target2;

    [Header("Settings")]
    [SerializeField] private float snapDistance = 80f;

    public event Action onPuzzleDone;

    private Canvas _canvas;

    private RectTransform _currentBattery;
    private CanvasGroup _currentCanvasGroup;

    private bool battery1Snapped = false;
    private bool battery2Snapped = false;


    private void Start()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas == null)
            Debug.LogError("❌ ClockBackUI: Canvas parent tidak ditemukan!");

        // Tambahkan CanvasGroup ke masing-masing battery jika belum ada
        SetupCanvasGroup(battery1);
        SetupCanvasGroup(battery2);
    }

    private void SetupCanvasGroup(RectTransform b)
    {
        if (b.GetComponent<CanvasGroup>() == null)
            b.gameObject.AddComponent<CanvasGroup>();
    }

    // ======================================================================
    // DRAG START
    // ======================================================================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currentBattery != null) return;
        if (battery1Snapped && battery2Snapped) return;

        var clicked = eventData.pointerPressRaycast.gameObject;

        if (clicked == null) return;

        // Tentukan battery mana yang di-drag
        if (clicked.transform == battery1 || clicked.transform.IsChildOf(battery1))
            _currentBattery = battery1;
        else if (clicked.transform == battery2 || clicked.transform.IsChildOf(battery2))
            _currentBattery = battery2;
        else
            return; // bukan battery → abaikan

        _currentCanvasGroup = _currentBattery.GetComponent<CanvasGroup>();
        _currentCanvasGroup.blocksRaycasts = false;
    }

    // ======================================================================
    // DRAGGING
    // ======================================================================
    public void OnDrag(PointerEventData eventData)
    {
        if (_currentBattery == null) return;
        if (battery1Snapped && battery2Snapped) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            _canvas.worldCamera,
            out Vector2 localPos))
        {
            _currentBattery.anchoredPosition = localPos;
        }
    }

    // ======================================================================
    // DRAG END
    // ======================================================================
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_currentBattery == null) return;
        if (battery1Snapped && battery2Snapped) return;

        _currentCanvasGroup.blocksRaycasts = true;

        TrySnap(_currentBattery);

        _currentBattery = null;
        _currentCanvasGroup = null;
    }

    // ======================================================================
    // DROP HANDLER
    // ======================================================================
    public void OnDrop(PointerEventData eventData)
    {
        if (_currentBattery == null) return;

        TrySnap(_currentBattery);
    }

    // ======================================================================
    // SNAP LOGIC WITH ANIMATION
    // ======================================================================
    private void TrySnap(RectTransform b)
    {
        RectTransform target = null;

        // Tentukan target zona baterai
        if (b == battery1) target = target1;
        if (b == battery2) target = target2;

        float dist = Vector2.Distance(b.anchoredPosition, target.anchoredPosition);
        Debug.Log($"📏 Distance: {dist}");

        if (dist <= snapDistance)
        {
            SnapBattery(b, target);
        }
    }

    private void SnapBattery(RectTransform battery, RectTransform target)
    {
        // Animasi move
        LeanTween.move(battery, target.anchoredPosition, 0.25f)
                 .setEaseOutQuad();

        // Animasi scale sedikit "pop"
        LeanTween.scale(battery, battery.transform.localScale, 0.25f)
                 .setEaseOutBack();

        // Kunci battery (tidak bisa drag lagi)
        var cg = battery.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = true;
        cg.interactable = false; // opsional

        // Tandai selesai
        if (battery == battery1)
            battery1Snapped = true;
        else if (battery == battery2)
            battery2Snapped = true;

        Debug.Log("✅ Battery inserted!");

        CheckPuzzleCompleted();
    }


    // ======================================================================
    // CHECK PUZZLE DONE
    // ======================================================================
    private void CheckPuzzleCompleted()
    {
        if (battery1Snapped && battery2Snapped)
        {
            Debug.Log("🎉 ClockBack Puzzle Completed!");

            onPuzzleDone?.Invoke();
        }

    }
}
