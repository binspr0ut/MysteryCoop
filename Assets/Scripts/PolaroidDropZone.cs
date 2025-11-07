using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DropZone : MonoBehaviour
{
    public int slotIndex;
    public PolaroidDragHandler currentPolaroid;

    [Header("Visuals")]
    [Tooltip("Optional: Image untuk highlight area dropzone (mis. Image kosong di atas slot).")]
    [SerializeField] private Image highlightImage; // boleh null
    [Tooltip("Optional: Outline/GLOW di tepi slot (UI Outline atau Shadow).")]
    [SerializeField] private Outline glowOutline;  // boleh null

    public RectTransform Rect => GetComponent<RectTransform>();

    void Reset()
    {
        highlightImage = GetComponent<Image>();
        glowOutline = GetComponent<Outline>();
    }
    void Awake()
    {
        if (glowOutline)
        {
            glowOutline.effectColor = Color.clear;
            glowOutline.effectDistance = Vector2.zero;
        }
        if (slotIndex >= DropZoneManager.Instance.correctOrder.Count)
        {
            Debug.LogError($"[DROPZONE ERROR] slotIndex {slotIndex} lebih besar dari correctOrder.Count ({DropZoneManager.Instance.correctOrder.Count}).");
        }

    }

    public void PlacePolaroid(PolaroidDragHandler p, bool animate = true)
    {
        if (p.currentDropZone != null)
            p.currentDropZone.currentPolaroid = null;

        currentPolaroid = p;
        p.currentDropZone = this;

        var target = Rect.anchoredPosition;
        if (animate) p.SnapWithBounce(target);
        else p.Rect.anchoredPosition = target;
        Debug.Log($"[DROP] Polaroid {p.photoID} placed into slot {slotIndex}");

        UpdateCorrectGlow();

    }

    public void SwapPolaroid(PolaroidDragHandler incoming)
    {
        var fromZone = incoming.currentDropZone;   // null = dari free area
        var outgoing = currentPolaroid;            // polaroid yang sekarang di slot ini

        // 1) Masukkan incoming ke slot ini
        PlacePolaroid(incoming, animate: true);

        // 2) Tangani outgoing (kalau ada)
        if (outgoing != null)
        {
            if (fromZone != null)
            {
                // KASUS: dua-duanya dari dropzone -> swap antar zone
                fromZone.PlacePolaroid(outgoing, animate: true);
            }
            else
            {
                // KASUS: incoming dari free area -> keluarkan outgoing ke free
                outgoing.currentDropZone = null;

                // pastikan originalPosition adalah posisi bebas (bukan slot)
                // (kita hanya set originalPosition ketika item berasal dari area bebas)
                outgoing.SnapWithBounce(outgoing.originalPosition);
            }
        }
    }


    public void SetHighlight(bool on)
    {
        if (!highlightImage) return;

        // simple pulse saat on
        if (on)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * DropZoneManager.Instance.highlightPulseSpeed);
            var c = DropZoneManager.Instance.highlightColor;
            c.a *= pulse;
            highlightImage.color = c;
        }
        else
        {
            highlightImage.color = DropZoneManager.Instance.normalColor;
        }
    }

    public void UpdateCorrectGlow()
    {
        if (!glowOutline)
        {
            Debug.LogWarning($"[GLOW] DropZone {slotIndex} tidak punya Outline.");
            return;
        }

        if (currentPolaroid == null)
        {
            // Tidak ada polaroid di sini → glow mati total
            glowOutline.effectColor = Color.clear;
            glowOutline.effectDistance = Vector2.zero;
            Debug.Log($"[GLOW] Slot {slotIndex} kosong → glow OFF");
            return;
        }

        var order = DropZoneManager.Instance.correctOrder;

        if (slotIndex < 0 || slotIndex >= order.Count)
        {
            Debug.LogWarning($"[GLOW] SlotIndex {slotIndex} di luar range order list.");
            glowOutline.effectColor = Color.clear;
            glowOutline.effectDistance = Vector2.zero;
            return;
        }

        int expectedID = order[slotIndex];
        int actualID = currentPolaroid.photoID;

        bool correct = actualID == expectedID;

        if (correct)
        {
            // gunakan warna glow dari manager (atau warna #A5FFDE yang kamu pilih)
            Color glowColor = DropZoneManager.Instance.correctGlowColor;

            glowOutline.effectColor = glowColor;
            glowOutline.effectDistance = new Vector2(3f, 3f);

            Debug.Log($"[GLOW] ✅ Slot {slotIndex}: Polaroid {actualID} BENAR (expected {expectedID}) → Glow ON");
        }
        else
        {
            glowOutline.effectColor = Color.clear;
            glowOutline.effectDistance = Vector2.zero;

            Debug.Log($"[GLOW] ❌ Slot {slotIndex}: Polaroid {actualID} SALAH (expected {expectedID}) → Glow OFF");
        }
    }

}
