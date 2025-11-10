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

    private bool _showNoteOnNextScene = false;

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

        // minta FirstFloor nanti menampilkan note overlay
        _showNoteOnNextScene = true;

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

    private System.Collections.IEnumerator ShowIntroNoteWhenReady()
    {
        // Tunggu beberapa frame sampai scene gameplay aktif & object overlay sudah ada
        for (int i = 0; i < 120; i++) // ~2 detik max
        {
            var overlay = FindObjectOfType<ReadNoteOverlay>(true);
            if (overlay != null)
            {
                overlay.Show();    // << tampilkan overlay di client ini
                yield break;
            }
            yield return null;
        }
        Debug.LogWarning("[SceneFlow] ReadNoteOverlay not found in this scene.");
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

        // Daftarkan listener di SEMUA client, bukan hanya host, dan jangan pakai flag
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoadedOnce;

        if (IsServer && !string.IsNullOrEmpty(nextScene))
        {
            NetworkManager.SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }
    }

    // === B. OnSceneLoadedOnce ===
    private void OnSceneLoadedOnce(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoadedOnce;

        // Tampilkan overlay di setiap client setelah scene aktif
        var note = FindObjectOfType<ReadNoteOverlay>(true);
        if (note != null) note.Show();
        else Debug.LogWarning("[SceneFlow] ReadNoteOverlay not found in scene.");
    }

}
