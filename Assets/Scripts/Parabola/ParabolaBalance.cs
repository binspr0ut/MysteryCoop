using Unity.Netcode;
using UnityEngine;

public class ParabolaBalance : NetworkBehaviour, IPossess, IStateObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI")]
    public GameObject BalanceUI;
    public GameObject ControlUI;

    [Header("Visual (opsional)")]
    public Transform dishHead;
    [Range(0f, 30f)] public float maxHeadAngle = 15f;
    [Range(0f, 10f)] public float headFollow = 4f;

    [Header("Net")]
    public NetworkVariable<float> Signal01 = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Debug")]
    public bool enableDebug = true;

    [Header("Components")]
    [SerializeField] private Collider2D interactionCollider;

    private ObjectState currentState = ObjectState.Disabled;
    private bool _isPossessed;
    private Quaternion _dishStartRot;

    public void SetObjectState(ObjectState state)
    {
        currentState = state;
        if (interactionCollider) interactionCollider.enabled = state != ObjectState.Disabled;
    }

    public bool CanPossess() => currentState == ObjectState.Active || currentState == ObjectState.Locked;

    public void Possess()
    {
        if (currentState == ObjectState.Disabled) return;
        if (currentState == ObjectState.Locked)
        {
            if (enableDebug) Debug.Log("🔒 Parabola terkunci.");
            return;
        }

        _isPossessed = true;
        OpenPuzzle();
    }

    public void Unpossess()
    {
        _isPossessed = false;
        ClosePuzzle();
    }

    public void Interact() { }

    private void OpenPuzzle()
    {
        if (ControlUI) ControlUI.SetActive(false);

        if (BalanceUI)
        {
            var ui = BalanceUI.GetComponent<ParabolaBalanceUI>();
            if (ui) ui.Init(this);
            BalanceUI.SetActive(true);
        }

        IsInteracted = true;
    }

    public void ClosePuzzle()
    {
        if (BalanceUI) BalanceUI.SetActive(false);
        if (ControlUI) ControlUI.SetActive(true);
        IsInteracted = false;

        if (dishHead) dishHead.localRotation = _dishStartRot;

        // Reset signal when puzzle closed
        ClientSetSignal01(0f);
    }

    // Called every frame by ParabolaBalanceUI
    public void ClientSetSignal01(float s01)
    {
        s01 = Mathf.Clamp01(s01);

        // LOCAL DEBUG only when reaches full strength
        if (enableDebug && s01 >= 1f && Mathf.Approximately(Signal01.Value, 1f) == false)
        {
            Debug.Log($"[ParabolaUI] ✅ Full Signal (Client)");
        }

        if (IsServer)
        {
            Signal01.Value = s01;
            if (enableDebug)
                Debug.Log($"[SignalRPC] 🟩 Server Updated Signal = {s01:F2}");
        }
        else
        {
            if (enableDebug)
                Debug.Log($"[SignalRPC] 📤 Client → Server Sending = {s01:F2}");

            SubmitSignalServerRpc(s01);
        }

        UpdateDishHeadLocal(s01);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitSignalServerRpc(float s01)
    {
        Signal01.Value = Mathf.Clamp01(s01);

        if (enableDebug)
            Debug.Log($"[SignalRPC] 🟪 Server Received & Set Signal = {Signal01.Value:F2}");
    }

    private void UpdateDishHeadLocal(float s01)
    {
        if (!dishHead) return;
        float targetZ = Mathf.Lerp(-maxHeadAngle, maxHeadAngle, s01);
        var e = dishHead.localEulerAngles;
        float z = Mathf.LerpAngle(e.z, targetZ, headFollow * Time.unscaledDeltaTime);
        dishHead.localEulerAngles = new Vector3(e.x, e.y, z);
    }

    private void Start()
    {
        ID ??= GlobalHelper.GenerateUniqueID(gameObject);
        if (BalanceUI) BalanceUI.SetActive(false);
        if (dishHead) _dishStartRot = dishHead.localRotation;
    }
}
