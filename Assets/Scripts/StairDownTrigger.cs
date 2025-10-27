using Unity.Netcode;
using UnityEngine;

public class StairDownTrigger : NetworkBehaviour, IObject
{
    private ulong interactorClientId; // simpan siapa yang terakhir interaksi

    public bool CanInteract() => true;

    public void Interact(Transform player)
    {
        GoBasement();
    }

    public void GoBasement()
    {
        Debug.Log("GoUp UI pressed");

        if (IsServer)
        {
            // kirim perintah ke client owner untuk teleport dirinya
            GoBasementClientRpc(interactorClientId);
        }
        else
        {
            // kirim ke server dulu, baru server broadcast ke owner
            GoBasementServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GoBasementServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Server received GoUp request");
        GoBasementClientRpc(interactorClientId);
    }

    [ClientRpc]
    private void GoBasementClientRpc(ulong targetClientId)
    {
        // hanya player owner yang eksekusi ini
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().transform;
        Vector3 newPos = player.position;
        newPos.y = -10.5f;
        newPos.x = 20f;
        player.position = newPos;

        Debug.Log($"[Client {targetClientId}] moved self up to {newPos}");
    }
    void Start()
    {

    }

    void Update()
    {

    }
}
