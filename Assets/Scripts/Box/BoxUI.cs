using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class BoxUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Puzzle Objects")]
    [SerializeField] private GameObject battery1;
    [SerializeField] private GameObject battery2;

    [SerializeField] private RectTransform[] covers;

    [Header("Puzzle Settings")]
    [SerializeField] private float moveThreshold = 150f; // jarak minimum dianggap "digeser"

    public event Action onPuzzleDone;

    private int batteryCount = 0;
    private bool puzzleDone = false;
    private HashSet<GameObject> clickedBatteries = new();
    public event Action<int> onBatteryCollected;

    private RectTransform draggingObject;
    private Canvas canvas;
    private readonly Dictionary<RectTransform, Vector2> initialPositions = new();

    void Start()
    {
        // Pastikan komponen UI dasar ada
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null) Debug.LogWarning("[BoxUI] Canvas tidak ditemukan di parent.");
        if (FindFirstObjectByType<EventSystem>() == null) Debug.LogWarning("[BoxUI] EventSystem tidak ada di scene.");

        // Simpan posisi awal cover
        foreach (var c in covers)
        {
            if (c != null)
                initialPositions[c] = c.anchoredPosition;
        }

        // (Opsional) Jika tetap mau pakai Button.onClick, ini aman juga:
        TryWireButton(battery1);
        TryWireButton(battery2);
    }

    private void TryWireButton(GameObject go)
    {
        if (go == null) return;
        var btn = go.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => HandleBatteryClick(go));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (puzzleDone) return;
        var clicked = eventData.pointerCurrentRaycast.gameObject;
        if (clicked == null) return;

        // 1) Jika klik cover → mulai drag
        foreach (var c in covers)
        {
            if (clicked == c?.gameObject)
            {
                draggingObject = c;
                return;
            }
        }

        // 2) Jika klik di area baterai (termasuk child)
        if (IsUnder(clicked, battery1))
        {
            HandleBatteryClick(battery1);
            return;
        }
        if (IsUnder(clicked, battery2))
        {
            HandleBatteryClick(battery2);
            return;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (puzzleDone || draggingObject == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPos))
        {
            draggingObject.localPosition = localPos;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (puzzleDone || draggingObject == null) return;

        float distance = Vector2.Distance(draggingObject.anchoredPosition, initialPositions[draggingObject]);

        if (distance > moveThreshold)
        {
            // Tanda visual cover sudah “terbuka”
            var img = draggingObject.GetComponent<Image>();
            if (img) img.enabled = false;

            // Yang terpenting: cover TIDAK lagi memblok raycast
            if (img) img.raycastTarget = false;
            var cg = draggingObject.GetComponent<CanvasGroup>();
            if (cg) cg.blocksRaycasts = false;

            CheckIfAllCoversRemoved();
        }

        draggingObject = null;
    }

    private void CheckIfAllCoversRemoved()
    {
        if (!AllCoversCleared()) return;

        Debug.Log("✅ All covers removed! You can now take the batteries!");
    }

    private bool AllCoversCleared()
    {
        foreach (var c in covers)
        {
            if (c == null) continue;

            // Jika masih dekat posisi awal → dianggap belum terbuka
            if (Vector2.Distance(c.anchoredPosition, initialPositions[c]) < moveThreshold)
                return false;

            // Selain jarak, kita juga coba lihat apakah masih memblok raycast
            var img = c.GetComponent<Image>();
            var cg = c.GetComponent<CanvasGroup>();
            bool stillBlocks = (img && img.raycastTarget) || (cg && cg.blocksRaycasts);
            if (stillBlocks) return false;
        }
        return true;
    }

    private void HandleBatteryClick(GameObject batteryObj)
    {
        if (puzzleDone) return;

        if (batteryObj == null) return;
        if (clickedBatteries.Contains(batteryObj)) return; // cegah double click

        clickedBatteries.Add(batteryObj);

        // Ambil baterai
        batteryObj.SetActive(false);
        batteryCount++;
        Debug.Log($"🔋 Battery collected: {batteryCount}/2");

        // Inform ClockBack!
        onBatteryCollected?.Invoke(batteryCount);



        if (batteryCount >= 2)
        {
            puzzleDone = true;
            Debug.Log("🎉 Puzzle solved! Both batteries collected!");
            onPuzzleDone?.Invoke();
        }
    }

    private bool IsUnder(GameObject clicked, GameObject targetRoot)
    {
        if (clicked == null || targetRoot == null) return false;
        var t = clicked.transform;
        var root = targetRoot.transform;
        while (t != null)
        {
            if (t == root) return true;
            t = t.parent;
        }
        return false;
    }
}
