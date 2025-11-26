using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

[RequireComponent(typeof(RectTransform))]
public class PolaroidDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int photoID;

    public RectTransform Rect { get; private set; }
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    public Vector2 originalPosition;
    public DropZone currentDropZone;

    [Header("Release Settings")]
    public float releaseThreshold = 35f;


    [Header("Drag Settings")]
    [Tooltip("Seberapa cepat lerp posisi saat drag (0..1). 1 = langsung.")]
    [Range(0.05f, 1f)] public float dragLerp = 0.25f;

    private bool isDragging = false;
    private Vector2 dragTarget;

    private bool startedFromZone;        // apakah drag dimulai dari slot?
    private bool wasDetachedFromZone;    // sudah auto-detach saat drag?
    private Vector2 lastFreePosition;    // posisi bebas terakhir (bukan slot)

    private Coroutine snapRoutine;

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Update()
    {
        if (isDragging)
        {
            // Smooth Lerp ke target (memberi rasa “berat” saat drag)
            Rect.anchoredPosition = Vector2.Lerp(Rect.anchoredPosition, dragTarget, dragLerp);
            KeepInsideParentBounds();


            // highlight zone terdekat
            var nearest = DropZoneManager.Instance.GetClosestZone(Rect.anchoredPosition, DropZoneManager.Instance.snapDistance);
            foreach (var z in DropZoneManager.Instance.zones)
                z.SetHighlight(z == nearest && nearest != null);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startedFromZone = (currentDropZone != null);
        wasDetachedFromZone = false;

        if (currentDropZone != null)
        {
            currentDropZone.currentPolaroid = null;
            currentDropZone.UpdateCorrectGlow();
            currentDropZone = null;
            Debug.Log($"[RELEASE] Polaroid {photoID} released from dropzone");
        }

        isDragging = true;

        // Simpan originalPosition HANYA kalau start dari area bebas.
        // (Kalau start dari slot, kita nggak mau "ingat" posisi slot sebagai tempat kembali.)
        if (!startedFromZone)
            originalPosition = Rect.anchoredPosition;

        if (snapRoutine != null) StopCoroutine(snapRoutine);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;

        dragTarget = Rect.anchoredPosition;
    }



    public void OnDrag(PointerEventData eventData)
    {
        dragTarget += eventData.delta / canvas.scaleFactor;

        if (!wasDetachedFromZone) // cek sekali saja setelah keluar dari slot
        {
            // kalau tadinya dari slot, anggap detach ketika target cukup jauh
            if (startedFromZone)
            {
                var nearestZone = currentDropZone; // pasti null setelah OnBeginDrag
                                                   // gunakan jarak dari target ke posisi slot terdekat hanya jika kamu mau
                                                   // tapi lebih simpel: begitu mulai drag, anggap kita sudah free
                wasDetachedFromZone = true;
            }
        }

        // update posisi bebas terakhir (buat fallback animasi kalau kamu mau)
        lastFreePosition = Rect.anchoredPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        foreach (var z in DropZoneManager.Instance.zones) z.SetHighlight(false);

        var nearest = DropZoneManager.Instance.GetClosestZone(Rect.anchoredPosition, DropZoneManager.Instance.snapDistance);

        if (nearest == null)
        {
            // ⛔ FIX UTAMA: JANGAN snap ke originalPosition. Biarkan tetap di tempatnya (free).
            // Rapikan sedikit: clamp biar tetap di dalam parent.
            KeepInsideParentBounds();
            return;
        }

        if (nearest.currentPolaroid == null)
            nearest.PlacePolaroid(this, animate: true);
        else
            nearest.SwapPolaroid(this);

        foreach (var z in DropZoneManager.Instance.zones) z.UpdateCorrectGlow();
        if (DropZoneManager.Instance.IsAllCorrect())
        {
            Debug.Log("🎉 Puzzle Solved!");
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            {
                StartCoroutine(MyEndingSequence());
            }
            else
            {
                // Mode offline fallback
                PuzzleNetworkManager.Instance.ActivateMapForOfflineTest();
            }
        }

    }

    // ======================
    // Helpers
    // ======================

    private IEnumerator MyEndingSequence()
    {
        GameObject eventSystem = GameObject.Find("EventSystem");

        eventSystem.SetActive(false);
        yield return SubtitleManager.Instance.ShowAndWaitRoutine(
            "polaroidnya udah selesai semua cok.", target: SubtitleTarget.Detective
        );

        yield return SubtitleManager.Instance.ShowAndWaitRoutine(
            "keren juga", target: SubtitleTarget.Spirit
        );


        PuzzleNetworkManager.Instance.PuzzleSolvedServerRpc();

        eventSystem.SetActive(false);
        yield return SubtitleManager.Instance.ShowAndWaitRoutine(
            "keknya ada polanya cok.", target: SubtitleTarget.Detective
        );

        yield return SubtitleManager.Instance.ShowAndWaitRoutine(
            "iya juga", target: SubtitleTarget.Spirit
        );


        eventSystem.SetActive(true);
    }

    void KeepInsideParentBounds()
    {
        var parent = Rect.parent as RectTransform;
        if (!parent) return;

        Vector2 target = Rect.anchoredPosition;

        Vector2 childSize = Rect.rect.size;
        Vector2 parentSize = parent.rect.size;

        Vector2 min = -parent.pivot * parentSize + Rect.pivot * childSize;
        Vector2 max = (Vector2.one - parent.pivot) * parentSize - (Vector2.one - Rect.pivot) * childSize;

        target.x = Mathf.Clamp(target.x, min.x, max.x);
        target.y = Mathf.Clamp(target.y, min.y, max.y);

        Rect.anchoredPosition = target;
    }



    public void SnapWithBounce(Vector2 target)
    {
        if (snapRoutine != null) StopCoroutine(snapRoutine);
        snapRoutine = StartCoroutine(SnapBounceRoutine(target, 0.22f));
    }

    IEnumerator SnapBounceRoutine(Vector2 target, float duration)
    {
        Vector2 start = Rect.anchoredPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = EaseOutBack(t, 1.2f); // sedikit bounce
            Rect.anchoredPosition = Vector2.LerpUnclamped(start, target, eased);
            yield return null;
        }

        Rect.anchoredPosition = target;
        snapRoutine = null;
    }

    // EaseOutBack (k = overshoot ~1.1–1.7)
    float EaseOutBack(float x, float k = 1.25f)
    {
        float c1 = k;
        float c3 = c1 + 1f;
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }
}
