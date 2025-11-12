using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum SubtitleTarget { Detective, Spirit }
public enum SubtitleScope { Local, Global }

public class SubtitleManager : NetworkBehaviour
{
    public static SubtitleManager Instance;

    [Header("UI References")]
    public CanvasGroup subtitleCanvas;
    public TextMeshProUGUI subtitleText;

    [Header("Underlay Settings")]
    public Color detectiveUnderlayColor = new(0.1f, 0.5f, 1f, 0.5f);
    public Color spiritUnderlayColor = new(0.8f, 0.1f, 1f, 0.5f);

    [Header("Timing Settings")]
    public float fadeDuration = 0.3f;
    public float stayDuration = 3f;

    private Queue<(string text, SubtitleTarget target)> localQueue = new();
    private Queue<(string text, SubtitleTarget target)> globalQueue = new();
    private bool isLocalShowing = false;
    private bool isGlobalShowing = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ================================================================
    // ✅ Public API
    // ================================================================
    public void ShowSubtitle(string text, SubtitleTarget forWho, SubtitleScope scope)
    {
        if (scope == SubtitleScope.Global)
        {
            // Pastikan hanya server yang enqueue
            if (IsServer)
                EnqueueGlobalSubtitle(text, forWho);
            else
                RequestGlobalSubtitleServerRpc(text, forWho);
            return; // 🚫 Jangan tampilkan langsung di client
        }

        // Local subtitle
        TryQueueLocal(text, forWho, isGlobal: false);
    }


    // ================================================================
    // 🔄 Network RPC
    // ================================================================
    [ServerRpc(RequireOwnership = false)]
    private void RequestGlobalSubtitleServerRpc(string text, SubtitleTarget forWho, ServerRpcParams rpcParams = default)
    {
        EnqueueGlobalSubtitle(text, forWho);
    }

    [ClientRpc]
    private void ShowSubtitleClientRpc(string text, SubtitleTarget forWho)
    {
        // isGlobal = true → tampil untuk semua pemain
        TryQueueLocal(text, forWho, isGlobal: true);
    }

    // ================================================================
    // 🧠 Global Queue (Server-side)
    // ================================================================
    private void EnqueueGlobalSubtitle(string text, SubtitleTarget forWho)
    {
        globalQueue.Enqueue((text, forWho));
        if (!isGlobalShowing)
            StartCoroutine(ProcessGlobalQueue());
    }

    private IEnumerator ProcessGlobalQueue()
    {
        isGlobalShowing = true;

        while (globalQueue.Count > 0)
        {
            var (text, target) = globalQueue.Dequeue();
            ShowSubtitleClientRpc(text, target);
            yield return new WaitForSeconds(stayDuration + fadeDuration + 0.1f);
        }

        isGlobalShowing = false;
    }

    // ================================================================
    // 🎨 Local Display (per player)
    // ================================================================
    private void TryQueueLocal(string text, SubtitleTarget forWho, bool isGlobal)
    {
        var player = NetworkManager.Singleton?.LocalClient?.PlayerObject;
        if (player == null) return;

        // 💡 Kalau lokal → tetap filter berdasarkan role
        // 💡 Kalau global → tampil di semua pemain
        if (!isGlobal)
        {
            var role = player.CompareTag("Detective") ? SubtitleTarget.Detective : SubtitleTarget.Spirit;
            if (forWho != role) return;
        }

        localQueue.Enqueue((text, forWho));

        if (!isLocalShowing)
            StartCoroutine(ProcessLocalQueue());
    }

    private IEnumerator ProcessLocalQueue()
    {
        isLocalShowing = true;

        while (localQueue.Count > 0)
        {
            var (text, target) = localQueue.Dequeue();
            ApplyUnderlay(target);
            yield return ShowRoutine(text);
        }

        isLocalShowing = false;
    }

    // ================================================================
    // 💡 Helper
    // ================================================================
    private void ApplyUnderlay(SubtitleTarget target)
    {
        var mat = subtitleText.fontMaterial;
        if (mat == null) return;

        if (target == SubtitleTarget.Detective)
            mat.SetColor(ShaderUtilities.ID_UnderlayColor, detectiveUnderlayColor);
        else
            mat.SetColor(ShaderUtilities.ID_UnderlayColor, spiritUnderlayColor);

        mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.5f);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0f);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -1f);
    }

    private IEnumerator ShowRoutine(string text)
    {
        subtitleText.text = text;
        yield return FadeCanvas(1);
        yield return new WaitForSeconds(stayDuration);
        yield return FadeCanvas(0);
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        float start = subtitleCanvas.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            subtitleCanvas.alpha = Mathf.Lerp(start, targetAlpha, time / fadeDuration);
            yield return null;
        }

        subtitleCanvas.alpha = targetAlpha;
    }
}
