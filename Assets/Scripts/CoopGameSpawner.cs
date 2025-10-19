using Unity.Netcode;
using UnityEngine;

public class CoopGameSpawner : NetworkBehaviour
{
    [Header("Prefabs")]
    public GameObject detectivePrefab;
    public GameObject spiritPrefab;

    [Header("Spawn Points")]
    public Transform detectiveSpawn;
    public Transform spiritSpawn;

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    override public void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    private void HandleServerStarted()
    {
        if (!IsServer) return;

        // === Spawn Detective for Host ===
        ulong hostClientId = NetworkManager.Singleton.LocalClientId;
        var detective = Instantiate(detectivePrefab, detectiveSpawn.position, Quaternion.identity);
        detective.GetComponent<NetworkObject>().SpawnAsPlayerObject(hostClientId);

        Debug.Log($"[Server] Spawned Detective for Host {hostClientId}");
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        // === Prevent spawning for host ===
        if (clientId == NetworkManager.Singleton.LocalClientId)
            return;

        // === Spawn Spirit for Client ===
        var spirit = Instantiate(spiritPrefab, spiritSpawn.position, Quaternion.identity);
        spirit.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        Debug.Log($"[Server] Spawned Spirit for Client {clientId}");
    }
}
