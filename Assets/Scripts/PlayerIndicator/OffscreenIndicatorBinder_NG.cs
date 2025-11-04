using Unity.Netcode;
using UnityEngine;

public class OffscreenIndicatorBinder_NG : MonoBehaviour
{
    [Header("Indicator Prefab")]
    public RectTransform indicatorPrefab;

    [Header("Style")]
    public Color teammateColor = Color.cyan;
    public float screenEdgePadding = 24f;
    public float worldHeightOffset = 1.2f;

    private OffscreenIndicator indicator;

    void Start()
    {
        InvokeRepeating(nameof(BindOnce), 0.25f, 0.25f);
    }

    private void BindOnce()
    {
        if (indicator != null) return;

        if (indicatorPrefab == null)
        {
            Debug.LogWarning("❌ Indicator prefab belum diassign di inspector!");
            return;
        }

        if (Camera.main == null)
        {
            Debug.Log("⏳ Belum ada MainCamera, tunggu sebentar...");
            return;
        }

        var nm = NetworkManager.Singleton;
        if (nm == null || !nm.IsClient) return;

        var localObj = nm.SpawnManager.GetLocalPlayerObject();
        if (localObj == null)
        {
            Debug.Log("⏳ Belum ada local player object, tunggu...");
            return;
        }

        // Cari target pemain lain
        Transform other = FindOtherPlayerTransform(localObj);
        if (other == null)
        {
            Debug.Log("❌ Tidak menemukan pemain lain!");
            return;
        }

        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ OffscreenIndicatorBinder_NG harus ditempel di HUD Canvas!");
            return;
        }

        var go = new GameObject("OffscreenIndicatorRuntime", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        indicator = go.AddComponent<OffscreenIndicator>();
        indicator.canvas = canvas;
        indicator.targetCamera = Camera.main;
        indicator.indicatorPrefab = indicatorPrefab;
        indicator.target = other;
        indicator.screenEdgePadding = screenEdgePadding;
        indicator.worldHeightOffset = worldHeightOffset;
        indicator.indicatorColor = teammateColor;

        indicator.hideWhenOnScreen = false; // biar kelihatan dulu

        Debug.Log($"✅ Indicator dibuat untuk target: {other.name}");
    }

    private Transform FindOtherPlayerTransform(NetworkObject localPlayerObj)
    {
        var dets = GameObject.FindObjectsOfType<DetectiveMovement>(true);
        foreach (var d in dets)
        {
            var no = d.GetComponentInParent<NetworkObject>();
            if (no != null && no != localPlayerObj)
                return d.transform;
        }

        var spirits = GameObject.FindObjectsOfType<SpiritMovement>(true);
        foreach (var s in spirits)
        {
            var no = s.GetComponentInParent<NetworkObject>();
            if (no != null && no != localPlayerObj)
                return s.transform;
        }

        return null;
    }
}