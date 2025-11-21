using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum SubtitleTarget { Detective, Spirit }
public enum SubtitleScope { Local, Global }

public class SubtitleManager : NetworkBehaviour
{
    public static SubtitleManager Instance;

    [Header("UI References")]
    public CanvasGroup subtitleCanvas;
    public TextMeshProUGUI subtitleText;

    [Header("BG Settings")]
    public RectTransform bgRect;   // drag BG object here
    public UnityEngine.UI.Image bgImage;
    public RectTransform containerRect; // drag Container ke sini


    public Color detectiveBG = new Color(0.0f, 0.15f, 0.3f, 0.85f);
    public Color spiritBG = new Color(0.2f, 0.0f, 0.25f, 0.85f);


    [Header("Timing Settings")]
    public float fadeDuration = 0.3f;
    public float stayDuration = 3f;   // default

    // Queue: text, target, duration
    private Queue<(string text, SubtitleTarget target, float duration)> localQueue = new();
    private Queue<(string text, SubtitleTarget target, float duration)> globalQueue = new();

    // Coroutine states
    private bool isLocalShowing = false;
    private bool isGlobalShowing = false;

    private Coroutine localRoutine;
    private Coroutine globalRoutine;
    public bool IsSubtitleShowing { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        subtitleCanvas.alpha = 0f;

        if (bgRect != null)
            bgRect.localScale = Vector3.zero;
    }


    // ========================================================================
    // 🧩 PUBLIC API
    // ========================================================================
    public void ShowSubtitle(
        string text,
        SubtitleTarget forWho,
        SubtitleScope scope,
        float duration = -1f,
        bool overwrite = false)
    {
        if (duration < 0f) duration = stayDuration;

        // ====================================================================
        // 🌍 GLOBAL SCOPE
        // ====================================================================
        if (scope == SubtitleScope.Global)
        {
            if (overwrite)
            {
                // Server clear queue & stop current routine
                if (IsServer)
                {
                    ForceClearGlobal();
                    ForceInstantHide();

                    // broadcast ke semua client untuk clear & tampilkan subtitle
                    ForceGlobalOverwriteClientRpc(text, forWho, duration);
                }
                else
                {
                    RequestGlobalOverwriteServerRpc(text, forWho, duration);
                }

                return;
            }

            // Non-overwrite global
            if (IsServer)
                EnqueueGlobalSubtitle(text, forWho, duration);
            else
                RequestGlobalSubtitleServerRpc(text, forWho, duration);

            return;
        }

        // ====================================================================
        // 📍 LOCAL SCOPE
        // ====================================================================
        if (overwrite)
        {
            ForceClearLocal();
            ForceInstantHide();
        }

        TryQueueLocal(text, forWho, duration, isGlobal: false);
    }

    // ========================================================================
    // 🔄 RPC
    // ========================================================================

    // Request normal global subtitle
    [ServerRpc(RequireOwnership = false)]
    private void RequestGlobalSubtitleServerRpc(string text, SubtitleTarget target, float duration)
    {
        EnqueueGlobalSubtitle(text, target, duration);
    }

    // Request overwrite secara global dari client
    [ServerRpc(RequireOwnership = false)]
    private void RequestGlobalOverwriteServerRpc(string text, SubtitleTarget target, float duration)
    {
        ForceClearGlobal();
        ForceInstantHide();
        ForceGlobalOverwriteClientRpc(text, target, duration);
    }

    // Server → client untuk overwrite global
    [ClientRpc]
    private void ForceGlobalOverwriteClientRpc(string text, SubtitleTarget target, float duration)
    {
        // buang semua antrean local
        ForceClearLocal();

        // hilangkan subtitle langsung
        ForceInstantHide();

        // tampilkan subtitle overwrite langsung
        TryQueueLocal(text, target, duration, isGlobal: true);
    }

    // Normal global subtitle → client
    [ClientRpc]
    private void ShowSubtitleClientRpc(string text, SubtitleTarget target, float duration)
    {
        TryQueueLocal(text, target, duration, isGlobal: true);
    }

    // ========================================================================
    // 🌍 GLOBAL QUEUE PROCESSING
    // ========================================================================
    private void EnqueueGlobalSubtitle(string text, SubtitleTarget target, float duration)
    {
        globalQueue.Enqueue((text, target, duration));

        if (!isGlobalShowing)
            globalRoutine = StartCoroutine(ProcessGlobalQueue());
    }

    private IEnumerator ProcessGlobalQueue()
    {
        isGlobalShowing = true;

        while (globalQueue.Count > 0)
        {
            var (text, target, duration) = globalQueue.Dequeue();

            ShowSubtitleClientRpc(text, target, duration);

            yield return new WaitForSeconds(duration + fadeDuration + 0.1f);
        }

        isGlobalShowing = false;
    }

    // ========================================================================
    // 👤 LOCAL QUEUE PROCESSING
    // ========================================================================
    private void TryQueueLocal(string text, SubtitleTarget forWho, float duration, bool isGlobal)
    {
        var player = NetworkManager.Singleton?.LocalClient?.PlayerObject;
        if (player == null) return;

        if (!isGlobal)
        {
            var tag = player.CompareTag("Detective") ? SubtitleTarget.Detective : SubtitleTarget.Spirit;
            if (forWho != tag) return;
        }

        localQueue.Enqueue((text, forWho, duration));

        if (!isLocalShowing)
            localRoutine = StartCoroutine(ProcessLocalQueue());
    }

    private IEnumerator ProcessLocalQueue()
    {
        isLocalShowing = true;

        while (localQueue.Count > 0)
        {
            var (text, target, duration) = localQueue.Dequeue();

            ApplyUnderlay(target);
            yield return ShowRoutine(text, duration);
        }

        isLocalShowing = false;
    }

    // ========================================================================
    // 🚨 FORCE CLEAR
    // ========================================================================
    private void ForceClearLocal()
    {
        localQueue.Clear();

        if (localRoutine != null)
            StopCoroutine(localRoutine);

        isLocalShowing = false;
    }

    private void ForceClearGlobal()
    {
        globalQueue.Clear();

        if (globalRoutine != null)
            StopCoroutine(globalRoutine);

        isGlobalShowing = false;
    }

    private void ForceInstantHide()
    {
        subtitleCanvas.alpha = 0;
    }

    public void ForceClearAll()
    {
        ForceClearLocal();
        ForceClearGlobal();
        ForceInstantHide();
    }

    // ========================================================================
    // 🎨 DISPLAY HELPERS
    // ========================================================================
    private void ApplyUnderlay(SubtitleTarget target)
    {
        if (bgImage != null)
        {
            bgImage.color = target == SubtitleTarget.Detective ? detectiveBG : spiritBG;
        }
    }


    private IEnumerator ShowRoutine(string text, float duration)
    {
        subtitleText.text = text;

        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        bgRect.sizeDelta = containerRect.sizeDelta + new Vector2(0, 0);


        // Pastikan start dalam keadaan invisible
        subtitleCanvas.alpha = 0f;
        bgRect.localScale = Vector3.zero;

        // ========================
        // ANIMASI SCALE BG MASUK
        // ========================
        float t = 0;
        float scaleDuration = 0.25f;
        while (t < scaleDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, t / scaleDuration);
            bgRect.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        bgRect.localScale = Vector3.one;

        // ========================
        // FADE MUNCUL
        // ========================
        yield return FadeCanvas(1);

        // TUNGGU DURASI
        yield return new WaitForSeconds(duration);

        // FADE KELUAR
        yield return FadeCanvas(0);

        bgRect.localScale = Vector3.zero;
    }

    public IEnumerator ShowAndWaitRoutine(string text, SubtitleTarget target = SubtitleTarget.Detective, float duration = -1f)
    {
        IsSubtitleShowing = true;

        // non-overwrite, force "local scope"
        ShowSubtitle(text, target, SubtitleScope.Global, duration, overwrite: false);

        // tunggu sampai subtitle selesai
        float estimated = (duration < 0 ? stayDuration : duration) + fadeDuration * 2f + 0.1f;

        yield return new WaitForSeconds(estimated);

        IsSubtitleShowing = false;
    }


    private IEnumerator FadeCanvas(float targetAlpha)
    {
        float start = subtitleCanvas.alpha;
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            subtitleCanvas.alpha = Mathf.Lerp(start, targetAlpha, t / fadeDuration);
            yield return null;
        }

        subtitleCanvas.alpha = targetAlpha;
    }
}



// using UnityEngine;
// using Unity.Netcode;
// using TMPro;
// using System.Collections;
// using System.Collections.Generic;

// public enum SubtitleTarget { Detective, Spirit }
// public enum SubtitleScope { Local, Global }

// public class SubtitleManager : NetworkBehaviour
// {
//     public static SubtitleManager Instance;

//     [Header("UI References")]
//     public CanvasGroup subtitleCanvas;
//     public TextMeshProUGUI subtitleText;

//     [Header("Underlay Settings")]
//     public Color detectiveUnderlayColor = new(0.1f, 0.5f, 1f, 0.5f);
//     public Color spiritUnderlayColor = new(0.8f, 0.1f, 1f, 0.5f);

//     [Header("Timing Settings")]
//     public float fadeDuration = 0.3f;
//     public float stayDuration = 3f;

//     private Queue<(string text, SubtitleTarget target)> localQueue = new();
//     private Queue<(string text, SubtitleTarget target)> globalQueue = new();
//     private bool isLocalShowing = false;
//     private bool isGlobalShowing = false;

//     void Awake()
//     {
//         if (Instance == null) Instance = this;
//         else Destroy(gameObject);
//     }

//     // ================================================================
//     // ✅ Public API
//     // ================================================================
//     public void ShowSubtitle(string text, SubtitleTarget forWho, SubtitleScope scope)
//     {
//         if (scope == SubtitleScope.Global)
//         {
//             // Pastikan hanya server yang enqueue
//             if (IsServer)
//                 EnqueueGlobalSubtitle(text, forWho);
//             else
//                 RequestGlobalSubtitleServerRpc(text, forWho);
//             return; // 🚫 Jangan tampilkan langsung di client
//         }

//         // Local subtitle
//         TryQueueLocal(text, forWho, isGlobal: false);
//     }


//     // ================================================================
//     // 🔄 Network RPC
//     // ================================================================
//     [ServerRpc(RequireOwnership = false)]
//     private void RequestGlobalSubtitleServerRpc(string text, SubtitleTarget forWho, ServerRpcParams rpcParams = default)
//     {
//         EnqueueGlobalSubtitle(text, forWho);
//     }

//     [ClientRpc]
//     private void ShowSubtitleClientRpc(string text, SubtitleTarget forWho)
//     {
//         // isGlobal = true → tampil untuk semua pemain
//         TryQueueLocal(text, forWho, isGlobal: true);
//     }

//     // ================================================================
//     // 🧠 Global Queue (Server-side)
//     // ================================================================
//     private void EnqueueGlobalSubtitle(string text, SubtitleTarget forWho)
//     {
//         globalQueue.Enqueue((text, forWho));
//         if (!isGlobalShowing)
//             StartCoroutine(ProcessGlobalQueue());
//     }

//     private IEnumerator ProcessGlobalQueue()
//     {
//         isGlobalShowing = true;

//         while (globalQueue.Count > 0)
//         {
//             var (text, target) = globalQueue.Dequeue();
//             ShowSubtitleClientRpc(text, target);
//             yield return new WaitForSeconds(stayDuration + fadeDuration + 0.1f);
//         }

//         isGlobalShowing = false;
//     }

//     // ================================================================
//     // 🎨 Local Display (per player)
//     // ================================================================
//     private void TryQueueLocal(string text, SubtitleTarget forWho, bool isGlobal)
//     {
//         var player = NetworkManager.Singleton?.LocalClient?.PlayerObject;
//         if (player == null) return;

//         // 💡 Kalau lokal → tetap filter berdasarkan role
//         // 💡 Kalau global → tampil di semua pemain
//         if (!isGlobal)
//         {
//             var role = player.CompareTag("Detective") ? SubtitleTarget.Detective : SubtitleTarget.Spirit;
//             if (forWho != role) return;
//         }

//         localQueue.Enqueue((text, forWho));

//         if (!isLocalShowing)
//             StartCoroutine(ProcessLocalQueue());
//     }

//     private IEnumerator ProcessLocalQueue()
//     {
//         isLocalShowing = true;

//         while (localQueue.Count > 0)
//         {
//             var (text, target) = localQueue.Dequeue();
//             ApplyUnderlay(target);
//             yield return ShowRoutine(text);
//         }

//         isLocalShowing = false;
//     }

//     // ================================================================
//     // 💡 Helper
//     // ================================================================
//     private void ApplyUnderlay(SubtitleTarget target)
//     {
//         var mat = subtitleText.fontMaterial;
//         if (mat == null) return;

//         if (target == SubtitleTarget.Detective)
//             mat.SetColor(ShaderUtilities.ID_UnderlayColor, detectiveUnderlayColor);
//         else
//             mat.SetColor(ShaderUtilities.ID_UnderlayColor, spiritUnderlayColor);

//         mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.5f);
//         mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0f);
//         mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -1f);
//     }

//     private IEnumerator ShowRoutine(string text)
//     {
//         subtitleText.text = text;
//         yield return FadeCanvas(1);
//         yield return new WaitForSeconds(stayDuration);
//         yield return FadeCanvas(0);
//     }

//     private IEnumerator FadeCanvas(float targetAlpha)
//     {
//         float start = subtitleCanvas.alpha;
//         float time = 0f;

//         while (time < fadeDuration)
//         {
//             time += Time.deltaTime;
//             subtitleCanvas.alpha = Mathf.Lerp(start, targetAlpha, time / fadeDuration);
//             yield return null;
//         }

//         subtitleCanvas.alpha = targetAlpha;
//     }
// }
