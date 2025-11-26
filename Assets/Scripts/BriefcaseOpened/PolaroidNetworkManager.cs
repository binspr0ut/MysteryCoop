using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleNetworkManager : NetworkBehaviour
{
    public static PuzzleNetworkManager Instance;

    [Header("Map & Ending UI References")]
    public GameObject mapObject;
    public GameObject toBeContinuedPanel; // Drag "ToBeContinued" GameObject here in Inspector
    public NetworkVariable<bool> detectiveSolved = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> spiritSolved = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    public override void OnNetworkSpawn()
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
    public void SubmitSolvedServerRpc(ulong senderId)
    {
        if (senderId == NetworkManager.ServerClientId)
            detectiveSolved.Value = true;
        else
            spiritSolved.Value = true;

        CheckAllSolved();
    }

    private void CheckAllSolved()
    {
        if (detectiveSolved.Value && spiritSolved.Value)
        {
            ShowEndingClientRpc();
        }
    }

    [ClientRpc]
    private void ShowEndingClientRpc()
    {
        SceneFlowManager.Instance.PlayCutscene("EndScene1Cutscene", "MainMenu");
    }

    [ClientRpc]
    public void SetEventSystemStateClientRpc(bool state)
    {
        var es = GameObject.Find("EventSystem");
        if (es != null)
        {
            es.SetActive(state);

        }
        else
        {
            Debug.Log("Event System not found");
        }
        Debug.Log(state);
    }

}
