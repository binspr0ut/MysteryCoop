using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ParabolaBalanceUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Refs")]
    public RectTransform track;
    public RectTransform window;
    public RectTransform bar;
    public Image progressFill;         // opsional

    [Header("Window Move")]
    public float moveRightSpeed = 600f; // px/sec saat tap
    public float driftLeftSpeed = 300f; // px/sec saat lepas

    [Header("Bar Move")]
    public float barSpeed = 280f;       // kecepatan dasar
    public float barJitter = 40f;       // random +/- tiap frame

    [Header("Win")]
    public float keepInsideTime = 3f;   // detik harus di dalam window

    private bool pressing;
    private float timer;
    private float barDir = 1f;
    private ParabolaBalance owner;

    float TrackHalf => track.rect.width * 0.5f;
    float WindowHalf => window.rect.width * 0.5f;

    public void Init(ParabolaBalance o)
    {
        owner = o;
        pressing = false;
        timer = 0f;
        barDir = Random.value > 0.5f ? 1f : -1f;

        // Mulai window di kiri (biar kerasa “dorong ke kanan”)
        SetX(window, -TrackHalf + WindowHalf);
        // Bar start tengah
        SetX(bar, 0f);

        if (progressFill) progressFill.fillAmount = 0f;
    }

    void Update()
    {
        // Gerak window
        float w = GetX(window);
        float v = pressing ? moveRightSpeed : -driftLeftSpeed;
        w = Mathf.Clamp(w + v * Time.unscaledDeltaTime, -TrackHalf + WindowHalf, TrackHalf - WindowHalf);
        SetX(window, w);

        // Gerak bar (acak + pantul di tepi)
        float b = GetX(bar);
        float jitter = Random.Range(-barJitter, barJitter);
        b += (barSpeed + jitter) * barDir * Time.unscaledDeltaTime;
        if (b > TrackHalf || b < -TrackHalf)
        {
            b = Mathf.Clamp(b, -TrackHalf, TrackHalf);
            barDir *= -1f; // mantul
        }
        SetX(bar, b);

        // Cek overlap: bar di dalam window?
        bool inside = Mathf.Abs(b - w) <= WindowHalf;

        // Timer naik/turun
        timer += (inside ? 1f : -1f) * Time.unscaledDeltaTime;
        timer = Mathf.Clamp(timer, 0f, keepInsideTime);

        // Progress & feedback stabilitas
        if (progressFill) progressFill.fillAmount = timer / keepInsideTime;
        owner?.ReportStability(timer / keepInsideTime);

        // Menang?
        if (timer >= keepInsideTime)
        {
            owner?.OnSolved();
        }
    }

    // Tangkap tap pada panel (pastikan Image RaycastTarget = ON)
    public void OnPointerDown(PointerEventData e) => pressing = true;
    public void OnPointerUp(PointerEventData e)   => pressing = false;

    // Dipanggil tombol Close (kalau kamu pasang)
    public void OnClickClose() => owner?.ClosePuzzle();

    // Helpers pos X lokal
    static float GetX(RectTransform rt) => rt.localPosition.x;
    static void SetX(RectTransform rt, float x)
    {
        var p = rt.localPosition;
        p.x = x;
        rt.localPosition = p;
    }
}