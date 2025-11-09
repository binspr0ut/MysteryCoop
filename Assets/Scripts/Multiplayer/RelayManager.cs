using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // tetap hidup di semua scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Signed in as: {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // ============================================================
    // === HOST ===
    // ============================================================
    public async Task<string> CreateRelayAsync()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"[RelayManager] Relay created. Join code: {joinCode}");

            var relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // ✅ Hook event hanya sekali, untuk log join client
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            NetworkManager.Singleton.StartHost();
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    // ============================================================
    // === CLIENT ===
    // ============================================================
    public async Task<bool> JoinRelayAsync(string joinCode)
    {
        try
        {
            Debug.Log($"[RelayManager] Joining relay with code: {joinCode}");

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // ✅ Pasang callback untuk deteksi koneksi sukses
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            NetworkManager.Singleton.StartClient();
            Debug.Log("[RelayManager] Client started!");
            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    // ============================================================
    // === CONNECTION EVENTS ===
    // ============================================================
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[RelayManager] Client connected: {clientId}");

        // // Kalau sudah sampai di scene Cutscene, biarkan CutsceneManager-nya yang handle
        // var cutsceneMgr = FindObjectOfType<CutsceneTimelineManager>();
        // if (cutsceneMgr != null && NetworkManager.Singleton.IsClient)
        // {
        //     Debug.Log("[RelayManager] Auto notifying CutsceneTimelineManager (client ready).");
        //     cutsceneMgr.ClientReadyForCutsceneServerRpc();
        // }
        // else
        // {
        //     // kalau belum di scene cutscene, ini cuma log biasa
        //     Debug.Log("[RelayManager] Scene belum cutscene, skip CutsceneReady call.");
        // }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[RelayManager] Client disconnected: {clientId}");
    }
}
