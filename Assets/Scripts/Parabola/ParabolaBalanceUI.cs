using UnityEngine;
using UnityEngine.UI;

public class ParabolaBalanceUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform track;
    public RectTransform window;
    public RectTransform bar;
    public Image progressFill;

    [Header("Gyro Settings")]
    public float gyroSensitivity = 1200f;
    public float maxTiltClamp = 0.6f;

    [Header("Window Move")]
    public float driftLeftSpeed = 300f;

    [Header("Bar Move")]
    public float barSpeed = 100f;
    public float barJitter = 0f;

    [Header("Win / Signal")]
    public float keepInsideTime = 1f;
    public float decayMultiplier = 0.6f;

    [Header("Haptic")]
    public float hapticCooldown = 0.12f;   // 120ms
    private float hapticTimer = 0f;
    private float lastSignal = 0f;

    [Header("Debug")]
    public bool debugLogs = true;

    private float timer;
    private float barDir = 1f;
    private ParabolaBalance owner;

    float TrackHalf => track.rect.width * 0.5f;
    float WindowHalf => window.rect.width * 0.5f;

    private Color lowColor = Color.white;
    private Color highColor = new Color(0.22f, 1f, 0.08f);

    // ============================================================
    // INIT
    // ============================================================
    public void Init(ParabolaBalance o)
    {
        owner = o;
        timer = 0f;
        barDir = Random.value > 0.5f ? 1f : -1f;

        SetX(window, -TrackHalf + WindowHalf);
        SetX(bar, 0f);

        if (progressFill)
            progressFill.fillAmount = 0f;

        lastSignal = 0f;
        hapticTimer = 0f;

        if (debugLogs)
            Debug.Log("[ParabolaUI] Init with Gyro");
    }

    void OnDisable()
    {
        owner?.ClientSetSignal01(0f);

        if (progressFill)
            progressFill.fillAmount = 0f;

        timer = 0f;
        lastSignal = 0f;
    }

    // ============================================================
    // UPDATE LOOP
    // ============================================================
    void Update()
    {
        if (owner == null) return;

        float dt = Time.unscaledDeltaTime;

        // ---------- 1. Gyro tilt  ----------
        float tiltX = Mathf.Clamp(Input.acceleration.x, -maxTiltClamp, maxTiltClamp);
        float tiltAmount = tiltX * gyroSensitivity;

        // ---------- 2. Window move ----------
        float w = GetX(window);
        float drift = -driftLeftSpeed * dt;
        w += (tiltAmount * dt) + drift;
        w = Mathf.Clamp(w, -TrackHalf + WindowHalf, TrackHalf - WindowHalf);
        SetX(window, w);

        // ---------- 3. Bar move ----------
        float b = GetX(bar);
        float jitter = Random.Range(-barJitter, barJitter);
        b += (barSpeed + jitter) * barDir * dt;

        if (b > TrackHalf || b < -TrackHalf)
        {
            b = Mathf.Clamp(b, -TrackHalf, TrackHalf);
            barDir *= -1f;
        }
        SetX(bar, b);

        // ---------- 4. Inside check ----------
        bool inside = Mathf.Abs(b - w) <= WindowHalf;

        if (inside) timer += dt;
        else timer -= dt * Mathf.Clamp(decayMultiplier, 0.05f, 1f);

        timer = Mathf.Clamp(timer, 0f, keepInsideTime);

        float signal01 = (keepInsideTime <= 0.001f) ? 0f : (timer / keepInsideTime);

        if (progressFill)
        {
            progressFill.fillAmount = signal01;
            progressFill.color = Color.Lerp(lowColor, highColor, signal01);
        }

        // HAPTIC ADAPTIVE
        DoAdaptiveHaptics(signal01);

        owner?.ClientSetSignal01(signal01);
    }

    // ============================================================
    // ADAPTIVE HAPTIC SYSTEM
    // ============================================================
    void DoAdaptiveHaptics(float signal01)
    {
        if (hapticTimer > 0f)
        {
            hapticTimer -= Time.unscaledDeltaTime;
            return;
        }

        // ==========================
        // SUCCESS (100%)
        // ==========================
        if (signal01 >= 1f && lastSignal < 1f)
        {
            iOSHaptic.NotifySuccess();
            if (debugLogs) Debug.Log("[HAPTIC] SUCCESS 100%");
            hapticTimer = hapticCooldown;
            lastSignal = signal01;
            return;
        }

        // Only vibrate when signal INCREASES
        if (signal01 <= lastSignal)
        {
            lastSignal = signal01;
            return;
        }

        // ==========================
        // LEVEL-BASED HAPTIC
        // ==========================
        if (signal01 < 0.3f)
        {
            iOSHaptic.ImpactSoft();
            if (debugLogs) Debug.Log("[HAPTIC] Soft (0–30%)");
        }
        else if (signal01 < 0.7f)
        {
            iOSHaptic.ImpactMedium();
            if (debugLogs) Debug.Log("[HAPTIC] Medium (30–70%)");
        }
        else
        {
            iOSHaptic.ImpactRigid();
            if (debugLogs) Debug.Log("[HAPTIC] Rigid (70–99%)");
        }

        // Prevent spam
        hapticTimer = hapticCooldown;

        lastSignal = signal01;
    }

    // ============================================================
    // HELPERS
    // ============================================================
    static float GetX(RectTransform rt) => rt.localPosition.x;

    static void SetX(RectTransform rt, float x)
    {
        var p = rt.localPosition;
        p.x = x;
        rt.localPosition = p;
    }
}


// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;

// public class ParabolaBalanceUI : MonoBehaviour
// {
//     [Header("Refs")]
//     public RectTransform track;
//     public RectTransform window;
//     public RectTransform bar;
//     public Image progressFill;
//     public Button balanceButton; // 🔹 Tambahkan ini

//     [Header("Window Move")]
//     public float moveRightSpeed = 450f;
//     public float driftLeftSpeed = 300f;

//     [Header("Bar Move")]
//     public float barSpeed = 100f;
//     public float barJitter = 0f;

//     [Header("Win / Signal")]
//     public float keepInsideTime = 1f;
//     public float decayMultiplier = 0.6f;

//     [Header("Debug")]
//     public bool debugLogs = true;
//     [Range(1, 60)] public int logEveryNFrames = 10;

//     private bool pressing;
//     private float timer;
//     private float barDir = 1f;
//     private ParabolaBalance owner;
//     private bool solvedOnceForDebugStop;

//     float TrackHalf => track.rect.width * 0.5f;
//     float WindowHalf => window.rect.width * 0.5f;

//     private Color lowColor = Color.white;
//     private Color highColor = new Color(0.22f, 1f, 0.08f);

//     public void Init(ParabolaBalance o)
//     {
//         owner = o;
//         pressing = false;
//         timer = 0f;
//         barDir = Random.value > 0.5f ? 1f : -1f;
//         solvedOnceForDebugStop = false;

//         SetX(window, -TrackHalf + WindowHalf);
//         SetX(bar, 0f);

//         if (progressFill) progressFill.fillAmount = 0f;

//         // 🔹 Pasang event listener tombol
//         if (balanceButton)
//         {
//             balanceButton.onClick.RemoveAllListeners();
//             EventTrigger trigger = balanceButton.GetComponent<EventTrigger>();
//             if (trigger == null) trigger = balanceButton.gameObject.AddComponent<EventTrigger>();

//             trigger.triggers.Clear();

//             // PointerDown
//             EventTrigger.Entry downEntry = new EventTrigger.Entry
//             {
//                 eventID = EventTriggerType.PointerDown
//             };
//             downEntry.callback.AddListener(_ => pressing = true);
//             trigger.triggers.Add(downEntry);

//             // PointerUp
//             EventTrigger.Entry upEntry = new EventTrigger.Entry
//             {
//                 eventID = EventTriggerType.PointerUp
//             };
//             upEntry.callback.AddListener(_ => pressing = false);
//             trigger.triggers.Add(upEntry);
//         }

//         if (debugLogs)
//             Debug.Log($"[ParabolaUI] Init with BalanceButton bound");
//     }

//     void OnDisable()
//     {
//         owner?.ClientSetSignal01(0f);
//         if (progressFill) progressFill.fillAmount = 0f;
//         timer = 0f;
//     }

//     void Update()
//     {
//         if (owner == null) return;

//         float dt = Time.unscaledDeltaTime;

//         // Window move
//         float w = GetX(window);
//         float v = pressing ? moveRightSpeed : -driftLeftSpeed;
//         w = Mathf.Clamp(w + v * dt, -TrackHalf + WindowHalf, TrackHalf - WindowHalf);
//         SetX(window, w);

//         // Bar move
//         float b = GetX(bar);
//         float jitter = Random.Range(-barJitter, barJitter);
//         b += (barSpeed + jitter) * barDir * dt;
//         if (b > TrackHalf || b < -TrackHalf)
//         {
//             b = Mathf.Clamp(b, -TrackHalf, TrackHalf);
//             barDir *= -1f;
//         }
//         SetX(bar, b);

//         // Inside check
//         bool inside = Mathf.Abs(b - w) <= WindowHalf;

//         if (inside) timer += dt;
//         else timer -= dt * Mathf.Clamp(decayMultiplier, 0.05f, 1f);
//         timer = Mathf.Clamp(timer, 0f, keepInsideTime);

//         float signal01 = (keepInsideTime <= 0.0001f) ? 0f : (timer / keepInsideTime);
//         if (progressFill) progressFill.fillAmount = signal01;
//         if (progressFill) progressFill.color = Color.Lerp(lowColor, highColor, signal01);

//         owner?.ClientSetSignal01(signal01);
//     }

//     // Helpers
//     static float GetX(RectTransform rt) => rt.localPosition.x;
//     static void SetX(RectTransform rt, float x)
//     {
//         var p = rt.localPosition; p.x = x; rt.localPosition = p;
//     }
// }
