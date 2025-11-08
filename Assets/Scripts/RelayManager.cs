// // using UnityEngine;
// // using Unity.Services.Relay;
// // using Unity.Services.Relay.Models;
// // using Unity.Services.Core;
// // using Unity.Services.Authentication;
// // using Unity.Netcode;
// // using Unity.Netcode.Transports.UTP;
// // using Unity.Networking.Transport.Relay;
// // using System.Threading.Tasks;

// // public class RelayManager : MonoBehaviour
// // {
// //     public static RelayManager Instance;

// //     private void Awake()
// //     {
// //         if (Instance == null)
// //             Instance = this;
// //         else
// //             Destroy(gameObject);
// //     }

// //     private async void Start()
// //     {
// //         await UnityServices.InitializeAsync();

// //         AuthenticationService.Instance.SignedIn += () =>
// //         {
// //             Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
// //         };

// //         await AuthenticationService.Instance.SignInAnonymouslyAsync();
// //     }

// //     // === HOST ===
// //     public async Task<string> CreateRelayAsync()
// //     {
// //         try
// //         {
// //             Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
// //             string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

// //             Debug.Log($"Relay created. Join code: {joinCode}");

// //             var relayServerData = new RelayServerData(allocation, "dtls");
// //             NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
// //             NetworkManager.Singleton.StartHost();

// //             return joinCode;
// //         }
// //         catch (RelayServiceException e)
// //         {
// //             Debug.LogError(e);
// //             return null;
// //         }
// //     }

// //     // === CLIENT ===
// //     public async Task JoinRelayAsync(string joinCode)
// //     {
// //         try
// //         {
// //             Debug.Log("Joining relay with code: " + joinCode);

// //             JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
// //             var relayServerData = new RelayServerData(joinAllocation, "dtls");

// //             NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
// //             NetworkManager.Singleton.StartClient();

// //             Debug.Log("Client started!");
// //         }
// //         catch (RelayServiceException e)
// //         {
// //             Debug.LogError(e);
// //         }
// //     }
// // }

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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // ============= CUTSCENE READY HOOKS =============
    // >>> Daftarkan callback ini SEBELUM StartHost/StartClient
    private void RegisterCutsceneReadyHook()
    {
        // Hindari double-subscribe
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        // Hanya kirim "ready" untuk diri kita sendiri
        if (clientId != NetworkManager.Singleton.LocalClientId) return;

        var cutsceneMgr = FindObjectOfType<CutsceneTimelineManager_NG>();
        if (cutsceneMgr != null)
        {
            Debug.Log("[RelayManager] Sending cutscene READY to server.");
            cutsceneMgr.ClientReadyForCutsceneServerRpc();
        }
        else
        {
            Debug.LogWarning("[RelayManager] CutsceneTimelineManager_NG not found in scene.");
        }

        // Unsubscribe supaya cuma kirim sekali
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }
    // =================================================

    // === HOST ===
    public async Task<string> CreateRelayAsync()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"Relay created. Join code: {joinCode}");

            var relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // >>> DAFTARKAN HOOK sebelum StartHost
            RegisterCutsceneReadyHook();

            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    // === CLIENT ===
    public async Task JoinRelayAsync(string joinCode)
    {
        try
        {
            Debug.Log("Joining relay with code: " + joinCode);

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // >>> DAFTARKAN HOOK sebelum StartClient
            RegisterCutsceneReadyHook();

            NetworkManager.Singleton.StartClient();

            Debug.Log("Client started!");
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }
}