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

    public override void OnNetworkSpawn()
    {
        // hanya server yang melakukan spawn player object
        if (!IsServer) return;

        Debug.Log("[CoopGameSpawner] NetworkSpawn detected, spawning players...");

        foreach (var clientPair in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong clientId = clientPair.ClientId;

            // host (server) = Detective
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                SpawnDetective(clientId);
            }
            else
            {
                SpawnSpirit(clientId);
            }
        }
    }

    private void SpawnDetective(ulong clientId)
    {
        if (NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId) != null)
        {
            Debug.Log($"[Spawner] Player {clientId} already has object, skipping spawn.");
            return;
        }

        var detective = Instantiate(detectivePrefab, detectiveSpawn.position, Quaternion.identity);
        detective.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        Debug.Log($"[CoopGameSpawner] Spawned Detective for Host ({clientId})");
    }

    private void SpawnSpirit(ulong clientId)
    {
        if (NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId) != null)
        {
            Debug.Log($"[Spawner] Player {clientId} already has object, skipping spawn.");
            return;
        }

        var spirit = Instantiate(spiritPrefab, spiritSpawn.position, Quaternion.identity);
        spirit.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        Debug.Log($"[CoopGameSpawner] Spawned Spirit for Client ({clientId})");
    }
}
