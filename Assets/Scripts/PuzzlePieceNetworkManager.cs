using Unity.Netcode;
using UnityEngine;

public class PuzzlePieceNetworkManager : NetworkBehaviour
{
    public static PuzzlePieceNetworkManager Instance;

    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PuzzleSolvedServerRpc()
    {
        Debug.Log("PUZZLE SOLVED!");
        PuzzleSolvedClientRpc();
    }

    [ClientRpc]
    void PuzzleSolvedClientRpc()
    {
        // contoh: buka map, scene, cutscene, dsb
        Debug.Log("SHOW MAP!");
    }
}
