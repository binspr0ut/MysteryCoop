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

// using System;
// using System.Threading.Tasks;
// using Unity.Netcode;
// using Unity.Networking.Transport.Relay;
// using Unity.Services.Core;
// using Unity.Services.Authentication;
// using Unity.Services.Relay;
// using Unity.Services.Relay.Models;
// using UnityEngine;
// using Unity.Netcode.Transports.UTP;

// public class RelayManager : MonoBehaviour
// {
//     [Tooltip("Max connections termasuk host")]
//     [SerializeField] private int maxConnections = 2;

//     private UnityTransport _transport;

//     private async Task EnsureInitializedAsync()
//     {
//         if (_transport == null)
//         {
//             var nm = NetworkManager.Singleton;
//             if (!nm)
//                 throw new Exception("NetworkManager not found in scene.");
//             _transport = nm.GetComponent<UnityTransport>();
//             if (!_transport)
//                 throw new Exception("UnityTransport not found on NetworkManager.");
//         }

//         if (UnityServices.State == ServicesInitializationState.Uninitialized)
//         {
//             await UnityServices.InitializeAsync();
//         }

//         if (!AuthenticationService.Instance.IsSignedIn)
//         {
//             await AuthenticationService.Instance.SignInAnonymouslyAsync();
//         }
//     }

//     public async Task<string> CreateRelayAsync()
//     {
//         await EnsureInitializedAsync();

//         try
//         {
//             Allocation alloc = await RelayService.Instance.CreateAllocationAsync(maxConnections - 1);
//             string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

//             var relayData = new RelayServerData(alloc, "dtls");
//             _transport.SetRelayServerData(relayData);

//             Debug.Log($"Relay created. Join code: {joinCode}");

//             // Start host
//             if (!NetworkManager.Singleton.IsListening)
//                 NetworkManager.Singleton.StartHost();

//             return joinCode;
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"CreateRelay failed: {e}");
//             throw;
//         }
//     }

//     public async Task JoinRelayAsync(string code)
//     {
//         await EnsureInitializedAsync();

//         try
//         {
//             // NOTE: code harus uppercase & masih valid
//             JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(code);

//             var relayData = new RelayServerData(joinAlloc, "dtls");
//             _transport.SetRelayServerData(relayData);

//             if (!NetworkManager.Singleton.IsListening)
//                 NetworkManager.Singleton.StartClient();
//         }
//         catch (RelayServiceException rse)
//         {
//             // Contoh error 404 "join code not found"
//             Debug.LogError($"JoinRelay failed: {rse}");
//             throw;
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"JoinRelay failed: {e}");
//             throw;
//         }
//     }
// }