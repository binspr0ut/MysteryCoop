using Unity.Netcode;
using UnityEngine;

public class Clock : NetworkBehaviour, IPossess, IStateObject
{
    public bool IsPossessed { get; private set; }
    public string ID { get; private set; }

    [Header("Clock Puzzle UI")]
    public GameObject ClockPuzzleUI;
    public GameObject ControlUI;

    public bool isSolved = false;
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

    // === INTERACTION ==
    public void Interact(Transform playerTransform)
    {
        if (IsPossessed)
        {
            Debug.Log("Unposess lampu belajar");
            Unpossess();
            IsPossessed = false;
        }
        else
        {
            Debug.Log("Possess lampu belajar");
            Possess();
            IsPossessed = true;
        }
    }

    public void ClosePuzzle()
    {
        ControlUI.SetActive(true);
        ClockPuzzleUI.SetActive(false);
        IsPossessed = false;
    }

    // === POSSESSION SYSTEM ===
    public void Possess()
    {
        if (IsPossessed) return;

        // Cari spirit milik player lokal
        var spirit = FindFirstObjectByType<SpiritMovement>();
        if (spirit != null && spirit.IsOwner)
        {
            // 🔹 Sembunyikan spirit di semua client
            spirit.SetVisibleServerRpc(false);
            PossessedSpirit = spirit;
        }

        if (isSolved)
        {
            Debug.Log("[Clock] Puzzle already solved.");
            return;
        }

        if (currentState == ObjectState.Disabled) return;

        if (currentState == ObjectState.Locked)
        {
            var puzzle = ClockPuzzleUI.GetComponentInChildren<ClockPuzzle>();
            if (puzzle != null && puzzle.alarmSound != null)
                puzzle.alarmSound.volume = 0f;


            ControlUI.SetActive(false);
            ClockPuzzleUI.SetActive(true);
            IsPossessed = true;
        }

        if (currentState == ObjectState.Active)
        {
            var puzzle = ClockPuzzleUI.GetComponentInChildren<ClockPuzzle>();
            if (puzzle != null && puzzle.alarmSound != null)
                puzzle.alarmSound.volume = 1f;


            ControlUI.SetActive(false);
            ClockPuzzleUI.SetActive(true);
            IsPossessed = true;
        }

        Debug.Log("[Clock] Possessed and puzzle opened.");
    }

    public void Interact()
    {
        if (IsPossessed)
        {
            Debug.Log("Unposess Clock");
            Unpossess();
            IsPossessed = false;
        }
        else
        {
            Debug.Log("Possess Clock");
            Possess();
            IsPossessed = true;
        }
    }

    public bool CanPossess() => currentState == ObjectState.Active || currentState == ObjectState.Locked;

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
        IsPossessed = false;

        Debug.Log("[Clock] Puzzle solved and updated for all clients.");
    }
}
