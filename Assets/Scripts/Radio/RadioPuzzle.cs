using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadioPuzzle : NetworkBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI References")]
    [SerializeField] private RectTransform knob;
    [SerializeField] private RectTransform needleIndicator;
    [SerializeField] private RectTransform leftLimit;
    [SerializeField] private RectTransform rightLimit;

    [Header("Signal UI")]
    [SerializeField] private Image signalFill; // progress bar signal pada radio

    [Header("Audio")]
    [SerializeField] private AudioSource correctSound;
    [SerializeField] private AudioSource noiseSound;

    [Header("Settings")]
    [Range(-360, 360)] public float targetAngle = -150f;
    public float tolerance = 5f;
    public float minAngle = -300f;
    public float maxAngle = 0f;
    public float rotationSpeed = 0.5f;

    [Header("Signal Source")]
    [SerializeField] private ParabolaBalance parabola; // assign via Inspector
    public float minSignalToHear = 0.2f;

    private bool isDragging = false;
    private float currentAngle;
    private float startAngle;
    private Vector2 startPointerPos;
    private float leftX, rightX;

    // Warna untuk signal bar (White → Neon Green)
    private Color lowColor = Color.white;
    private Color highColor = new Color(0.22f, 1f, 0.08f);

    void Start()
    {
        if (leftLimit) leftX = leftLimit.anchoredPosition.x;
        if (rightLimit) rightX = rightLimit.anchoredPosition.x;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        startPointerPos = eventData.position;
        startAngle = currentAngle;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        float deltaX = eventData.position.x - startPointerPos.x;
        currentAngle = Mathf.Clamp(startAngle - deltaX * rotationSpeed, minAngle, maxAngle);

        if (knob) knob.localEulerAngles = new Vector3(0, 0, currentAngle);

        UpdateRadioFeedback();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        UpdateRadioFeedback();
    }

    void Update()
    {
        UpdateRadioFeedback();
    }

    private void UpdateRadioFeedback()
    {
        // ========== 1) Needle Movement ==========
        float normalized = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);
        normalized = 1f - Mathf.Clamp01(normalized);

        if (needleIndicator)
        {
            float xPos = Mathf.Lerp(leftX, rightX, normalized);
            var pos = needleIndicator.anchoredPosition;
            needleIndicator.anchoredPosition = new Vector2(xPos, pos.y);
        }

        // ========== 2) Get Signal ==========
        float signal = 0f;
        if (parabola != null && parabola.IsSpawned)
            signal = Mathf.Clamp01(parabola.Signal01.Value);

        // ✅ Update Signal UI (Progress + Color)
        if (signalFill)
        {
            signalFill.fillAmount = signal;
            signalFill.color = Color.Lerp(lowColor, highColor, signal);
        }

        // ========== 3) Audio Logic ==========
        if (signal < minSignalToHear)
        {
            if (correctSound) correctSound.volume = 0f;
            if (noiseSound) noiseSound.volume = 1f;
            return;
        }

        float distance = Mathf.Abs(currentAngle - targetAngle);
        float tuning01 = Mathf.InverseLerp(90f, 0f, distance);

        float clarity = Mathf.Clamp01(signal * tuning01);

        if (correctSound) correctSound.volume = clarity;
        if (noiseSound) noiseSound.volume = 1f - clarity;
    }
}
