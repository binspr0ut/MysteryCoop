using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using Unity.Netcode;

public class ReadNoteOverlay : NetworkBehaviour
{
    [Header("Refs")]
    public GameObject root;
    public CanvasGroup canvasGroup;
    public GameObject objectiveUI;
    public GameObject controlUI;


    [Header("Countdown Settings")]
    public bool autoHide = true;
    public float countdownDuration = 5f;
    public TextMeshProUGUI countdownText;   // ⬅️ tambahkan ini

    [Header("Behavior")]
    public bool showOnStart = false;
    public bool pauseGameWhileShown = false;
    public bool onlyOncePerSession = true;
    public string onceKey = "FirstFloor_IntroNote_Shown";

    [Header("Events")]
    public UnityEvent onShown;
    public UnityEvent onHidden;

    public AudioClip[] audioClips;

    bool _visible;
    bool _alreadyShown;
    Coroutine countdownRoutine;

    void Awake()
    {
        if (root == null) root = gameObject;
        if (canvasGroup == null) canvasGroup = root.GetComponent<CanvasGroup>();

        _alreadyShown = onlyOncePerSession && PlayerPrefs.GetInt(onceKey, 0) == 1;

        SetActiveInstant(false);
        controlUI.SetActive(false);
        objectiveUI.SetActive(false);
    }

    void Start()
    {
        if (showOnStart && !_alreadyShown)
            Show();
    }

    public void Show()
    {
        if (_visible) return;

        _visible = true;

        if (pauseGameWhileShown)
            Time.timeScale = 0f;

        root.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            StopAllCoroutines();
            StartCoroutine(FadeTo(1f, 0.2f));
        }

        // start countdown jika diaktifkan
        if (autoHide)
        {
            if (countdownRoutine != null) { }
            countdownRoutine = StartCoroutine(AutoCountdownHide());
        }

        onShown?.Invoke();
    }

    public void Hide()
    {
        if (!_visible) return;
        _visible = false;

        if (pauseGameWhileShown)
            Time.timeScale = 1f;

        if (onlyOncePerSession)
        {
            PlayerPrefs.SetInt(onceKey, 1);
            PlayerPrefs.Save();
        }

        // ❌ JANGAN StopAllCoroutines();
        if (countdownRoutine != null)
        {
            countdownRoutine = null;
        }

        if (canvasGroup != null)
        {
            StartCoroutine(FadeOutAndDisable(0.2f));
        }
        else
        {
            onHidden?.Invoke();
        }
    }


    // ================================
    // COUNTDOWN 5 DETIK
    // ================================
    IEnumerator AutoCountdownHide()
    {
        float timeLeft = countdownDuration;

        while (timeLeft > 0)
        {
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(timeLeft).ToString();

            timeLeft -= (pauseGameWhileShown ? Time.unscaledDeltaTime : Time.deltaTime);
            yield return null;
        }

        // Hapus angka saat selesai
        if (countdownText != null)
            countdownText.text = "";

        // Scene1StateManager.Instance.ChangeState(Level1State.ExploreBuilding);
        // ⚠️ PENTING: Hanya server yang orkestrasi cutscene
        if (IsServer)
        {
            SubtitleManager.Instance.ShowSubtitleWithSound(
                        "Wait—aren’t you the intern? Why are you here? What happened to you?",
                        SubtitleTarget.Detective,
                        SubtitleScope.Global,
                        audioClips[0],
                        0.7f,
                        5f
                    );

            SubtitleManager.Instance.ShowSubtitleWithSound(
                "I... I don’t remember much. It went dark, and now I’m here… not alive anymore.",
                SubtitleTarget.Spirit,
                SubtitleScope.Global,
                audioClips[1],
                0.8f,
                7f
            );

            SubtitleManager.Instance.ShowSubtitleWithSound(
                "Do you remember anything at all?",
                SubtitleTarget.Detective,
                SubtitleScope.Global,
                audioClips[2],
                0.7f,
                3f
            );

            SubtitleManager.Instance.ShowSubtitleWithSound(
                "Not exactly. But I can help you find out.",
                SubtitleTarget.Spirit,
                SubtitleScope.Global,
                audioClips[3],
                0.8f,
                3f
            );

            SubtitleManager.Instance.ShowSubtitleWithSound(
                "...since that’s why you’re here, right?",
                SubtitleTarget.Spirit,
                SubtitleScope.Global,
                audioClips[4],
                0.8f,
                3f
            );

            SubtitleManager.Instance.ShowSubtitleWithSound(
                "...Alright. Let’s work together.",
                SubtitleTarget.Detective,
                SubtitleScope.Global,
                audioClips[5],
                0.7f,
                3f
            );
        }
        StartCoroutine(ServerDelayUIRoutine(26f));

        Hide();
    }

    private IEnumerator ServerDelayUIRoutine(float delay)
    {
        Debug.Log("ServerDelayUIRoutine started");
        yield return new WaitForSeconds(delay);
        if (IsServer)
            ShowUIClientRpc(); // panggil UI ke semua client
        yield return new WaitForSeconds(1f);
        root.SetActive(false);
    }


    // ================================
    // FADES
    // ================================
    IEnumerator FadeTo(float target, float dur)
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < dur)
        {
            t += (pauseGameWhileShown ? Time.unscaledDeltaTime : Time.deltaTime);
            canvasGroup.alpha = Mathf.Lerp(start, target, t / dur);
            yield return null;
        }
        canvasGroup.alpha = target;
    }

    IEnumerator FadeOutAndDisable(float dur)
    {
        yield return FadeTo(0f, dur);
        onHidden?.Invoke();
    }


    void SetActiveInstant(bool on)
    {
        root.SetActive(on);
        if (canvasGroup != null) canvasGroup.alpha = on ? 1f : 0f;
    }


    IEnumerator FadeInCanvasGroup(CanvasGroup cg, float duration)
    {
        cg.alpha = 0f;
        cg.gameObject.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        cg.alpha = 1f;
    }

    [ClientRpc]
    private void ShowUIClientRpc()
    {
        Debug.Log($"[ShowUIClientRpc] CALLED on client = {NetworkManager.Singleton.LocalClientId}");

        if (controlUI == null)
        {
            Debug.LogWarning("[ShowUIClientRpc] controlUI is NULL! (Not assigned or wrong canvas?)");
        }
        else
        {
            StartCoroutine(FadeInCanvasGroup(controlUI.GetComponent<CanvasGroup>(), 0.5f));
            Debug.Log("[ShowUIClientRpc] controlUI.SetActive(true) DONE");
        }

        if (objectiveUI == null)
        {
            Debug.LogWarning("[ShowUIClientRpc] objectiveUI is NULL!");
        }
        else
        {
            CanvasGroup cg = objectiveUI.GetComponent<CanvasGroup>();
            Debug.Log($"[ShowUIClientRpc] objectiveUI found: {objectiveUI.name}, cg = {cg != null}");


            Debug.Log("[ShowUIClientRpc] starting fade-in coroutine...");
            StartCoroutine(FadeInCanvasGroup(cg, 0.5f));

        }
    }


}
