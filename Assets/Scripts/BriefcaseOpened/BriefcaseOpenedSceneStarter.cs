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
            "The lock’s finally off", SubtitleTarget.Detective
        );

        yield return SubtitleManager.Instance.ShowAndWaitRoutine(
            "Wait there's something inside", SubtitleTarget.Spirit
        );

        yield return SubtitleManager.Instance.ShowAndWaitRoutine(
            "Polaroid..? and Calendar..? Why would someone hide these?", SubtitleTarget.Spirit
        );

        // Sync: EventSystem OFF (lagi)

        yield return SubtitleManager.Instance.ShowAndWaitRoutine(
            "Whatever the reason, this might be our next clue", SubtitleTarget.Detective
        );

    }

}
