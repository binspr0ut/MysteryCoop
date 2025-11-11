using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class ClockBack : NetworkBehaviour, IObject, IStateObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject controlUI;
    [SerializeField] private GameObject clockBackUIPanel;

    [Header("Puzzle Elements")]
    [SerializeField] private GameObject batteryUI1;
    [SerializeField] private GameObject batteryUI2;
    [SerializeField] private Clock clockTarget;      // target jam (punya Collider2D yg awalnya disabled)
    [SerializeField] private Box boxDependency;      // ketergantungan Box

    private ClockBackUI _puzzleUI;

    [Header("Components")]
    [SerializeField] private Collider2D interactionCollider;

    private ObjectState currentState = ObjectState.Disabled;


    public void SetObjectState(ObjectState state)
    {
        currentState = state;

        switch (state)
        {
            case ObjectState.Disabled:
                interactionCollider.enabled = false;
                break;

            case ObjectState.Locked:
                interactionCollider.enabled = true;
                break;

            case ObjectState.Active:
                interactionCollider.enabled = true;
                break;
        }
    }

    // === KUNCI SINKRONISASI ===
    private readonly NetworkVariable<bool> _isSolvedNet = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Start()
    {
        ID ??= System.Guid.NewGuid().ToString();

        batteryUI1.SetActive(false);
        batteryUI2.SetActive(false);

        ClockBackBatterySync.Instance.OnBatteryChanged += HandleBatterySync;
    }
    private void HandleBatterySync(int count)
    {
        if (count >= 1)
            ShowBattery(batteryUI1);

        if (count >= 2)
            ShowBattery(batteryUI2);
    }

    private void ShowBattery(GameObject obj)
    {
        var cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        obj.SetActive(true);
        var scale = obj.transform.localScale;
        obj.transform.localScale = Vector3.zero;
        cg.alpha = 0f;

        LeanTween.scale(obj, scale, 0.3f).setEaseOutBack();
        LeanTween.value(obj, 0f, 1f, 0.3f)
                 .setOnUpdate(v => cg.alpha = v);
    }


    public override void OnNetworkSpawn()
    {
        // Sinkron awal ketika object ini muncul di client mana pun.
        ApplySolvedState(_isSolvedNet.Value);

        // Dengarkan perubahan status agar late-observer / late-join ikut tersinkron.
        _isSolvedNet.OnValueChanged += OnSolvedChanged;
    }

    public override void OnNetworkDespawn()
    {
        _isSolvedNet.OnValueChanged -= OnSolvedChanged;
    }

    private void OnSolvedChanged(bool oldValue, bool newValue)
    {
        ApplySolvedState(newValue);
    }

    public bool CanInteract() => !_isSolvedNet.Value && (currentState == ObjectState.Active || currentState == ObjectState.Locked);

    public void Interact(Transform player)
    {
        if (currentState == ObjectState.Disabled) return;

        if (currentState == ObjectState.Locked)
        {
            Debug.Log("🔒 Objek masih terkunci. Kamu memerlukan kunci.");
            // tampilkan UI "Memerlukan kunci"
            return;
        }

        if (currentState == ObjectState.Active)
        {  // Hanya Detective yang boleh membuka panel
            SubtitleManager.Instance.ShowSubtitle(
                    "Agung: This clock’s stopped. I’ll need to find a battery",
                    SubtitleTarget.Detective,
                    SubtitleScope.Local
                );

            var detective = player.GetComponent<DetectiveMovement>();
            if (detective == null)
            {
                Debug.Log("❌ Only detective can interact with ClockBack!");
                return;
            }

            IsInteracted = true;
            if (controlUI) controlUI.SetActive(false);
            if (clockBackUIPanel) clockBackUIPanel.SetActive(true);

            // Tampilkan battery jika Box sudah solved
            if (batteryUI1 && batteryUI2)
            {
                batteryUI1.SetActive(boxDependency != null && boxDependency.isSolved);
                batteryUI2.SetActive(boxDependency != null && boxDependency.isSolved);
            }
        }
    }

    public void ClosePuzzle()
    {
        IsInteracted = false;
        if (controlUI) controlUI.SetActive(true);
        if (clockBackUIPanel) clockBackUIPanel.SetActive(false);
    }

    // Dipanggil lokal oleh UI saat battery sukses dipasang
    private void HandlePuzzleDoneLocal()
    {
        if (IsServer)
        {
            // Host langsung set NetworkVariable (satu sumber kebenaran)
            SetSolvedOnServer();
        }
        else
        {
            // Client minta server untuk menetapkan solved
            RequestSetSolvedServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSetSolvedServerRpc(ServerRpcParams rpcParams = default)
    {
        SetSolvedOnServer();
    }

    private void SetSolvedOnServer()
    {
        if (_isSolvedNet.Value) return; // idempotent
        _isSolvedNet.Value = true;      // memicu OnValueChanged di semua client saat mereka observer
        // Server juga langsung apply untuk dirinya
        ApplySolvedState(true);
        Debug.Log("✅ ClockBack solved (server authority).");
    }

    /// <summary>
    /// Terapkan status solved secara lokal (dipanggil saat spawn & saat NV berubah).
    /// Aman dipanggil berkali-kali (idempotent).
    /// </summary>
    private void ApplySolvedState(bool solved)
    {
        // Tutup UI kalau kebuka
        if (solved)
            ClosePuzzle();

        // Aktifkan collider & script clock
        if (clockTarget != null)
        {
            var col = clockTarget.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = solved;

            clockTarget.enabled = solved;
        }

        // Tampilkan battery jika Box sudah solved
        if (batteryUI1 && batteryUI2)
        {
            batteryUI1.SetActive(!solved);
            batteryUI2.SetActive(!solved);
        }
    }
}
