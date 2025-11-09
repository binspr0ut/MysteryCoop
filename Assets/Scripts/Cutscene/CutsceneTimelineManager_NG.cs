using UnityEngine;
using Unity.Netcode;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using TMPro; // jika kamu mau mengubah teks hint
public class CutsceneTimelineManager : NetworkBehaviour
{
    [Header("Cutscenes in Order")]
    public PlayableDirector[] directors; // isi 10 director dari Hierarchy
    public CanvasGroup fadeCanvas;
    public GameObject skipHint;

    private int index = 0;
    private bool localDone = false;

    private void Start()
    {
        if (IsServer)
            StartNextCutsceneClientRpc(0);
    }

    // [ServerRpc(RequireOwnership = false)]
    // public void ClientReadyForCutsceneServerRpc(ServerRpcParams rpcParams = default)
    // {
    //     // Player baru bergabung, langsung ikut cutscene saat ini
    //     Debug.Log($"[CutsceneTimelineManager] ClientReady dari {rpcParams.Receive.SenderClientId}");
    //     if (IsServer)
    //     {
    //         // kirim ulang cutscene saat ini ke client yang baru ready
    //         StartNextCutsceneClientRpc(index);
    //     }
    // }

    [ClientRpc]
    private void StartNextCutsceneClientRpc(int idx)
    {
        if (directors == null || directors.Length == 0)
        {
            Debug.LogError("[CutsceneTimelineManager] Directors array is empty!");
            return;
        }

        if (idx < 0 || idx >= directors.Length)
        {
            Debug.Log($"[CutsceneTimelineManager] Cutscene selesai. Lanjut ke GameplayScene.");
            if (IsHost)
                SceneFlowManager.Instance.ChangeScene("FirstFloor");
            return;
        }

        index = idx;
        PlayCutsceneLocal();
    }



    void PlayCutsceneLocal()
    {
        var d = directors[index];
        localDone = false;

        // Pastikan punya CutsceneController
        var ctrl = d.GetComponent<CutsceneController>();
        if (ctrl != null)
        {
            ctrl.Play(OnCutsceneEnd);
        }
        else
        {
            Debug.LogWarning($"[CutsceneTimelineManager] Director {d.name} tanpa CutsceneController, memutar langsung.");
            d.stopped -= OnStopped;
            d.stopped += OnStopped;
            d.time = 0;
            d.Play();
        }

        if (fadeCanvas) StartCoroutine(FadeIn());
        if (skipHint) { skipHint.SetActive(false); Invoke(nameof(ShowSkip), 1f); }
    }

    private void OnCutsceneEnd()
    {
        if (localDone) return;
        localDone = true;
        ReportCutsceneDoneServerRpc(index);
    }


    void ShowSkip() => skipHint?.SetActive(true);

    private void Update()
    {
        if (Input.anyKeyDown && !localDone)
        {
            directors[index].time = directors[index].duration - 0.05;
            directors[index].Evaluate();
        }
    }

    private void OnStopped(PlayableDirector d)
    {
        if (localDone) return;
        localDone = true;
        ReportCutsceneDoneServerRpc(index);
    }

    [ServerRpc(RequireOwnership = false)]
    void ReportCutsceneDoneServerRpc(int idx)
    {
        playerReadyCount++;
        if (playerReadyCount >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            playerReadyCount = 0;
            StartNextCutsceneClientRpc(idx + 1);
        }
    }

    // fade effect opsional
    private System.Collections.IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = 1f - t / 0.5f;
            yield return null;
        }
        fadeCanvas.alpha = 0f;
    }

    private int playerReadyCount = 0;
}
