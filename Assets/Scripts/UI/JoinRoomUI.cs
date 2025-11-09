using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class JoinRoomUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputCode;   // input kode join
    [SerializeField] private TMP_Text txtStatus;
    [SerializeField] private Button btnJoin;

    [Header("Managers")]
    [SerializeField] private RelayManager relayManager;  // drag dari scene menu
    [SerializeField] private string introCutsceneScene = "IntroCutscene"; // scene tujuan setelah join
    [SerializeField] private bool debugOffline = false;

    private void Start()
    {
        btnJoin.onClick.AddListener(OnJoinClicked);
    }

    private async void OnJoinClicked()
    {
        // MODE DEBUG OFFLINE
        if (debugOffline)
        {
            txtStatus.text = "Debug: offline mode";
            SceneManager.LoadScene(introCutsceneScene);
            return;
        }

        // Ambil kode dari input field
        string code = inputCode.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(code))
        {
            txtStatus.text = "Please enter a join code.";
            return;
        }

        btnJoin.interactable = false;
        txtStatus.text = $"Connecting with code {code}...";

        // Jalankan proses join relay
        bool joined = await TryJoinRelayAsync(code);

        if (joined && NetworkManager.Singleton.IsClient)
        {
            txtStatus.text = "Connected! Waiting host...";
            // ✅ Jangan load scene manual di sini
            // Host yang akan trigger LoadScene("IntroCutscene")
        }
        else
        {
            txtStatus.text = "Failed to connect.";
            btnJoin.interactable = true;
        }
    }

    private async Task<bool> TryJoinRelayAsync(string code)
    {
        try
        {
            return await relayManager.JoinRelayAsync(code);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            return false;
        }
    }
}
