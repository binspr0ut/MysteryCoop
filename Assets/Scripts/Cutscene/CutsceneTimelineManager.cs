using UnityEngine;
using Unity.Netcode;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // jika kamu mau mengubah teks hint

public class CutsceneTimelineManager : NetworkBehaviour
{

    public PlayableDirector[] directors;

    private int index = 0;
    private bool isPlaying = false;
    private bool hasReported = false;
    private bool skipArmed = false;


    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f); // beri waktu agar semua client siap
        StartCoroutine(PlayCutsceneSequence());
    }

    private IEnumerator PlayCutsceneSequence()
    {
        if (isPlaying) yield break;
        isPlaying = true;

        for (index = 0; index < directors.Length; index++)
        {
            Debug.Log("================Superman===================");

            var d = directors[index];

            // 🟢 Tambahkan baris ini
            var controller = d.GetComponent<CutsceneController>();
            if (controller != null)
                controller.Play(() => { }); // agar VideoPlayer.Play() ikut jalan

            skipArmed = false; // reset sebelum tiap director

            d.Play(); // 🎬 mainkan timeline seperti biasa

            while (d.state == PlayState.Playing)
            {
                if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                {
                    if (!skipArmed)
                    {
                        skipArmed = true;
                        // TAP 1 → langsung loncat ke akhir timeline ini
                        d.time = d.duration;
                        d.Evaluate();
                        d.Stop();
                        Debug.Log($"[Cutscene] Skipped segment {index + 1}");
                    }
                    else
                    {
                        // TAP 2 → langsung akhiri seluruh sequence
                        Debug.Log("[Cutscene] Force end entire cutscene");
                        index = directors.Length;
                        break;
                    }
                }

                yield return null;
            }
            controller.RigelStop();
        }

        // selesai semua director
        yield return new WaitForSeconds(0.5f);
        ReportDoneToManager();
    }


    private void ReportDoneToManager()
    {
        if (hasReported) return;
        hasReported = true;

        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.ReportCutsceneDoneServerRpc();
            Debug.Log("[IntroCutsceneManager] Reported done to SceneFlowManager");
        }
        else
        {
            Debug.LogWarning("[IntroCutsceneManager] SceneFlowManager not found!");
        }
    }
    // [Header("Timeline References")]
    // public CutsceneController[] cutscenes;
    // public CanvasGroup fadeCanvas;
    // public GameObject skipHint;
    // public string nextSceneName = "";

    // private int currentIndex = 0;
    // private bool localPlaying = false;
    // private bool localReportedDone = false;

    // private bool localSkipArmed = false;   // <-- TAP #1 sudah dilakukan?
    // private CutsceneController localController;
    // private GameObject spawnedLocal;

    // // ====== server state (unchanged) ======
    // private NetworkVariable<int> readyCount = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // private NetworkVariable<int> doneCount = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // private NetworkVariable<int> indexSync = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // private NetworkVariable<bool> sequenceRunning = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // [ServerRpc(RequireOwnership = false)]
    // public void ClientReadyForCutsceneServerRpc()
    // {
    //     if (!IsServer) return;
    //     readyCount.Value = Mathf.Min(2, readyCount.Value + 1);
    //     if (readyCount.Value >= 2 && !sequenceRunning.Value)
    //     {
    //         sequenceRunning.Value = true;
    //         indexSync.Value = 0;
    //         doneCount.Value = 0;
    //         StartTimelineClientRpc(indexSync.Value);
    //     }
    // }

    // [ClientRpc]
    // private void StartTimelineClientRpc(int index)
    // {
    //     if (index < 0 || index >= cutscenes.Length) { Debug.LogWarning("Index cutscene invalid"); return; }

    //     HideAllSubtitles();

    //     if (spawnedLocal) Destroy(spawnedLocal);
    //     localController = Instantiate(cutscenes[index]);
    //     spawnedLocal = localController.gameObject;

    //     currentIndex = index;
    //     localReportedDone = false;
    //     localSkipArmed = false;          // <-- reset dua-tap tiap mulai scene
    //     localPlaying = true;

    //     if (skipHint) { skipHint.SetActive(false); SetSkipHintText("Tap to Skip"); }
    //     FadeTo(0f, 0.3f);

    //     localController.Play(OnTimelineEnded);  // callback selesai
    //     Invoke(nameof(ShowSkipHint), 1.0f);
    // }

    // void ShowSkipHint()
    // {
    //     if (localPlaying && skipHint) skipHint.SetActive(true);
    // }

    // // ====== SATU tempat input untuk host & client ======
    // void Update()
    // {
    //     if (!localPlaying) return;

    //     bool pressed = Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.touchCount > 0;
    //     if (!pressed) return;

    //     // TAP #1: loncat ke akhir lokal (tanpa report)
    //     if (!localSkipArmed)
    //     {
    //         localSkipArmed = true;
    //         localController?.SkipToEnd();
    //         SetSkipHintText("Tap to Continue"); // feedback visual
    //         return;
    //     }

    //     // TAP #2: lapor selesai ke server (host & client sama-sama harus tap)
    //     if (!localReportedDone)
    //     {
    //         localReportedDone = true;
    //         ReportDoneServerRpc(indexSync.Value);
    //     }
    // }

    // private void OnTimelineEnded()
    // {
    //     // Cutscene benar-benar selesai (oleh waktu normal ATAU setelah SkipToEnd)
    //     FadeTo(1f, 0.2f);
    //     // biarkan tombol menunggu TAP #2 agar advance (server butuh 2 laporan)
    //     // tidak apa-apa kalau pemain belum tap kedua—mereka “parkir” di frame akhir.
    // }

    // [ServerRpc(RequireOwnership = false)]
    // void ReportDoneServerRpc(int index)
    // {
    //     if (!IsServer) return;
    //     if (index != indexSync.Value) return; // cegah race

    //     doneCount.Value = Mathf.Clamp(doneCount.Value + 1, 0, 2);
    //     if (doneCount.Value >= 2)
    //     {
    //         doneCount.Value = 0;
    //         int next = index + 1;
    //         if (next < cutscenes.Length)
    //         {
    //             indexSync.Value = next;
    //             StartTimelineClientRpc(next);
    //         }
    //         else
    //         {
    //             sequenceRunning.Value = false;
    //             EndSequenceClientRpc();
    //         }
    //     }
    // }

    // [ClientRpc]
    // void EndSequenceClientRpc()
    // {
    //     if (skipHint) skipHint.SetActive(false);
    //     FadeTo(0f, 0.3f);

    //     if (!string.IsNullOrEmpty(nextSceneName))
    //     {
    //         if (IsHost) NetworkManager.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    //     }
    //     else
    //     {
    //         Debug.Log("Semua cutscene selesai!");
    //     }

    //     if (spawnedLocal) Destroy(spawnedLocal);
    //     localController = null;
    // }

    // private void FadeTo(float targetAlpha, float dur)
    // {
    //     if (!fadeCanvas) return;
    //     StopAllCoroutines();
    //     StartCoroutine(FadeCoroutine(targetAlpha, dur));
    // }

    // private System.Collections.IEnumerator FadeCoroutine(float target, float dur)
    // {
    //     float t = 0f, start = fadeCanvas.alpha;
    //     while (t < dur) { t += Time.deltaTime; fadeCanvas.alpha = Mathf.Lerp(start, target, t / dur); yield return null; }
    //     fadeCanvas.alpha = target;
    // }

    // [SerializeField] Transform subtitleRoot; // drag: CutsceneCanvas
    // private void HideAllSubtitles()
    // {
    //     if (!subtitleRoot) return;
    //     for (int i = 0; i < subtitleRoot.childCount; i++)
    //     {
    //         var go = subtitleRoot.GetChild(i).gameObject;
    //         if (go.name.StartsWith("SubtitleTMP")) go.SetActive(false);
    //     }
    // }

    // // util kecil untuk ganti tulisan hint (pakai TMP atau Text biasa)
    // private void SetSkipHintText(string s)
    // {
    //     if (!skipHint) return;
    //     var tmp = skipHint.GetComponentInChildren<TMPro.TMP_Text>();
    //     if (tmp) tmp.text = s;
    //     else
    //     {
    //         var uiText = skipHint.GetComponentInChildren<UnityEngine.UI.Text>();
    //         if (uiText) uiText.text = s;
    //     }
    // }
}