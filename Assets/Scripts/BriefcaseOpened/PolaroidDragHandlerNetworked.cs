using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class PolaroidDragHandlerNetworked : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int photoID;

    public RectTransform Rect { get; private set; }
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Coroutine lerpRoutine;
    private bool isDragging = false;

    // Disimpan sebagai posisi relatif (0–1) dalam parent
    private NetworkVariable<Vector2> syncedNormalizedPos = new(writePerm: NetworkVariableWritePermission.Owner);

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    public override void OnNetworkSpawn()
    {
        syncedNormalizedPos.OnValueChanged += (_, newVal) =>
        {
            if (!IsOwner)
            {
                if (lerpRoutine != null) StopCoroutine(lerpRoutine);
                lerpRoutine = StartCoroutine(LerpToNormalizedPosition(newVal));
            }
        };

        if (!IsOwner)
        {
            if (TryGetComponent(out GraphicRaycaster gr)) gr.enabled = false;
            if (TryGetComponent(out EventTrigger ev)) ev.enabled = false;
            if (canvasGroup) canvasGroup.blocksRaycasts = false;
        }

        // Pastikan RectTransform sudah siap sebelum sync posisi
        StartCoroutine(DeferredInitPosition());
    }

    IEnumerator DeferredInitPosition()
    {
        RectTransform parent = Rect.parent as RectTransform;

        // Tunggu sampai parent rect valid (sudah punya ukuran)
        while (parent.rect.width < 10f)
            yield return null;

        // Tunggu 1 frame tambahan agar CanvasScaler selesai
        yield return new WaitForEndOfFrame();

        // Jika nilai sync masih default (0,0), artinya belum pernah diset oleh owner
        Vector2 norm = syncedNormalizedPos.Value;
        if (norm == Vector2.zero)
        {
            // Gunakan posisi awal dari prefab / editor sebagai nilai awal
            syncedNormalizedPos.Value = Normalize(Rect.anchoredPosition);
            Debug.Log($"[INIT] Set default normalized pos = {syncedNormalizedPos.Value}");
        }
        else
        {
            // Sudah ada nilai valid dari owner → denormalize dengan benar
            Rect.anchoredPosition = Denormalize(norm);
            Debug.Log($"[INIT] Applied network normalized pos = {norm}");
        }
    }


    void Update()
    {
        if (IsOwner && isDragging)
            syncedNormalizedPos.Value = Normalize(Rect.anchoredPosition);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsOwner) return;
        isDragging = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsOwner) return;
        Rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsOwner) return;

        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        var nearest = DropZoneManager.Instance.GetClosestZone(Rect.anchoredPosition, DropZoneManager.Instance.snapDistance);
        if (nearest == null)
        {
            KeepInsideParentBounds();
            syncedNormalizedPos.Value = Normalize(Rect.anchoredPosition);
            return;
        }

        if (nearest.currentPolaroid == null)
            nearest.PlacePolaroid(GetComponent<PolaroidDragHandler>(), true);
        else
            nearest.SwapPolaroid(GetComponent<PolaroidDragHandler>());

        StartCoroutine(UpdateAfterSnap());
    }

    IEnumerator UpdateAfterSnap()
    {
        yield return new WaitForSeconds(0.25f); // tunggu animasi SnapWithBounce
        syncedNormalizedPos.Value = Normalize(Rect.anchoredPosition);
    }

    // ==========================================
    // 🔧 Helpers
    // ==========================================

    Vector2 Normalize(Vector2 anchoredPos)
    {
        RectTransform parent = Rect.parent as RectTransform;
        Vector2 size = parent.rect.size;
        return new Vector2(
            (anchoredPos.x + size.x * 0.5f) / size.x,
            (anchoredPos.y + size.y * 0.5f) / size.y
        );
    }

    Vector2 Denormalize(Vector2 norm)
    {
        RectTransform parent = Rect.parent as RectTransform;
        Vector2 size = parent.rect.size;
        return new Vector2(
            norm.x * size.x - size.x * 0.5f,
            norm.y * size.y - size.y * 0.5f
        );
    }

    IEnumerator LerpToNormalizedPosition(Vector2 normalized)
    {
        Vector2 target = Denormalize(normalized);
        Vector2 start = Rect.anchoredPosition;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 8f;
            Rect.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }
        Rect.anchoredPosition = target;
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
}
