using Unity.Netcode;
using UnityEngine;

public class Clock : NetworkBehaviour, IPossess
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("Clock Puzzle UI")]
    public GameObject ClockPuzzleUI;
    public GameObject ControlUI;

    public bool isSolved = false;
    private SpiritMovement PossessedSpirit;

    void Start()
    {
        ID ??= System.Guid.NewGuid().ToString();

        if (ClockPuzzleUI != null)
            ClockPuzzleUI.SetActive(false);

        if (ControlUI != null)
            ControlUI.SetActive(true);

        var col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

    }

    // === INTERACTION ===
    public bool CanInteract() => true;

    public void Interact(Transform playerTransform)
    {
        if (isSolved)
        {
            Debug.Log("[Clock] Puzzle already solved.");
            return;
        }

        ControlUI.SetActive(false);
        ClockPuzzleUI.SetActive(true);
        IsInteracted = true;
    }

    public void ClosePuzzle()
    {
        ControlUI.SetActive(true);
        ClockPuzzleUI.SetActive(false);
        IsInteracted = false;
    }

    // === POSSESSION SYSTEM ===
    public void Possess()
    {
        if (IsInteracted) return;

        // Cari spirit milik player lokal
        var spirit = FindFirstObjectByType<SpiritMovement>();
        if (spirit != null && spirit.IsOwner)
        {
            // 🔹 Sembunyikan spirit di semua client
            spirit.SetVisibleServerRpc(false);
            PossessedSpirit = spirit;
        }

        // 🔹 Tampilkan puzzle jam
        ControlUI.SetActive(false);
        ClockPuzzleUI.SetActive(true);
        IsInteracted = true;

        Debug.Log("[Clock] Possessed and puzzle opened.");
    }

    public void Interact()
    {
        // Tidak dipakai, tapi wajib implement
    }

    public bool CanPossess() => !IsInteracted;

    public void Unpossess()
    {
        if (PossessedSpirit != null)
        {
            // 🔹 Tampilkan kembali spirit di semua client
            PossessedSpirit.SetVisibleServerRpc(true);
            PossessedSpirit = null;
        }

        // 🔹 Tutup UI puzzle dan kembalikan control
        ClosePuzzle();

        Debug.Log("[Clock] Unpossessed and puzzle closed.");
    }

    // === RPCs for puzzle completion ===
    [ServerRpc(RequireOwnership = false)]
    public void SolveClockServerRpc()
    {
        isSolved = true;
        UpdateClockClientRpc();
    }

    [ClientRpc]
    private void UpdateClockClientRpc()
    {
        // Tutup puzzle, sembunyikan UI
        ClockPuzzleUI.SetActive(false);
        ControlUI.SetActive(true);
        IsInteracted = false;

        Debug.Log("[Clock] Puzzle solved and updated for all clients.");
    }
}
