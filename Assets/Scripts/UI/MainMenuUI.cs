using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;

/// <summary>
/// Main menu + room flow dalam 1 scene:
/// - Panel_Main            : Start/Chapter/Settings (placeholder)
/// - Panel_RoomSelect      : Create vs Join
/// - Panel_CreateRoom      : Host create relay, tampilkan kode, host Start Cutscene
/// - Panel_JoinRoom        : Client input kode, join, lalu menunggu host
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelRoomSelect;
    [SerializeField] private GameObject panelCreateRoom;
    [SerializeField] private GameObject panelJoinRoom;

    [Header("Main Buttons")]
    [SerializeField] private Button btnStartGame;
    [SerializeField] private Button btnChapterSelect;
    [SerializeField] private Button btnSettings;

    [Header("Room Select Buttons")]
    [SerializeField] private Button btnCreateRoom;
    [SerializeField] private Button btnJoinRoom;
    [SerializeField] private Button btnBackFromRoomSelect;

    [Header("Create Room Page")]
    [SerializeField] private TMP_Text txtInviteCode;        // contoh: "AXBCD12S3"
    [SerializeField] private Button btnCopyCode;            // opsional
    [SerializeField] private Button btnStartCutscene;       // host-only
    [SerializeField] private Button btnBackFromCreate;

    [Header("Join Room Page")]
    [SerializeField] private TMP_InputField inputJoinCode;
    [SerializeField] private Button btnDoJoin;
    [SerializeField] private TMP_Text txtJoinStatus;
    [SerializeField] private Button btnBackFromJoin;

    [Header("Refs")]
    [SerializeField] private RelayManager relayManager;     // drag RelayManager di scene
    [SerializeField] private string cutsceneSceneName = "TextObjective";

    private string _lastJoinCode = "";

    private void Awake()
    {
        // Fallback cari RelayManager kalau belum di-drag
        if (relayManager == null) relayManager = FindObjectOfType<RelayManager>(true);

        // Bind klik tombol
        btnStartGame?.onClick.AddListener(() => ShowPanel(panelRoomSelect));
        btnChapterSelect?.onClick.AddListener(() => Debug.Log("Chapter Select (placeholder)"));
        btnSettings?.onClick.AddListener(() => Debug.Log("Settings (placeholder)"));

        btnCreateRoom?.onClick.AddListener(OnCreateRoomClicked);
        btnJoinRoom?.onClick.AddListener(() => ShowPanel(panelJoinRoom));
        btnBackFromRoomSelect?.onClick.AddListener(() => ShowPanel(panelMain));

        btnCopyCode?.onClick.AddListener(CopyInviteCodeToClipboard);
        btnStartCutscene?.onClick.AddListener(HostStartCutscene);
        btnBackFromCreate?.onClick.AddListener(() => ShowPanel(panelRoomSelect));

        btnDoJoin?.onClick.AddListener(OnDoJoinClicked);
        btnBackFromJoin?.onClick.AddListener(() => ShowPanel(panelRoomSelect));
    }

    private void Start()
    {
        // Tampilan awal
        ShowPanel(panelMain);
        if (txtInviteCode) txtInviteCode.text = "-";
        if (txtJoinStatus) txtJoinStatus.text = "";
        if (inputJoinCode) inputJoinCode.text = "";
    }

    // === Helper untuk mengatur tampilan panel ===
    private void ShowPanel(GameObject target)
    {
        if (panelMain)       panelMain.SetActive(target == panelMain);
        if (panelRoomSelect) panelRoomSelect.SetActive(target == panelRoomSelect);
        if (panelCreateRoom) panelCreateRoom.SetActive(target == panelCreateRoom);
        if (panelJoinRoom)   panelJoinRoom.SetActive(target == panelJoinRoom);
    }

    // REATE ROOM (HOST)
    private async void OnCreateRoomClicked()
    {
        if (relayManager == null)
        {
            Debug.LogError("[MainMenuUI] RelayManager belum di-assign.");
            return;
        }

        ShowPanel(panelCreateRoom);
        if (txtInviteCode) txtInviteCode.text = "Creating relay...";

        // Panggil Unity Relay → StartHost + dapat join code
        string joinCode = await relayManager.CreateRelayAsync();
        _lastJoinCode = joinCode;

        if (string.IsNullOrEmpty(joinCode))
        {
            if (txtInviteCode) txtInviteCode.text = "Failed to create room";
            return;
        }

        if (txtInviteCode) txtInviteCode.text = joinCode;

        // Host menunggu client → tombol StartCutscene aktif kalau mau manual
        if (btnStartCutscene) btnStartCutscene.interactable = true;
    }

    private void CopyInviteCodeToClipboard()
    {
        if (string.IsNullOrEmpty(_lastJoinCode)) return;
        GUIUtility.systemCopyBuffer = _lastJoinCode;
        Debug.Log($"Copied: {_lastJoinCode}");
    }

    private void HostStartCutscene()
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("[MainMenuUI] StartCutscene hanya untuk Host.");
            return;
        }

        // Load scene berbasis NGO agar semua client ikut
        NetworkManager.Singleton.SceneManager.LoadScene(cutsceneSceneName, LoadSceneMode.Single);
    }

    // JOIN ROOM (CLIENT)
    private async void OnDoJoinClicked()
    {
        if (relayManager == null)
        {
            Debug.LogError("[MainMenuUI] RelayManager belum di-assign.");
            return;
        }

        string code = inputJoinCode ? inputJoinCode.text.Trim() : "";
        if (string.IsNullOrEmpty(code))
        {
            if (txtJoinStatus) txtJoinStatus.text = "Code is empty.";
            return;
        }

        if (txtJoinStatus) txtJoinStatus.text = "Joining...";
        btnDoJoin.interactable = false;

        await relayManager.JoinRelayAsync(code);

        // Jika sukses, NetworkManager akan menjadi client.
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            if (txtJoinStatus) txtJoinStatus.text = "Joined. Waiting for host...";
            // Client menunggu Host melakukan SwitchScene ke cutscene.
            // (Kalau kamu ingin tombol Join memaksa mulai cutscene,
            //  kita bisa tambahkan RPC ke host di sini.)
        }
        else
        {
            if (txtJoinStatus) txtJoinStatus.text = "Join failed.";
            btnDoJoin.interactable = true;
        }
    }
}

// using System;
// using System.Threading.Tasks;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
// using Unity.Netcode;
// using UnityEngine.SceneManagement;

// public class MainMenuUI : MonoBehaviour
// {
//     [Header("Panels")]
//     [SerializeField] private GameObject panelMain;           // Start Screen
//     [SerializeField] private GameObject panelRoom;           // Menu pilih Create/Join
//     [SerializeField] private GameObject panelCreateRoom;     // Page Create
//     [SerializeField] private GameObject panelJoinRoom;       // Page Join

//     [Header("Main Buttons")]
//     [SerializeField] private Button btnStartGame;
//     [SerializeField] private Button btnChapterSelect;
//     [SerializeField] private Button btnSettings;

//     [Header("Room Select Buttons")]
//     [SerializeField] private Button btnCreateRoom;
//     [SerializeField] private Button btnJoinRoom;
//     [SerializeField] private Button btnBackFromRoom;

//     [Header("Create Room Page")]
//     [SerializeField] private TMP_Text txtInviteCode;
//     [SerializeField] private Button btnCopyCode;
//     [SerializeField] private Button btnStartCutscene; // host lanjut ke cutscene (switch scene)
//     [SerializeField] private Button btnBackFromCreate;

//     [Header("Join Room Page")]
//     [SerializeField] private TMP_InputField inputJoinCode;
//     [SerializeField] private Button btnDoJoin;
//     [SerializeField] private TMP_Text txtJoinStatus;
//     [SerializeField] private Button btnBackFromJoin;

//     [Header("Refs")]
//     [SerializeField] private RelayManager relayManager;
//     [SerializeField] private string cutsceneSceneName = "TextObjective";

//     private void Awake()
//     {
//         // Default state
//         ShowPanel(panelMain);

//         // Wire buttons
//         btnStartGame.onClick.AddListener(() => ShowPanel(panelRoom));
//         btnChapterSelect.onClick.AddListener(() => Debug.Log("Chapter Select (placeholder)"));
//         btnSettings.onClick.AddListener(() => Debug.Log("Settings (placeholder)"));

//         btnCreateRoom.onClick.AddListener(OnCreateRoomClicked);
//         btnJoinRoom.onClick.AddListener(() => ShowPanel(panelJoinRoom));
//         btnBackFromRoom.onClick.AddListener(() => ShowPanel(panelMain));

//         btnBackFromCreate.onClick.AddListener(() => ShowPanel(panelRoom));
//         btnBackFromJoin.onClick.AddListener(() => ShowPanel(panelRoom));

//         btnCopyCode.onClick.AddListener(CopyCodeToClipboard);
//         btnDoJoin.onClick.AddListener(OnJoinClicked);
//         btnStartCutscene.onClick.AddListener(OnHostStartCutscene);
//     }

//     private void ShowPanel(GameObject target)
//     {
//         panelMain.SetActive(false);
//         panelRoom.SetActive(false);
//         panelCreateRoom.SetActive(false);
//         panelJoinRoom.SetActive(false);

//         target.SetActive(true);
//     }

//     private async void OnCreateRoomClicked()
//     {
//         ShowPanel(panelCreateRoom);
//         txtInviteCode.text = "Creating...";
//         try
//         {
//             var code = await relayManager.CreateRelayAsync();
//             txtInviteCode.text = code;
//         }
//         catch (Exception e)
//         {
//             Debug.LogError(e);
//             txtInviteCode.text = "Failed to create room";
//         }
//     }

//     private async void OnJoinClicked()
//     {
//         var raw = inputJoinCode.text ?? "";
//         var code = raw.Trim().ToUpperInvariant().Replace(" ", "");
//         txtJoinStatus.text = "Joining...";
//         try
//         {
//             await relayManager.JoinRelayAsync(code);
//             txtJoinStatus.text = "Joined! Waiting host...";
//         }
//         catch (Exception e)
//         {
//             Debug.LogError(e);
//             txtJoinStatus.text = $"Join failed: {e.Message}";
//         }
//     }

//     private void CopyCodeToClipboard()
//     {
// #if UNITY_EDITOR
//         GUIUtility.systemCopyBuffer = txtInviteCode.text;
// #endif
//         Debug.Log($"Copied: {txtInviteCode.text}");
//     }

//     private void OnHostStartCutscene()
//     {
//         if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
//         {
//             Debug.LogWarning("Only host can start cutscene.");
//             return;
//         }

//         // Pindah scene networked ke TextObjective
//         NetworkManager.Singleton.SceneManager.LoadScene(cutsceneSceneName, LoadSceneMode.Single);
//     }
// }