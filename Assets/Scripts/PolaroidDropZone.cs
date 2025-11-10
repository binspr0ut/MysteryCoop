using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DropZone : NetworkBehaviour
{
    public int slotIndex;
    public PolaroidDragHandler currentPolaroid;

    [Header("Visuals")]
    [Tooltip("Highlight area dropzone (opsional, untuk hover).")]
    [SerializeField] private Image highlightImage; // boleh null
    [Tooltip("Glow image (GameObject UI yang menyala jika benar).")]
    [SerializeField] private GameObject glowImage; // <— ini pengganti Outline

    public RectTransform Rect => GetComponent<RectTransform>();


    void Awake()
    {
        // Matikan glow di awal
        if (glowImage) glowImage.SetActive(false);

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
        var fromZone = incoming.currentDropZone;
        var outgoing = currentPolaroid;

        PlacePolaroid(incoming, animate: true);

        if (outgoing != null)
        {
            if (fromZone != null)
            {
                fromZone.PlacePolaroid(outgoing, animate: true);
            }
            else
            {
                outgoing.currentDropZone = null;
                outgoing.SnapWithBounce(outgoing.originalPosition);
            }
        }
    }


    public void SetHighlight(bool on)
    {
        if (!highlightImage) return;

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
        if (!glowImage) return;

        if (currentPolaroid == null)
        {
            glowImage.SetActive(false);
            DropZoneManager.Instance.BroadcastCorrectSlots();
            return;
        }

        int expectedID = DropZoneManager.Instance.correctOrder[slotIndex];
        int actualID = currentPolaroid.photoID;
        bool correct = actualID == expectedID;

        if (NetworkManager.Singleton.IsHost)
            DropZoneManager.Instance.BroadcastCorrectSlots(); // kirim update setelah perubahan posisi

        if (!NetworkManager.Singleton.IsHost)
        {
            // client tidak menghitung apapun, hanya menunggu broadcast
            return;
        }
    }


    public void ShowGlow(bool on)
    {
        if (!glowImage) return;
        StopAllCoroutines();
        StartCoroutine(FadeGlow(on));
    }


    private IEnumerator FadeGlow(bool on)
    {
        if (!glowImage) yield break;
        var img = glowImage.GetComponent<Image>();
        if (!img) yield break;

        float startAlpha = img.color.a;
        float targetAlpha = on ? 1f : 0f;
        float t = 0f;

        while (t < 0.25f)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t / 0.25f);
            var c = img.color;
            c.a = a;
            img.color = c;
            yield return null;
        }

        img.color = new Color(img.color.r, img.color.g, img.color.b, targetAlpha);
        glowImage.SetActive(on);
    }

}
