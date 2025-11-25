using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class PuzzlePieceDragHandler : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Puzzle Info")]
    public int pieceId;
    public bool ownerIsHost; // siapa yg boleh drag?
    public bool isLocked = false;

    [Header("SFX")]
    [SerializeField] private AudioClip dragPaperSFX;

    [Header("Textures")]
    public GameObject DetectivePaper;
    public GameObject SpiritPaper;

    public RectTransform rect;
    private Canvas canvas;
    private CanvasGroup group;

    bool isDragging = false;

    // ================================
    // DOUBLE CHANNEL
    // ================================
    private NetworkVariable<Vector2> hostNorm =
        new(writePerm: NetworkVariableWritePermission.Server);

    private NetworkVariable<Vector2> clientNorm =
        new(writePerm: NetworkVariableWritePermission.Server);

    Coroutine smoothRoutine;


    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        group = GetComponent<CanvasGroup>();
    }

    public override void OnNetworkSpawn()
    {
        ApplyTexture(IsHost);

        // HOST menerima clientNorm
        clientNorm.OnValueChanged += (_, newVal) =>
        {
            if (IsHost && !ownerIsHost)
                SmoothMove(Denormalize(newVal));
        };

        // CLIENT menerima hostNorm
        hostNorm.OnValueChanged += (_, newVal) =>
        {
            if (!IsHost && ownerIsHost)
                SmoothMove(Denormalize(newVal));
        };

        StartCoroutine(InitPos());
    }

    IEnumerator InitPos()
    {
        yield return null;

        if (ownerIsHost)
            hostNorm.Value = Normalize(rect.anchoredPosition);
        else
            clientNorm.Value = Normalize(rect.anchoredPosition);
    }

    bool CanControl()
    {
        if (ownerIsHost && IsHost) return true;
        if (!ownerIsHost && !IsHost) return true;
        return false;
    }

    // ================================
    // DRAG
    // ================================
    public void OnBeginDrag(PointerEventData d)
    {
        if (!CanControl()) return;
        if (isLocked) return;

        // 🔊 SFX: mulai drag / memindahkan paper
        PlaySFX(dragPaperSFX);

        isDragging = true;
        group.alpha = 0.8f;
        group.blocksRaycasts = false;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData d)
    {
        if (!isDragging || !CanControl()) return;
        if (isLocked) return;

        rect.anchoredPosition += d.delta / canvas.scaleFactor;

        // Kirim posisi:
        if (IsHost)
            hostNorm.Value = Normalize(rect.anchoredPosition);
        else
            UpdateClientPositionServerRpc(Normalize(rect.anchoredPosition));
    }

    public void OnEndDrag(PointerEventData d)
    {
        if (!CanControl()) return;
        if (isLocked) return;

        isDragging = false;
        group.alpha = 1f;
        group.blocksRaycasts = true;
    }

    // CLIENT → SERVER
    [ServerRpc(RequireOwnership = false)]
    void UpdateClientPositionServerRpc(Vector2 pos)
    {
        clientNorm.Value = pos;
    }

    [ClientRpc]
    public void SetLockedClientRpc(bool state)
    {
        isLocked = state;

        // optionally: visual feedback
        if (state)
            group.alpha = 1f;
    }


    // ================================
    // HELPERS
    // ================================
    Vector2 Normalize(Vector2 pos)
    {
        RectTransform parent = rect.parent as RectTransform;
        Vector2 s = parent.rect.size;

        return new Vector2(
            (pos.x + s.x * 0.5f) / s.x,
            (pos.y + s.y * 0.5f) / s.y
        );
    }

    Vector2 Denormalize(Vector2 n)
    {
        RectTransform parent = rect.parent as RectTransform;
        Vector2 s = parent.rect.size;

        return new Vector2(
            n.x * s.x - s.x * 0.5f,
            n.y * s.y - s.y * 0.5f
        );
    }

    void SmoothMove(Vector2 target)
    {
        if (smoothRoutine != null) StopCoroutine(smoothRoutine);
        smoothRoutine = StartCoroutine(SmoothRoutine(target));
    }

    IEnumerator SmoothRoutine(Vector2 target)
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

    void ApplyTexture(bool isDetective)
    {
        if (DetectivePaper) DetectivePaper.SetActive(isDetective);
        if (SpiritPaper) SpiritPaper.SetActive(!isDetective);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip);
    }

}
