using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleNetworkManager : NetworkBehaviour
{
    public static PuzzleNetworkManager Instance;

    [Header("Map & Ending UI References")]
    public GameObject mapObject;
    public GameObject toBeContinuedPanel; // Drag "ToBeContinued" GameObject here in Inspector

    void Awake()
    {
        Instance = this;
    }

    // ===========================================================
    // 🧩 Puzzle Solved (existing)
    // ===========================================================
    [ServerRpc(RequireOwnership = false)]
    public void PuzzleSolvedServerRpc()
    {
        Debug.Log("✅ Puzzle solved (server notified)");
        ActivateMapClientRpc();
    }

    [ClientRpc]
    private void ActivateMapClientRpc()
    {
        if (mapObject != null)
            mapObject.SetActive(true);
    }

    public void ActivateMapForOfflineTest()
    {
        if (mapObject != null)
            mapObject.SetActive(true);
    }

    // ===========================================================
    // 🚉 Train Station Button Trigger
    // ===========================================================
    [ServerRpc(RequireOwnership = false)]
    public void ShowToBeContinuedServerRpc()
    {
        Debug.Log("🚉 Train Station button clicked — notifying all clients...");
        ShowToBeContinuedClientRpc();
    }

    [ClientRpc]
    private void ShowToBeContinuedClientRpc()
    {
        if (toBeContinuedPanel != null)
        {
            toBeContinuedPanel.SetActive(true);
            Debug.Log("✨ ToBeContinued panel activated on client");
        }
    }
}
