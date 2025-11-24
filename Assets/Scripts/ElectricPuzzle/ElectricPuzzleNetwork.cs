using UnityEngine;
using Unity.Netcode;

public class ElectricPuzzleNetwork : NetworkBehaviour
{
    [SerializeField] private ElectricPuzzle puzzle;

    public static ElectricPuzzleNetwork Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Spirit / Detective panggil ini dari client
    [ServerRpc(RequireOwnership = false)]
    public void RequestSetCableConnectedServerRpc(WireColor color, bool connected)
    {
        if (!IsServer) return; // safety
        if (puzzle == null) return;

        puzzle.SetCableConnected(color, connected);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestToggleSwitchServerRpc(WireColor color)
    {
        if (!IsServer) return;
        if (puzzle == null) return;

        puzzle.ToggleSwitch(color);
    }
}