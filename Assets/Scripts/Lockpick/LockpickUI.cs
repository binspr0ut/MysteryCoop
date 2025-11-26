using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class LockpickUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private RectTransform lockpick;
    [SerializeField] private RectTransform[] pins;
    [SerializeField] private Image[] pinImages;
    [SerializeField] private float[] shearHeights;
    [SerializeField] private float liftLimit = 150f;
    [SerializeField] private float snapThreshold = 80f;
    [SerializeField] private float shearTolerance = 12f;
    [SerializeField] private float colorLerpSpeed = 8f;

    [Header("Shelf Reference")]
    [SerializeField] private ShelfLockpick shelfLockpick;

    [Header("SFX")]
    [SerializeField] private AudioClip lockpickMoveSFX;
    [SerializeField] private AudioClip pinMoveSFX;      // SFX baru: pin naik-turun

    private RectTransform canvasRect;
    private bool isDragging;
    private Vector2 startMousePos;
    private Vector2 startLockpickPos;
    private int currentPinIndex = -1;

    private Vector2[] pinStartPos;
    private bool[] pinUnlocked;
    private Coroutine[] checkCoroutines; // 🕒 cek otomatis per pin

    private float lockpickHalfHeight;

    void Start()
    {
        canvasRect = GetComponent<RectTransform>();
        pinStartPos = new Vector2[pins.Length];
        pinUnlocked = new bool[pins.Length];
        checkCoroutines = new Coroutine[pins.Length];
        pinMoveSFXPlayed = new bool[pins.Length];

        for (int i = 0; i < pins.Length; i++)
            pinStartPos[i] = pins[i].anchoredPosition;

        if (shearHeights.Length != pins.Length)
        {
            shearHeights = new float[pins.Length];
            for (int i = 0; i < pins.Length; i++)
                shearHeights[i] = 100f;
        }

        // langsung ke pin pertama
        if (pins.Length > 0)
        {
            Vector2 lp = lockpick.anchoredPosition;
            lp.x = pins[0].anchoredPosition.x;
            lockpick.anchoredPosition = lp;
        }

        lockpickHalfHeight = lockpick.rect.height * 0.5f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;

        // 🔊 SFX: mulai menggerakkan lockpick
        PlaySFX(lockpickMoveSFX);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out startMousePos);
        startLockpickPos = lockpick.anchoredPosition;
        currentPinIndex = FindNearestPin(lockpick.anchoredPosition.x);

        // reset flag SFX pin untuk pin yang sedang aktif
        if (pinMoveSFXPlayed != null && currentPinIndex >= 0 && currentPinIndex < pinMoveSFXPlayed.Length)
        {
            pinMoveSFXPlayed[currentPinIndex] = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out localPoint);
        Vector2 delta = localPoint - startMousePos;
        Vector2 newPos = startLockpickPos + delta;

        // === Snap horizontal ke pin ===
        float closestX = SnapToPinX(newPos.x);
        newPos.x = closestX;
        lockpick.anchoredPosition = newPos;

        // === Tentukan pin aktif ===
        currentPinIndex = FindNearestPin(lockpick.anchoredPosition.x);
        if (currentPinIndex < 0 || pinUnlocked[currentPinIndex]) return;

        // === Vertikal: angkat pin ===
        Vector3 worldTip = lockpick.TransformPoint(new Vector3(0, lockpick.rect.height * 0.5f, 0));
        Vector2 localTip;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, worldTip),
            eventData.pressEventCamera,
            out localTip
        );

        float lift = Mathf.Clamp(localTip.y - pinStartPos[currentPinIndex].y, 0, liftLimit);

        // 🔊 SFX: pin mulai digerakkan (naik)
        if (lift > 0.5f && pinMoveSFXPlayed != null && currentPinIndex >= 0 && currentPinIndex < pinMoveSFXPlayed.Length)
        {
            if (!pinMoveSFXPlayed[currentPinIndex])
            {
                PlaySFX(pinMoveSFX);
                pinMoveSFXPlayed[currentPinIndex] = true; // 1x per drag
            }
        }

        Vector2 pinPos = pinStartPos[currentPinIndex];
        pins[currentPinIndex].anchoredPosition = new Vector2(pinPos.x, pinPos.y + lift);

        // jika mendekati shear line, mulai cek otomatis
        float distance = Mathf.Abs((pinPos.y + lift) - (pinStartPos[currentPinIndex].y + shearHeights[currentPinIndex]));
        if (distance <= shearTolerance && checkCoroutines[currentPinIndex] == null)
        {
            checkCoroutines[currentPinIndex] = StartCoroutine(AutoUnlockAfterDelay(currentPinIndex, 1f));
        }
        else if (distance > shearTolerance && checkCoroutines[currentPinIndex] != null)
        {
            StopCoroutine(checkCoroutines[currentPinIndex]);
            checkCoroutines[currentPinIndex] = null;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        // reset agar drag berikutnya bisa bunyi lagi
        if (pinMoveSFXPlayed != null && currentPinIndex >= 0 && currentPinIndex < pinMoveSFXPlayed.Length)
        {
            pinMoveSFXPlayed[currentPinIndex] = false;
        }
    }

    IEnumerator AutoUnlockAfterDelay(int index, float delay)
    {
        yield return new WaitForSeconds(delay);

        // pastikan masih di shear line
        float lifted = pins[index].anchoredPosition.y - pinStartPos[index].y;
        if (Mathf.Abs(lifted - shearHeights[index]) <= shearTolerance)
        {
            pinUnlocked[index] = true;
            pins[index].anchoredPosition = new Vector2(pinStartPos[index].x, pinStartPos[index].y + shearHeights[index]);
            Debug.Log($"✅ Pin {index + 1} unlocked!");
            checkCoroutines[index] = null;

            // otomatis pindah ke pin berikutnya
            MoveToNextPin();
            CheckIfAllUnlocked();
        }
    }

    private void MoveToNextPin()
    {
        for (int i = 0; i < pins.Length; i++)
        {
            if (!pinUnlocked[i])
            {
                Vector2 lp = lockpick.anchoredPosition;
                lp.x = pins[i].anchoredPosition.x;
                lockpick.anchoredPosition = lp;
                return;
            }
        }
    }


    private bool[] pinMoveSFXPlayed;   // track biar 1x per drag
    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip);
    }

    private void CheckIfAllUnlocked()
    {
        foreach (bool unlocked in pinUnlocked)
        {
            if (!unlocked) return;
        }

        Debug.Log("🔓 All pins unlocked! Lock opened!");

        // === Trigger Shelf ===
        if (shelfLockpick != null)
        {
            shelfLockpick.UnlockShelfServerRpc(); // 🔹 panggil RPC baru
            shelfLockpick.ClosePuzzle();
        }
    }

    void Update()
    {
        // === Color feedback ===
        for (int i = 0; i < pins.Length; i++)
        {
            if (pinImages == null || i >= pinImages.Length) continue;

            float lifted = pins[i].anchoredPosition.y - pinStartPos[i].y;
            float distance = Mathf.Abs(lifted - shearHeights[i]);
            Color targetColor;

            targetColor = distance <= shearTolerance ? Color.yellow : Color.white;

            pinImages[i].color = Color.Lerp(pinImages[i].color, targetColor, Time.deltaTime * colorLerpSpeed);
        }
    }

    private float SnapToPinX(float x)
    {
        float minDist = float.MaxValue;
        float closestX = x;

        foreach (var pin in pins)
        {
            float dist = Mathf.Abs(pin.anchoredPosition.x - x);
            if (dist < minDist && dist < snapThreshold)
            {
                minDist = dist;
                closestX = pin.anchoredPosition.x;
            }
        }
        return closestX;
    }

    private int FindNearestPin(float x)
    {
        float minDist = float.MaxValue;
        int index = -1;
        for (int i = 0; i < pins.Length; i++)
        {
            float dist = Mathf.Abs(pins[i].anchoredPosition.x - x);
            if (dist < minDist && dist < snapThreshold)
            {
                minDist = dist;
                index = i;
            }
        }
        return index;
    }
}
