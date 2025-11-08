using Unity.Netcode;
using UnityEngine;

public class ShelfLockpick : NetworkBehaviour, IPossess, IStateObject
{
    public bool IsPossessed { get; private set; }
    public string ID { get; private set; }

    [Header("UI References")]
    public GameObject ControlUI;
    public GameObject LockpickOverlay;
    public GameObject OpenedShelf;
    public SpriteRenderer OpenedShelfRenderer;

    public GameObject LockpickTrigger;
    public bool isSolved;
    private SpiritMovement PossessedSpirit;

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

    private void Awake()
    {
        Debug.Log($"ShelfLockpick implements IStateObject? {this is IStateObject}");
    }


    void Start()
    {
        if (LockpickOverlay != null)
            LockpickOverlay.SetActive(false);

        // Hubungkan otomatis LockpickUI dengan Shelf ini
        var ui = LockpickOverlay?.GetComponentInChildren<LockpickUI>();
        if (ui != null)
            ui.GetType().GetField("shelfLockpick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              ?.SetValue(ui, this);
    }

    public void Possess()
    {
        if (currentState == ObjectState.Disabled) return;


        if (currentState == ObjectState.Locked)
        {
            Debug.Log("🔒 Objek masih terkunci. Kamu memerlukan kunci.");
            // tampilkan UI "Memerlukan kunci"
            return;
        }

        if (currentState == ObjectState.Active)
        {
            // cari spirit di sekitar (atau lewat parameter dari PossesDetector)
            var spirit = FindFirstObjectByType<SpiritMovement>();
            if (spirit != null && spirit.IsOwner)
            {
                // 🔹 Panggil RPC agar semua client tahu spirit menghilang
                spirit.SetVisibleServerRpc(false);
                PossessedSpirit = spirit;
            }

            ControlUI.SetActive(false);
            LockpickOverlay.SetActive(true);
            IsPossessed = true;
        }
    }

    public void Interact()
    {
        if (IsPossessed)
        {
            Debug.Log("Unposess ShelfLockpick");
            Unpossess();
            IsPossessed = false;
        }
        else
        {
            Debug.Log("Possess ShelfLockPick");
            Possess();
            IsPossessed = true;
        }
    }

    public bool CanPossess() => currentState == ObjectState.Active || currentState == ObjectState.Locked;

    public void Unpossess()
    {
        if (PossessedSpirit != null)
        {
            PossessedSpirit.SetVisibleServerRpc(true);
            PossessedSpirit = null;
        }

        ClosePuzzle();
    }


    public void ClosePuzzle()
    {
        ControlUI.SetActive(true);
        LockpickOverlay.SetActive(false);
        IsPossessed = false;
    }
    public System.Action OnShelfUnlocked;

    [ServerRpc(RequireOwnership = false)]
    public void UnlockShelfServerRpc()
    {
        isSolved = true;
        UpdateShelfClientRpc();

        // 🔥 Trigger event untuk beri tahu State Manager
        OnShelfUnlocked?.Invoke();

    }

    [ClientRpc]
    private void UpdateShelfClientRpc()
    {
        LockpickOverlay.SetActive(false);
        OpenedShelfRenderer.enabled = true;
        LockpickTrigger.SetActive(false);
    }
}
