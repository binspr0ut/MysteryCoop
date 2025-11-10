using UnityEngine;
using UnityEngine.Events;

public class ReadNoteOverlay : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("Root panel overlay yang menutupi layar (punya Image dengan Raycast Target ON).")]
    public GameObject root;                 // ex: Panel (isi gambar kertas & teks)
    [Tooltip("(Opsional) CanvasGroup pada root untuk fade.")]
    public CanvasGroup canvasGroup;         // boleh kosong

    [Header("Behavior")]
    public bool showOnStart = false;        // true kalau mau muncul otomatis saat scene masuk
    public bool pauseGameWhileShown = false;
    public bool onlyOncePerSession = true;  // hanya sekali per run
    public string onceKey = "FirstFloor_IntroNote_Shown";

    [Header("Events")]
    public UnityEvent onShown;
    public UnityEvent onHidden;

    bool _visible;
    bool _alreadyShown;

    void Awake()
    {
        if (root == null) root = gameObject;     // fallback
        if (canvasGroup == null) canvasGroup = root.GetComponent<CanvasGroup>();

        _alreadyShown = onlyOncePerSession && PlayerPrefs.GetInt(onceKey, 0) == 1;

        // Pastikan awalnya tersembunyi di Editor/Build
        SetActiveInstant(false);
    }

    void Start()
    {
        if (showOnStart && !_alreadyShown)
            Show();
    }

    void Update()
    {
        if (!_visible) return;

        // ——— INPUT TANPA NEW INPUT SYSTEM ———
        // Mouse / tap layar
        bool mouseTap = Input.GetMouseButtonDown(0);
        bool touchTap = false;
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) touchTap = true;
        }
        // (opsional) tombol apa saja
        bool anyKey = Input.anyKeyDown;

        if (mouseTap || touchTap || anyKey)
        {
            Hide();
        }
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
            StartCoroutine(FadeOutAndDisable(0.15f));
        }
        else
        {
            root.SetActive(false);
            onHidden?.Invoke();
        }
    }

    // ===== Helpers =====
    System.Collections.IEnumerator FadeTo(float target, float dur)
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

    System.Collections.IEnumerator FadeOutAndDisable(float dur)
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