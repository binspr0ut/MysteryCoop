using System.Collections;
using UnityEngine;

using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class BriefcaseOpenedSceneStarter : NetworkBehaviour
{
    private void Start()
    {
        // only host starts sequence
        if (NetworkManager.Singleton.IsHost)
            StartCoroutine(SceneStart());
    }

    private IEnumerator SceneStart()
    {
        yield return new WaitForSeconds(0.2f);

        // Sync: EventSystem OFF
        yield return SubtitleManager.Instance.ShowAndWaitRoutine(
            "Kopernya kebuka juga.", SubtitleTarget.Detective
        );

        yield return SubtitleManager.Instance.ShowAndWaitRoutine(
            "iya ada polaroid", SubtitleTarget.Spirit
        );

        // Sync: EventSystem OFF (lagi)

        yield return SubtitleManager.Instance.ShowAndWaitRoutine(
            "Coba berantakin kali ya.", SubtitleTarget.Detective
        );

    }

}
