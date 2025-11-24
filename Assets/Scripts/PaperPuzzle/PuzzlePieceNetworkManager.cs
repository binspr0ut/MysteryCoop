using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PuzzlePieceNetworkManager : NetworkBehaviour
{
    public static PuzzlePieceNetworkManager Instance;

    public GameObject panel;

    public override void OnNetworkSpawn()
    {
        Instance = this;
        panel.GetComponent<CanvasGroup>().alpha = 0;
        panel.GetComponent<CanvasGroup>().interactable = false;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = false;
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
