using Unity.Netcode;
using UnityEngine;

public class ShelfLockpick : NetworkBehaviour, IPossess
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI References")]
    public GameObject ControlUI;
    public GameObject LockpickOverlay;
    public GameObject OpenedShelf;

    public GameObject LockpickTrigger;
    public bool isSolved;
    private SpiritMovement PossessedSpirit;

    void Start()
    {
        if (LockpickOverlay != null)
            LockpickOverlay.SetActive(false);
        if (OpenedShelf != null)
            OpenedShelf.SetActive(false);

        // Hubungkan otomatis LockpickUI dengan Shelf ini
        var ui = LockpickOverlay?.GetComponentInChildren<LockpickUI>();
        if (ui != null)
            ui.GetType().GetField("shelfLockpick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              ?.SetValue(ui, this);
    }

    public void Possess()
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
        IsInteracted = true;
    }

    public void Interact()
    {
        return;
    }

    public bool CanPossess() => true;

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
        IsInteracted = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UnlockShelfServerRpc()
    {
        isSolved = true;
        UpdateShelfClientRpc();
    }

    [ClientRpc]
    private void UpdateShelfClientRpc()
    {
        LockpickOverlay.SetActive(false);
        OpenedShelf.SetActive(true);
        LockpickTrigger.SetActive(false);
    }
}
