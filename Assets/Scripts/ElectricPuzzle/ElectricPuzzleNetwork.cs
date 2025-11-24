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

    // =============== CABLE ===============

    [ServerRpc(RequireOwnership = false)]
    public void RequestSetCableConnectedServerRpc(WireColor color, bool connected)
    {
        if (!IsServer) return;

        // Server yang pegang kebenaran state
        ApplyCableConnectedClientRpc(color, connected);
    }

    [ClientRpc]
    private void ApplyCableConnectedClientRpc(WireColor color, bool connected)
    {
        if (puzzle == null) return;

        // Ini dipanggil di HOST dan SEMUA CLIENT
        puzzle.SetCableConnected(color, connected);
    }

    // =============== SWITCH ===============

    [ServerRpc(RequireOwnership = false)]
    public void RequestToggleSwitchServerRpc(WireColor color)
    {
        if (!IsServer) return;

        ApplyToggleSwitchClientRpc(color);
    }

    [ClientRpc]
    private void ApplyToggleSwitchClientRpc(WireColor color)
    {
        if (puzzle == null) return;

        // Logic puzzle jalan di semua client
        puzzle.ToggleSwitch(color);

        // Ambil state baru dari puzzle
        bool state = puzzle.IsSwitchOn(color);

        // Cari semua ElectricSwitch dan update yang warnanya sama
        var switches = GameObject.FindObjectsOfType<ElectricSwitch>(true);
        foreach (var sw in switches)
        {
            if (sw.color == color)
            {
                sw.ApplyState(state);
            }
        }
    }
}