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

        // 🛑 Stop BGM segera saat masuk cutscene scene
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopBGM(true);
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
}