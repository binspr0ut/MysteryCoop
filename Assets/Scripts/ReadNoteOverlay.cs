using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class ReadNoteOverlay : MonoBehaviour
{
    [Header("Refs")]
    public GameObject root;
    public CanvasGroup canvasGroup;
    public GameObject objectiveUI;

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

    bool _visible;
    bool _alreadyShown;
    Coroutine countdownRoutine;

    void Awake()
    {
        if (root == null) root = gameObject;
        if (canvasGroup == null) canvasGroup = root.GetComponent<CanvasGroup>();

        _alreadyShown = onlyOncePerSession && PlayerPrefs.GetInt(onceKey, 0) == 1;

        SetActiveInstant(false);
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
            if (countdownRoutine != null) StopCoroutine(countdownRoutine);
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

        if (canvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutAndDisable(0.2f));
        }
        else
        {
            root.SetActive(false);
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
        Hide();
        objectiveUI.SetActive(true);

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
        root.SetActive(false);
        onHidden?.Invoke();
    }

    void SetActiveInstant(bool on)
    {
        root.SetActive(on);
        if (canvasGroup != null) canvasGroup.alpha = on ? 1f : 0f;
    }
}
