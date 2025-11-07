using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ParabolaBalanceUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Refs")]
    public RectTransform track;
    public RectTransform window;
    public RectTransform bar;
    public Image progressFill; // pastikan Image.type = Filled (Horizontal/Vertical)

    [Header("Window Move")]
    public float moveRightSpeed = 450f; // px/sec saat tap
    public float driftLeftSpeed = 300f; // px/sec saat lepas

    [Header("Bar Move")]
    public float barSpeed = 100f;
    public float barJitter = 0f;

    [Header("Win / Signal")]
    public float keepInsideTime = 1f;          // waktu agar full bar
    public float decayMultiplier = 0.6f;       // Q2=B: decay halus (0..1), 1 = sama cepat dgn naik

    [Header("Debug")]
    public bool debugLogs = true;
    [Range(1, 60)] public int logEveryNFrames = 10;

    private bool pressing;
    private float timer;
    private float barDir = 1f;
    private ParabolaBalance owner;
    private bool solvedOnceForDebugStop; // hanya utk stop log, bukan solve permanen

    float TrackHalf => track.rect.width * 0.5f;
    float WindowHalf => window.rect.width * 0.5f;

    private Color lowColor = Color.white;
    private Color highColor = new Color(0.22f, 1f, 0.08f); // #39FF14 neon green


    public void Init(ParabolaBalance o)
    {
        owner = o;
        pressing = false;
        timer = 0f;
        barDir = Random.value > 0.5f ? 1f : -1f;
        solvedOnceForDebugStop = false;

        // Start positions
        SetX(window, -TrackHalf + WindowHalf); // kiri
        SetX(bar, 0f);                         // tengah

        if (progressFill) progressFill.fillAmount = 0f;

        if (debugLogs)
            Debug.Log($"[ParabolaUI] Init. TrackHalf={TrackHalf:F2}, WindowHalf={WindowHalf:F2}");
    }

    void OnDisable()
    {
        // Q1=B: saat UI ditutup → reset signal ke 0
        owner?.ClientSetSignal01(0f);
        if (progressFill) progressFill.fillAmount = 0f;
        timer = 0f;
    }

    void Update()
    {
        if (owner == null) return; // owner belum di-Init → jangan jalan

        float dt = Time.unscaledDeltaTime;

        // Window move
        float w = GetX(window);
        float v = pressing ? moveRightSpeed : -driftLeftSpeed;
        w = Mathf.Clamp(w + v * dt, -TrackHalf + WindowHalf, TrackHalf - WindowHalf);
        SetX(window, w);

        // Bar move (ping-pong)
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

        // Timer naik saat inside, turun halus saat outside
        if (inside) timer += dt;
        else timer -= dt * Mathf.Clamp(decayMultiplier, 0.05f, 1f);

        timer = Mathf.Clamp(timer, 0f, keepInsideTime);

        // progress 0..1
        float signal01 = (keepInsideTime <= 0.0001f) ? 0f : (timer / keepInsideTime);
        if (progressFill) progressFill.fillAmount = signal01;

        if (progressFill)
        {
            progressFill.color = Color.Lerp(lowColor, highColor, signal01);
        }

        // kirim ke owner (akan di-forward via RPC ke server → replicate)
        owner?.ClientSetSignal01(signal01);

        // Debug (stop setelah pertama kali penuh, tapi mekanik tetap jalan)
        if (!solvedOnceForDebugStop && debugLogs && Time.frameCount % logEveryNFrames == 0)
        {
            Debug.Log($"[ParabolaUI] inside={inside} | timer={timer:F2}/{keepInsideTime} | " +
                      $"barX={b:F1} windowX={w:F1} | signal={signal01:F2}");
        }
        if (!solvedOnceForDebugStop && signal01 >= 1f)
        {
            solvedOnceForDebugStop = true;
            if (owner != null && owner.enableDebug)
                Debug.Log("[ParabolaUI] ✅ Full Signal");
        }

    }

    public void OnPointerDown(PointerEventData e) => pressing = true;
    public void OnPointerUp(PointerEventData e) => pressing = false;

    public void OnClickClose() => owner?.ClosePuzzle();

    // Helpers (pakai localPosition sesuai setup awalmu)
    static float GetX(RectTransform rt) => rt.localPosition.x;
    static void SetX(RectTransform rt, float x)
    {
        var p = rt.localPosition; p.x = x; rt.localPosition = p;
    }
}
