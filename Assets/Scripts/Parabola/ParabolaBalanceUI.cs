using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ParabolaBalanceUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform track;
    public RectTransform window;
    public RectTransform bar;
    public Image progressFill;
    public Button balanceButton; // 🔹 Tambahkan ini

    [Header("Window Move")]
    public float moveRightSpeed = 450f;
    public float driftLeftSpeed = 300f;

    [Header("Bar Move")]
    public float barSpeed = 100f;
    public float barJitter = 0f;

    [Header("Win / Signal")]
    public float keepInsideTime = 1f;
    public float decayMultiplier = 0.6f;

    [Header("Debug")]
    public bool debugLogs = true;
    [Range(1, 60)] public int logEveryNFrames = 10;

    private bool pressing;
    private float timer;
    private float barDir = 1f;
    private ParabolaBalance owner;
    private bool solvedOnceForDebugStop;

    float TrackHalf => track.rect.width * 0.5f;
    float WindowHalf => window.rect.width * 0.5f;

    private Color lowColor = Color.white;
    private Color highColor = new Color(0.22f, 1f, 0.08f);

    public void Init(ParabolaBalance o)
    {
        owner = o;
        pressing = false;
        timer = 0f;
        barDir = Random.value > 0.5f ? 1f : -1f;
        solvedOnceForDebugStop = false;

        SetX(window, -TrackHalf + WindowHalf);
        SetX(bar, 0f);

        if (progressFill) progressFill.fillAmount = 0f;

        // 🔹 Pasang event listener tombol
        if (balanceButton)
        {
            balanceButton.onClick.RemoveAllListeners();
            EventTrigger trigger = balanceButton.GetComponent<EventTrigger>();
            if (trigger == null) trigger = balanceButton.gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear();

            // PointerDown
            EventTrigger.Entry downEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            downEntry.callback.AddListener(_ => pressing = true);
            trigger.triggers.Add(downEntry);

            // PointerUp
            EventTrigger.Entry upEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            upEntry.callback.AddListener(_ => pressing = false);
            trigger.triggers.Add(upEntry);
        }

        if (debugLogs)
            Debug.Log($"[ParabolaUI] Init with BalanceButton bound");
    }

    void OnDisable()
    {
        owner?.ClientSetSignal01(0f);
        if (progressFill) progressFill.fillAmount = 0f;
        timer = 0f;
    }

    void Update()
    {
        if (owner == null) return;

        float dt = Time.unscaledDeltaTime;

        // Window move
        float w = GetX(window);
        float v = pressing ? moveRightSpeed : -driftLeftSpeed;
        w = Mathf.Clamp(w + v * dt, -TrackHalf + WindowHalf, TrackHalf - WindowHalf);
        SetX(window, w);

        // Bar move
        float b = GetX(bar);
        float jitter = Random.Range(-barJitter, barJitter);
        b += (barSpeed + jitter) * barDir * dt;
        if (b > TrackHalf || b < -TrackHalf)
        {
            b = Mathf.Clamp(b, -TrackHalf, TrackHalf);
            barDir *= -1f;
        }
        SetX(bar, b);

        // Inside check
        bool inside = Mathf.Abs(b - w) <= WindowHalf;

        if (inside) timer += dt;
        else timer -= dt * Mathf.Clamp(decayMultiplier, 0.05f, 1f);
        timer = Mathf.Clamp(timer, 0f, keepInsideTime);

        float signal01 = (keepInsideTime <= 0.0001f) ? 0f : (timer / keepInsideTime);
        if (progressFill) progressFill.fillAmount = signal01;
        if (progressFill) progressFill.color = Color.Lerp(lowColor, highColor, signal01);

        owner?.ClientSetSignal01(signal01);
    }

    // Helpers
    static float GetX(RectTransform rt) => rt.localPosition.x;
    static void SetX(RectTransform rt, float x)
    {
        var p = rt.localPosition; p.x = x; rt.localPosition = p;
    }
}
