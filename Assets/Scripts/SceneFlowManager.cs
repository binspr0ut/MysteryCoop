using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class SceneFlowManager : NetworkBehaviour
{
    public static SceneFlowManager Instance;

    // state sync
    private NetworkVariable<bool> isCutsceneRunning = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> readyCount = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private string pendingNextScene = null;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ===================== PUBLIC API =====================

    /// <summary>Host memerintahkan semua client pindah ke scene biasa (tanpa cutscene).</summary>
    public void ChangeScene(string sceneName)
    {
        if (!IsServer)
        {
            Debug.LogWarning("[SceneFlow] ChangeScene hanya boleh oleh host.");
            return;
        }

        StartCoroutine(LoadSceneForAll(sceneName));
    }

    /// <summary>Host memerintahkan semua client pindah ke scene cutscene. 
    /// Setelah semua pemain melapor selesai, host otomatis load ke scene berikutnya.</summary>
    public void PlayCutscene(string cutsceneSceneName, string nextSceneName = null)
    {
        if (!IsServer)
        {
            Debug.LogWarning("[SceneFlow] PlayCutscene hanya boleh oleh host.");
            return;
        }

        pendingNextScene = string.IsNullOrWhiteSpace(nextSceneName) ? null : nextSceneName;
        StartCoroutine(LoadCutsceneForAll(cutsceneSceneName));
    }

    // ===================== INTERNAL FLOW ======================

    private IEnumerator LoadSceneForAll(string sceneName)
    {
        pendingNextScene = null; // ini pure change scene
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        yield return null;
    }

    private IEnumerator LoadCutsceneForAll(string sceneName)
    {
        readyCount.Value = 0;
        isCutsceneRunning.Value = true;

        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

        // beri sedikit waktu agar scene ter-load
        yield return null;
        yield return new WaitForSeconds(0.25f);

        Debug.Log($"[SceneFlow] All clients now in cutscene scene: {sceneName}");
    }

    // Dipanggil oleh cutscene di masing-masing client setelah selesai
    [ServerRpc(RequireOwnership = false)]
    public void ReportCutsceneDoneServerRpc(ServerRpcParams _ = default)
    {
        readyCount.Value++;

        int total = NetworkManager.Singleton.ConnectedClients.Count;
        Debug.Log($"[SceneFlow] Client reported done ({readyCount.Value}/{total})");

        if (readyCount.Value >= total)
        {
            readyCount.Value = 0;
            isCutsceneRunning.Value = false;
            EndCutsceneClientRpc(pendingNextScene ?? "");
        }
    }

    [ClientRpc]
    private void EndCutsceneClientRpc(string nextScene)
    {
        Debug.Log("[SceneFlow] All players done, ending cutscene");

        if (IsServer && !string.IsNullOrEmpty(nextScene))
        {
            NetworkManager.SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }
    }
}
