using System;
using Unity.Netcode;
using UnityEngine;

public class Lift : NetworkBehaviour, IObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("Lift UI References")]
    public GameObject LiftOverlay;
    public GameObject ControlUI;

    private ulong interactorClientId; // simpan siapa yang terakhir interaksi

    public bool CanInteract() => true;

    // Dijalankan saat player tekan tombol "E" atau setara
    public void Interact(Transform playerTransform)
    {
        ControlUI.SetActive(false);
        LiftOverlay.SetActive(true);
        IsInteracted = true;

        var netObj = playerTransform.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            interactorClientId = netObj.OwnerClientId;
            Debug.Log($"Lift interacted by client {interactorClientId}");
        }
        else
        {
            Debug.LogWarning("Player has no NetworkObject!");
        }
    }

    public void CloseLift()
    {
        Debug.Log("CloseLift triggered");
        LiftOverlay?.SetActive(false);
        ControlUI?.SetActive(true);
        IsInteracted = false;
    }

    // Dipanggil oleh tombol UI (EventTrigger)
    public void GoUp()
    {
        Debug.Log("GoUp UI pressed");

        if (IsServer)
        {
            // kirim perintah ke client owner untuk teleport dirinya
            GoUpClientRpc(interactorClientId);
        }
        else
        {
            // kirim ke server dulu, baru server broadcast ke owner
            GoUpServerRpc();
        }
        CloseLift();
    }

    public void Go3rdFloor()
    {
        Debug.Log("Go 3rd Floor Pressed");

        if (IsServer)
        {
            // kirim perintah ke client owner untuk teleport dirinya
            GoUp3rdFloorClientRpc(interactorClientId);
        }
        else
        {
            // kirim ke server dulu, baru server broadcast ke owner
            Go3rdFloorServerRpc();
        }
        CloseLift();
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void GoUpServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Server received GoUp request");
        GoUpClientRpc(interactorClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void Go3rdFloorServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Server received GoUp request");
        GoUp3rdFloorClientRpc(interactorClientId);
    }

    [ClientRpc]
    private void GoUpClientRpc(ulong targetClientId)
    {
        // hanya player owner yang eksekusi ini
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().transform;
        Vector3 newPos = player.position;
        newPos.y += 10f;
        player.position = newPos;

        Debug.Log($"[Client {targetClientId}] moved self up to {newPos}");
    }

    [ClientRpc]
    private void GoUp3rdFloorClientRpc(ulong targetClientId)
    {
        // hanya player owner yang eksekusi ini
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().transform;
        Vector3 newPos = player.position;
        newPos.y += 20f;
        player.position = newPos;

        Debug.Log($"[Client {targetClientId}] moved self up to {newPos}");
    }

    private void Start()
    {
        ID ??= GlobalHelper.GenerateUniqueID(gameObject);
        if (LiftOverlay != null)
            LiftOverlay.SetActive(false);
    }
}