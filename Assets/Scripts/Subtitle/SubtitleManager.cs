using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

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
    [Header("Audio Settings")]
    public AudioSource subtitleAudioSource;
    public float defaultVolume = 1f;


    public Color detectiveBG = new Color(0.0f, 0.15f, 0.3f, 0.85f);
    public Color spiritBG = new Color(0.2f, 0.0f, 0.25f, 0.85f);


    [Header("Timing Settings")]
    public float fadeDuration = 0.3f;
    public float stayDuration = 3f;   // default

    // Queue: text, target, duration
    private Queue<(string text, SubtitleTarget target, float duration)> localQueue = new();
    private Queue<(string text, SubtitleTarget target, float duration)> globalQueue = new();
    private Queue<(AudioClip clip, float volume)> audioQueue = new();


    // Coroutine states
    private bool isLocalShowing = false;
    private bool isGlobalShowing = false;

    private Coroutine localRoutine;
    private Coroutine globalRoutine;
    public bool IsSubtitleShowing { get; private set; }

    public UnityEvent onAllSubtitlesFinished;

    private void CheckIfAllDone()
    {
        if (globalQueue.Count == 0 &&
            localQueue.Count == 0 &&
            !isGlobalShowing &&
            !isLocalShowing &&
            !IsSubtitleShowing)
        {
            onAllSubtitlesFinished?.Invoke();
        }
    }

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
        bool overwrite = false,
        SubtitleTarget target = default)
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


    // public void ShowSubtitleWithSound(
    //     string text,
    //     SubtitleTarget forWho,
    //     SubtitleScope scope,
    //     AudioClip clip,
    //     float volume,
    //     float duration = -1f,
    //     bool overwrite = false)
    // {
    //     // masukkan subtitle ke queue seperti biasa
    //     ShowSubtitle(text, forWho, scope, duration, overwrite);

    //     // MASUKKAN AUDIO KE QUEUE SENDIRI
    //     if (clip != null)
    //         audioQueue.Enqueue((clip, volume));
    // }

    public void ShowSubtitleWithSound(
    string text,
    SubtitleTarget forWho,
    SubtitleScope scope,
    AudioClip clip,
    float volume,
    float duration = -1f,
    bool overwrite = false)
    {
        ShowSubtitle(text, forWho, scope, duration, overwrite);

        if (!IsServer) return;

        // find clip index from AudioLibrary
        int clipIndex = System.Array.IndexOf(AudioLibrary.Instance.audioClips, clip);
        if (clipIndex == -1)
        {
            Debug.LogError($"Clip {clip.name} not found in AudioLibrary!");
            return;
        }

        // find target client ID
        // broadcast only to target
        AddAudioToQueueClientRpc(clipIndex, volume);
    }



    private void PlayNextAudio()
    {
        if (audioQueue.Count == 0) return;

        var (clip, volume) = audioQueue.Dequeue();

        if (subtitleAudioSource != null && clip != null)
        {
            subtitleAudioSource.volume = volume;
            subtitleAudioSource.PlayOneShot(clip);
        }
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
        CheckIfAllDone();

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
        PlayNextAudio();


        // TUNGGU DURASI
        yield return new WaitForSeconds(duration);

        // FADE KELUAR
        yield return FadeCanvas(0);

        bgRect.localScale = Vector3.zero;
        IsSubtitleShowing = false;
        CheckIfAllDone();

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

    // ===============================================================
    // 🔊 RPC: Broadcast SFX ke semua client
    // ===============================================================
    [ClientRpc]
    private void AddAudioToQueueClientRpc(int clipIndex, float volume)
    {
        AudioClip clip = AudioLibrary.Instance.GetClip(clipIndex);

        if (clip != null)
            audioQueue.Enqueue((clip, volume));
    }



}


