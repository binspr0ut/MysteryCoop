using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class SceneFlowManager : NetworkBehaviour
{
    public static SceneFlowManager Instance;

    [Header("Cutscene")]
    [Tooltip("Prefab CutsceneController yang akan di-spawn lokal pada tiap client.")]
    public CutsceneController cutscenePrefab;

    // state sync
    private NetworkVariable<bool> isCutsceneRunning = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> readyCount = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // lokal
    private CutsceneController localCutscene;
    private bool localReported;
    private string pendingNextScene = null;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ===================== PUBLIC API =====================

    /// <summary>Host memerintahkan semua client pindah ke scene biasa.</summary>
    public void ChangeScene(string sceneName)
    {
        if (!IsServer) { Debug.LogWarning("[SceneFlow] ChangeScene hanya boleh oleh host."); return; }
        StartCoroutine(LoadSceneForAll(sceneName));
    }

    /// <summary>Host memerintahkan semua client pindah ke scene cutscene dan memainkannya sinkron.
    /// Jika nextSceneName tidak null/kosong, otomatis pindah ke scene itu setelah semua player selesai.</summary>
    public void PlayCutscene(string cutsceneSceneName, string nextSceneName = null)
    {
        if (!IsServer) { Debug.LogWarning("[SceneFlow] PlayCutscene hanya boleh oleh host."); return; }
        pendingNextScene = string.IsNullOrWhiteSpace(nextSceneName) ? null : nextSceneName;
        StartCoroutine(LoadCutsceneForAll(cutsceneSceneName));
    }

    // ===================== CORE FLOW ======================
    private IEnumerator LoadSceneForAll(string sceneName)
    {
        pendingNextScene = null; // ini pure change scene
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        yield return null;
    }

    private IEnumerator LoadCutsceneForAll(string sceneName)
    {
        readyCount.Value = 0;
        isCutsceneRunning.Value = false;
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        // beri 1 frame supaya scene ter-load
        yield return null;
        yield return new WaitForSeconds(0.25f);
        StartCutsceneClientRpc();
    }

    [ClientRpc]
    private void StartCutsceneClientRpc()
    {
        // reset lokal
        localReported = false;
        isCutsceneRunning.Value = true;

        // spawn cutscene lokal
        if (localCutscene != null) Destroy(localCutscene.gameObject);
        localCutscene = Instantiate(cutscenePrefab);
        localCutscene.Play(OnLocalCutsceneEnd);
    }

    private void OnLocalCutsceneEnd()
    {
        if (localReported) return;
        localReported = true;
        ReportCutsceneDoneServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReportCutsceneDoneServerRpc(ServerRpcParams _ = default)
    {
        readyCount.Value++;

        // jumlah client aktif (host termasuk)
        int total = NetworkManager.Singleton.ConnectedClients.Count;
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
        if (localCutscene != null) Destroy(localCutscene.gameObject);
        if (IsServer && !string.IsNullOrEmpty(nextScene))
        {
            // hanya host yang memanggil load scene berikutnya
            NetworkManager.SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }
    }

    private void Update()
    {
        // input skip universal (host & client)
        if (isCutsceneRunning.Value && localCutscene != null)
        {
            bool pressed = Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.touchCount > 0;
            if (pressed) localCutscene.SkipToEnd();
        }
    }
}
