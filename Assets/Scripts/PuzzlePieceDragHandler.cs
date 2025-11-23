using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class PuzzlePieceDragHandler : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Puzzle Info")]
    public int pieceId;          // ID puzzle
    public bool ownerIsHost;     // Host punya piece ini?

    [Header("Texture for Both Roles")]
    public GameObject DetectivePaper;  // visible only for host
    public GameObject SpiritPaper;     // visible only for client

    [HideInInspector] public RectTransform rect;
    private Canvas canvas;
    private CanvasGroup group;

    private bool isDragging = false;

    // normalisasi posisi untuk sync
    private NetworkVariable<Vector2> syncedNorm =
        new(writePerm: NetworkVariableWritePermission.Server);

    Coroutine smoothRoutine;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        group = GetComponent<CanvasGroup>();
        if (!group) group = gameObject.AddComponent<CanvasGroup>();

        Debug.Log($"[INIT] Loaded PieceId {pieceId}, ownerIsHost={ownerIsHost}, CanvasGroupFound={group != null}");
    }

    void Start()
    {
        Debug.Log($"[HIERARCHY] GameObject={gameObject.name} parent={transform.parent.name} hasImage={GetComponent<Image>() != null}");
    }


    public override void OnNetworkSpawn()
    {
        bool isDetective = IsHost;
        ApplyTexture(isDetective);

        syncedNorm.OnValueChanged += (_, newVal) =>
        {
            if (!IsOwner)
            {
                if (smoothRoutine != null) StopCoroutine(smoothRoutine);
                smoothRoutine = StartCoroutine(SmoothSetPosition(Denormalize(newVal)));
            }
        };

        StartCoroutine(InitDelayed());
    }

    IEnumerator InitDelayed()
    {
        yield return null;

        if (syncedNorm.Value == Vector2.zero)
        {
            UpdatePositionServerRpc(Normalize(rect.anchoredPosition));
        }
        else
        {
            rect.anchoredPosition = Denormalize(syncedNorm.Value);
        }
    }

    //=============================
    // ROLE FILTER
    //=============================
    bool CanControl()
    {
        bool result = false;

        if (ownerIsHost && IsHost) result = true;
        if (!ownerIsHost && !IsHost && IsClient) result = true;

        Debug.Log($"[ROLE CHECK] ownerIsHost={ownerIsHost} | IsHost={IsHost} | IsClient={IsClient} | RESULT={result}");

        return result;
    }


    //=============================
    // DRAG
    //=============================
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[DRAG] OnBeginDrag dipanggil? CanControl={CanControl()} IsOwner={IsOwner}");

        if (!CanControl())
        {
            Debug.Log("[DRAG] GAGAL — CanControl = FALSE (Role salah)");
            return;
        }

        Debug.Log("[DRAG] LOLOS CanControl");

        isDragging = true;
        group.alpha = 0.8f;
        group.blocksRaycasts = false;
        transform.SetAsLastSibling();

        Debug.Log("[DRAG] MULAI DRAG — group.blocksRaycasts=false");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log($"[DRAG] OnDrag dipanggil? isDragging={isDragging} CanControl={CanControl()}");

        if (!CanControl())
        {
            Debug.Log("[DRAG] DITOLAK — CanControl=FALSE");
            return;
        }

        if (!isDragging)
        {
            Debug.Log("[DRAG] isDragging FALSE (OnBeginDrag tidak pernah terpanggil)");
            return;
        }

        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
        UpdatePositionServerRpc(Normalize(rect.anchoredPosition));

        Debug.Log($"[DRAG] Posisi baru: {rect.anchoredPosition}");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("[DRAG] OnEndDrag terpanggil");

        if (!CanControl())
        {
            Debug.Log("[DRAG] EndDrag ditolak — CanControl FALSE");
            return;
        }

        isDragging = false;
        group.alpha = 1f;
        group.blocksRaycasts = true;
        UpdatePositionServerRpc(Normalize(rect.anchoredPosition));

        Debug.Log("[DRAG] STOP DRAG");
    }


    IEnumerator DelayedSync()
    {
        yield return new WaitForSeconds(0.2f);
        UpdatePositionServerRpc(Normalize(rect.anchoredPosition));
    }

    //=============================
    // HELPERS
    //=============================
    void ApplyTexture(bool isDetective)
    {
        if (DetectivePaper) DetectivePaper.SetActive(isDetective);
        if (SpiritPaper) SpiritPaper.SetActive(!isDetective);
    }

    Vector2 Normalize(Vector2 pos)
    {
        RectTransform parent = rect.parent as RectTransform;
        Vector2 size = parent.rect.size;

        return new Vector2(
            (pos.x + size.x * 0.5f) / size.x,
            (pos.y + size.y * 0.5f) / size.y
        );
    }

    Vector2 Denormalize(Vector2 norm)
    {
        RectTransform parent = rect.parent as RectTransform;
        Vector2 size = parent.rect.size;

        return new Vector2(
            norm.x * size.x - size.x * 0.5f,
            norm.y * size.y - size.y * 0.5f
        );
    }

    IEnumerator SmoothSetPosition(Vector2 target)
    {
        Vector2 start = rect.anchoredPosition;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 8f;
            rect.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        rect.anchoredPosition = target;
    }

    void KeepInsideParent()
    {
        RectTransform parent = rect.parent as RectTransform;
        Vector2 pos = rect.anchoredPosition;
        Vector2 size = parent.rect.size;
        Vector2 child = rect.sizeDelta;

        Vector2 min = -size * 0.5f + child * 0.5f;
        Vector2 max = size * 0.5f - child * 0.5f;

        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.y = Mathf.Clamp(pos.y, min.y, max.y);

        rect.anchoredPosition = pos;
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdatePositionServerRpc(Vector2 newNorm)
    {
        syncedNorm.Value = newNorm;
    }

}
