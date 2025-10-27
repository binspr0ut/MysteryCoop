using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class BoxUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Puzzle Objects")]
    [SerializeField] private GameObject battery;
    [SerializeField] private RectTransform[] covers;

    private RectTransform draggingObject;
    private Canvas canvas;
    private bool puzzleDone = false;
    private Dictionary<RectTransform, Vector2> initialPositions = new(); // 🧩 posisi awal semua cover
    public event Action onPuzzleDone;

    [Header("Puzzle Settings")]
    [SerializeField] private float moveThreshold = 150f; // jarak minimum dianggap "digeser"


    void Start()
    {
        canvas = GetComponentInParent<Canvas>();

        foreach (var c in covers)
        {
            if (c != null)
                initialPositions[c] = c.anchoredPosition;
        }

        battery.GetComponent<Button>().onClick.AddListener(OnBatteryClicked);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (puzzleDone) return;

        var clicked = eventData.pointerCurrentRaycast.gameObject;
        if (clicked == null) return;

        foreach (var c in covers)
        {
            if (clicked == c.gameObject)
            {
                draggingObject = c;
                break;
            }
        }
        if (clicked.name.Contains("Battery"))
            OnBatteryClicked();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (puzzleDone || draggingObject == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPos);

        draggingObject.localPosition = localPos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (puzzleDone || draggingObject == null) return;

        float distance = Vector2.Distance(draggingObject.anchoredPosition, initialPositions[draggingObject]);

        if (distance > moveThreshold)
        {
            draggingObject.GetComponent<Image>().color = Color.blue;
            CheckIfAllCoversRemoved();
        }

        draggingObject = null;
    }

    private void CheckIfAllCoversRemoved()
    {
        foreach (var c in covers)
        {
            if (Vector2.Distance(c.anchoredPosition, initialPositions[c]) < moveThreshold)
                return;
        }

        var img = battery.GetComponent<Image>();
        if (img) img.color = Color.yellow;
        Debug.Log("Battery revealed!");
    }

    private void OnBatteryClicked()
    {
        if (puzzleDone) return;

        // Pastikan semua cover sudah hilang
        foreach (var c in covers)
        {
            if (Vector2.Distance(c.anchoredPosition, initialPositions[c]) < moveThreshold)
                return;
        }

        puzzleDone = true;
        battery.SetActive(false);
        Debug.Log("Puzzle solved!");

        onPuzzleDone?.Invoke();
    }

}
