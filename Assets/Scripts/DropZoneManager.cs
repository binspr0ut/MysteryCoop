using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using System.Linq;

public class DropZoneManager : NetworkBehaviour
{
    public static DropZoneManager Instance;

    [Header("Zones")]
    public List<DropZone> zones = new();

    [Header("Puzzle Order (by slotIndex)")]
    // Ubah sesuai kebutuhanmu: indeks 0..N harus cocok dengan photoID yang benar.
    public List<int> correctOrder = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };

    [Header("Snap & Highlight")]
    public float snapDistance = 150f;
    public Color highlightColor = new Color(1f, 1f, 1f, 0.35f);  // semi-white overlay
    public Color normalColor = new Color(1f, 1f, 1f, 0f);        // no overlay
    public Color correctGlowColor = new Color(0.65f, 1f, 0.87f, 1f); // A5FFDE
    // 🧠 daftar slot yang benar dikirim dari host ke client
    private NetworkVariable<FixedString64Bytes> syncedCorrectSlots = new(writePerm: NetworkVariableWritePermission.Server);

    public float highlightPulseSpeed = 3f;

    private void Awake()
    {
        Instance = this;
        if (zones.Count == 0)
            zones.AddRange(GetComponentsInChildren<DropZone>(true));
        AutoBindInitial();

    }
    void Start()
    {
        Debug.Log($"[MANAGER] correctOrder count = {correctOrder.Count}");
    }

    public void AutoBindInitial()
    {
        var allPolaroids = FindObjectsOfType<PolaroidDragHandler>(true);
        foreach (var p in allPolaroids)
        {
            DropZone nearest = GetClosestZone(p.Rect.anchoredPosition, Mathf.Infinity);
            if (!nearest) continue;

            float d = Vector2.Distance(p.Rect.anchoredPosition, nearest.Rect.anchoredPosition);
            // toleransi: kalau sangat dekat dengan slot, anggap sudah ditempatkan
            if (d <= 20f && nearest.currentPolaroid == null)
            {
                nearest.PlacePolaroid(p, animate: false);
            }
        }
    }

    public DropZone GetClosestZone(Vector2 pos, float maxDistance)
    {
        DropZone best = null;
        float bestDist = Mathf.Infinity;

        foreach (var z in zones)
        {
            float dist = Vector2.Distance(pos, z.Rect.anchoredPosition);
            if (dist < bestDist && dist <= maxDistance)
            {
                bestDist = dist;
                best = z;
            }
        }

        return best;
    }

    public bool IsAllCorrect()
    {
        // Semua slot terisi dan tiap slot cocok photoID
        foreach (var z in zones)
        {
            if (z == null || z.currentPolaroid == null) return false;
            if (z.slotIndex < 0 || z.slotIndex >= correctOrder.Count) return false;

            if (z.currentPolaroid.photoID != correctOrder[z.slotIndex]) return false;
        }
        return true;
    }

    public override void OnNetworkSpawn()
    {
        syncedCorrectSlots.OnValueChanged += (_, newVal) =>
        {
            // hanya Spirit yang update visual glow
            if (!IsServer)
                UpdateGlowFromString(newVal.ToString());
        };
    }

    // 🟢 dipanggil host setiap kali ada perubahan
    public void BroadcastCorrectSlots()
    {
        if (!IsServer) return;

        var correctIndexes = zones
            .Where(z => z.currentPolaroid != null && z.currentPolaroid.photoID == correctOrder[z.slotIndex])
            .Select(z => z.slotIndex)
            .ToList();

        syncedCorrectSlots.Value = string.Join(",", correctIndexes);
        Debug.Log($"[DropZoneManager] Synced correct slots: {syncedCorrectSlots.Value}");
    }

    // 🔵 dijalankan client (Spirit)
    private void UpdateGlowFromString(string str)
    {
        var activeIndexes = str.Split(',').Where(s => int.TryParse(s, out _)).Select(int.Parse).ToHashSet();

        foreach (var z in zones)
        {
            bool on = activeIndexes.Contains(z.slotIndex);
            z.ShowGlow(on);
        }
    }

}
