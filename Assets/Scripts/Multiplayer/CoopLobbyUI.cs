using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LobbyUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject lobbyPanel;
    public GameObject debugPanel;

    [Header("UI Objects")]
    public GameObject joinCodeInputObj;
    public GameObject joinCodeDisplayObj;
    public GameObject createButtonObj;
    public GameObject joinButtonObj;
    public GameObject shutdownButtonObj;

    private TMP_InputField joinCodeInput;
    private TMP_Text joinCodeDisplay;
    private bool gameStarted = false;

    [SerializeField] private GameObject detectiveSelectButton;
    [SerializeField] private GameObject spiritSelectButton;


    private void Awake()
    {
        if (joinCodeInputObj) joinCodeInput = joinCodeInputObj.GetComponent<TMP_InputField>();
        if (joinCodeDisplayObj) joinCodeDisplay = joinCodeDisplayObj.GetComponent<TMP_Text>();
    }

    private void Start()
    {
        lobbyPanel.SetActive(true);
        debugPanel.SetActive(false);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;

        if (createButtonObj)
            createButtonObj.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnCreatePressed);
        if (joinButtonObj)
            joinButtonObj.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnJoinPressed);
        if (shutdownButtonObj)
            shutdownButtonObj.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnShutdownPressed);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
    }

    // === HOST: Create Relay ===
    public async void OnCreatePressed()
    {
        string joinCode = await RelayManager.Instance.CreateRelayAsync();

        if (!string.IsNullOrEmpty(joinCode))
        {
            if (joinCodeDisplay)
                joinCodeDisplay.text = $"Join Code: {joinCode}";

            if (NetworkManager.Singleton.IsHost)
                joinCodeDisplay.text = $"Join Code: {joinCode}\nWaiting for player...";

            // Host tetap di lobby menunggu client
            createButtonObj.SetActive(false);
            joinButtonObj.SetActive(false);
            if (joinCodeInputObj) joinCodeInputObj.SetActive(false);
        }
    }

    // === CLIENT: Join Relay ===
    public async void OnJoinPressed()
    {
        if (joinCodeInput == null) return;
        string code = joinCodeInput.text.Trim();

        if (!string.IsNullOrEmpty(code))
        {
            await RelayManager.Instance.JoinRelayAsync(code);
            // Client tetap di lobby sementara, nanti auto-hide saat connected
        }
    }

    private void OnServerStarted()
    {
        Debug.Log("[LobbyUI] Host server started");
    }

    private void OnClientConnected(ulong clientId)
    {
        if (gameStarted) return;

        // Jika host, tunggu minimal 1 client terhubung
        if (NetworkManager.Singleton.IsHost)
        {
            if (NetworkManager.Singleton.ConnectedClientsIds.Count > 1)
            {
                joinCodeDisplay.text = $"Player connected! Starting game...";
                HideLobbyAndShowDebug();
                gameStarted = true;

                // ✅ NEW: Host pindahkan semua ke FirstFloor via NetworkSceneManager
                NetworkManager.Singleton.SceneManager
                    .LoadScene("FirstFloor", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            joinCodeDisplay.text = $"Player connected! Starting game...";

            // Client cukup menunggu — NGO akan auto pindah saat host load scene
            HideLobbyAndShowDebug();
            gameStarted = true;
        }
    }


    private void HideLobbyAndShowDebug()
    {
        lobbyPanel.SetActive(false);
        debugPanel.SetActive(true);
    }

    public void OnShutdownPressed()
    {
        StartCoroutine(RestartSceneClean());
    }

    private IEnumerator RestartSceneClean()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
            yield return new WaitForSeconds(0.1f);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
