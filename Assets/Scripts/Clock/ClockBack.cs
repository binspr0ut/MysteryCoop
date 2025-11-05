using Unity.Netcode;
using UnityEngine;

public class ClockBack : NetworkBehaviour, IObject, IStateObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject controlUI;
    [SerializeField] private GameObject clockBackUIPanel;

    [Header("Puzzle Elements")]
    [SerializeField] private GameObject batteryUI;   // battery di UI (hanya tampil jika Box solved)
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

        if (clockBackUIPanel != null)
        {
            clockBackUIPanel.SetActive(false);
            _puzzleUI = clockBackUIPanel.GetComponent<ClockBackUI>();
            if (_puzzleUI != null)
                _puzzleUI.onPuzzleDone += HandlePuzzleDoneLocal;
        }

        if (batteryUI != null)
            batteryUI.SetActive(false);
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

    public bool CanInteract() => !_isSolvedNet.Value;

    public void Interact(Transform player)
    {
        // Hanya Detective yang boleh membuka panel
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
        if (batteryUI)
            batteryUI.SetActive(boxDependency != null && boxDependency.isSolved);
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

        // (Opsional) Sembunyikan battery UI setelah solved
        if (batteryUI != null)
            batteryUI.SetActive(!solved);
    }
}
