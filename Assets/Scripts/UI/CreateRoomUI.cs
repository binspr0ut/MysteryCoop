using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class CreateRoomUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text txtInviteCode;
    [SerializeField] private TMP_Text txtStatus;
    [SerializeField] private Button btnCopy;
    [SerializeField] private Button btnStart;
    [SerializeField] private RelayManager relayManager;
    [SerializeField] private string cutsceneScene = "TextObjective";

    private string joinCode = "";

    private async void OnEnable()
    {
        // Tampilkan status awal
        txtInviteCode.text = "Creating room...";
        txtStatus.text = "Waiting for player to join...";
        btnStart.interactable = false; // nonaktif di awal
        btnStart.GetComponentInChildren<TMP_Text>().text = "Waiting...";

        // Buat relay room
        joinCode = await relayManager.CreateRelayAsync();
        txtInviteCode.text = string.IsNullOrEmpty(joinCode) ? "Failed" : joinCode;

        // Tambahkan listener hanya jika host
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientJoined;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientLeft;
        }
    }

    private void Start()
    {
        btnCopy.onClick.AddListener(() => GUIUtility.systemCopyBuffer = joinCode);
        btnStart.onClick.AddListener(StartCutscene);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientJoined;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientLeft;
        }
    }

    private void OnClientJoined(ulong clientId)
    {
        // Karena host juga dihitung sebagai client 1
        int playerCount = NetworkManager.Singleton.ConnectedClients.Count;

        if (playerCount >= 2)
        {
            txtStatus.text = "Player joined!";
            btnStart.interactable = true;
            btnStart.GetComponentInChildren<TMP_Text>().text = "Start Game";
        }
    }

    private void OnClientLeft(ulong clientId)
    {
        int playerCount = NetworkManager.Singleton.ConnectedClients.Count;

        if (playerCount < 2)
        {
            txtStatus.text = "Waiting for player to join...";
            btnStart.interactable = false;
            btnStart.GetComponentInChildren<TMP_Text>().text = "Waiting...";
        }
    }

    private void StartCutscene()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            txtStatus.text = "Loading intro...";
            btnStart.interactable = false;

            // Gunakan SceneFlowManager untuk memulai cutscene bersama
            SceneFlowManager.Instance.PlayCutscene("IntroCutscene", "FirstFloor");
        }
    }

}
